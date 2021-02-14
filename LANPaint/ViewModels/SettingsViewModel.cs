﻿using LANPaint.Dialogs.CustomDialogs;
using LANPaint.MVVM;
using LANPaint.Services.Network;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Windows.Threading;

namespace LANPaint.ViewModels
{
    public class SettingsViewModel : CustomDialogViewModelBase<IPEndPoint>
    {
#warning Change this two constants to Range?
        private const int PortMinValue = 1024;
        private const int PortMaxValue = 65535;
        private readonly Dispatcher _dispatcher;
        private NetworkInterfaceUiInfo _selectedNetworkInterfaceUiInfo;
        private int _port;
        private bool _isPortValid;
        private readonly NetworkInterfaceHelper _networkInterfaceHelper;

        public NetworkInterfaceUiInfo SelectedNetworkInterfaceUiInfo
        {
            get => _selectedNetworkInterfaceUiInfo;
            set
            {
                if (!Interfaces.Contains(value)) return;
                _selectedNetworkInterfaceUiInfo = value;
                OkCommand.RaiseCanExecuteChanged();
            }
        }
        public int Port
        {
            get => _port;
            set
            {
#warning Do we really need to notify listeners about change of this property?
                if (!SetProperty(ref _port, value)) return;
                IsPortValid = Enumerable.Range(PortMinValue, PortMaxValue - PortMinValue + 1).Contains(value);
                OkCommand.RaiseCanExecuteChanged();
            }
        }
        public bool IsPortValid
        {
            get => _isPortValid;
            private set => SetProperty(ref _isPortValid, value);
        }
        public ObservableCollection<NetworkInterfaceUiInfo> Interfaces { get; }

        public RelayCommand<IDialogWindow> OkCommand { get; }
        public RelayCommand<IDialogWindow> CancelCommand { get; }

        public SettingsViewModel() : base("Settings")
        {
            Interfaces = new ObservableCollection<NetworkInterfaceUiInfo>();
            _networkInterfaceHelper = NetworkInterfaceHelper.GetInstance();
            _dispatcher = Dispatcher.CurrentDispatcher;
            UpdateInterfaceCollection();
            _networkInterfaceHelper.Interfaces.CollectionChanged += CollectionChangedHandler;

            OkCommand = new RelayCommand<IDialogWindow>(OnOkCommand,
                () => Enumerable.Range(PortMinValue, PortMaxValue - PortMinValue + 1).Contains(Port) &&
                      SelectedNetworkInterfaceUiInfo != null &&
                      SelectedNetworkInterfaceUiInfo.IsReadyToUse);
            CancelCommand = new RelayCommand<IDialogWindow>(OnCancelCommand);
            OkCommand.RaiseCanExecuteChanged();
        }

        public SettingsViewModel(IPAddress ipAddress, int port) : this()
        {
            SelectedNetworkInterfaceUiInfo = Interfaces.FirstOrDefault(ni => ni.IpAddress.Equals(ipAddress));
            Port = port;
            OkCommand.RaiseCanExecuteChanged();
        }

        private void OnOkCommand(IDialogWindow window)
        {
            var result = new IPEndPoint(SelectedNetworkInterfaceUiInfo.IpAddress, Port);
            CloseDialogWithResult(window, true, result);
        }

        private void OnCancelCommand(IDialogWindow window) => CloseDialogWithResult(window);

        private void UpdateInterfaceCollection()
        {
            var interfaces = _networkInterfaceHelper.Interfaces.Select(nic => new NetworkInterfaceUiInfo()
            {
                Name = nic.Name,
                IpAddress = _networkInterfaceHelper.GetIpAddress(nic),
                IsReadyToUse = _networkInterfaceHelper.IsReadyToUse(nic)
            }).ToList();

            if (Dispatcher.CurrentDispatcher == _dispatcher)
            {
                Interfaces.Clear();
                interfaces.ForEach(nicInfo => Interfaces.Add(nicInfo));
            }
            else
            {
                _dispatcher.Invoke(() =>
                {
                    Interfaces.Clear();
                    interfaces.ForEach(nicInfo => Interfaces.Add(nicInfo));
                });
            }
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e) => UpdateInterfaceCollection();
    }

    public class NetworkInterfaceUiInfo
    {
        public string Name { get; set; }
        public IPAddress IpAddress { get; set; }
        public bool IsReadyToUse { get; set; }
    }
}