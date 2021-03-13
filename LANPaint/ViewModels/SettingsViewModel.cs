using System;
using LANPaint.Dialogs.CustomDialogs;
using LANPaint.MVVM;
using LANPaint.Services.Network;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using LANPaint.Services.Network.Utilities;
using LANPaint.Services.Network.Watchers;

namespace LANPaint.ViewModels
{
    public class SettingsViewModel : CustomDialogViewModelBase<IPEndPoint>
    {
        private const int PortMinValue = 1024;
        private const int PortMaxValue = 65535;
        private NetworkInterfaceUiInfo _selectedNetworkInterfaceUiInfo;
        private int _port;
        private bool _isPortValid;
        private readonly INetworkWatcher _networkWatcher;
        private readonly INetworkUtility _networkUtility;

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
                _port = value;
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

        public SettingsViewModel(INetworkWatcher networkWatcher, INetworkUtility networkUtility) : base("Settings")
        {
            Interfaces = new ObservableCollection<NetworkInterfaceUiInfo>();
            _networkWatcher = networkWatcher ?? throw new ArgumentNullException(nameof(networkWatcher));
            _networkUtility = networkUtility ?? throw new ArgumentNullException(nameof(networkUtility));

            UpdateInterfaceCollection();
            _networkWatcher.NetworkStateChanged += NetworkStateChangedHandler;

            OkCommand = new RelayCommand<IDialogWindow>(OnOkCommand,
                () => Enumerable.Range(PortMinValue, PortMaxValue - PortMinValue + 1).Contains(Port) &&
                      SelectedNetworkInterfaceUiInfo != null &&
                      SelectedNetworkInterfaceUiInfo.IsReadyToUse);
            CancelCommand = new RelayCommand<IDialogWindow>(OnCancelCommand);
            OkCommand.RaiseCanExecuteChanged();
        }

        public SettingsViewModel(INetworkWatcher networkWatcher, INetworkUtility networkUtility, IPAddress ipAddress, int port) :
            this(networkWatcher, networkUtility)
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
            var interfaces = _networkWatcher.Interfaces.Select(nic => new NetworkInterfaceUiInfo()
            {
                Name = nic.Name,
                IpAddress = _networkUtility.GetIpAddress(nic),
                IsReadyToUse = _networkUtility.IsReadyToUse(nic)
            }).ToList();

            Interfaces.Clear();
            interfaces.ForEach(nicInfo => Interfaces.Add(nicInfo));
        }

        private void NetworkStateChangedHandler(object sender, EventArgs e) => UpdateInterfaceCollection();
    }

    public class NetworkInterfaceUiInfo
    {
        public string Name { get; set; }
        public IPAddress IpAddress { get; set; }
        public bool IsReadyToUse { get; set; }
    }
}