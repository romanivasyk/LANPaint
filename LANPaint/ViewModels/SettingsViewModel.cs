using LANPaint.Dialogs.Service;
using LANPaint.Services.Network;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Threading;

namespace LANPaint.ViewModels
{
    public class SettingsViewModel : DialogViewModelBase<IPAddress>, IDisposable
    {
        private readonly Dispatcher _dispatcher;
        private NetworkInterfaceUiInfo _selectedNetworkInterfaceUiInfo;

        public NetworkInterfaceUiInfo SelectedNetworkInterfaceUiInfo
        {
            get => _selectedNetworkInterfaceUiInfo;
            set
            {
                if (!Interfaces.Contains(value)) return;
                _selectedNetworkInterfaceUiInfo = value;
            }
        }
        public int Port { get; set; }
        public ObservableCollection<NetworkInterfaceUiInfo> Interfaces { get; }
        public RelayCommand<IDialogWindow> OkCommand { get; }
        public RelayCommand<IDialogWindow> CancelCommand { get; }

        public SettingsViewModel() : base("Settings")
        {
            Interfaces = new ObservableCollection<NetworkInterfaceUiInfo>();
            _dispatcher = Dispatcher.CurrentDispatcher;
            UpdateInterfaceCollection();

            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChangedHandler;
            NetworkChange.NetworkAddressChanged += NetworkAddressChangedHandler;
            OkCommand = new RelayCommand<IDialogWindow>(OnOkCommand);
            CancelCommand = new RelayCommand<IDialogWindow>(OnCancelCommand);
            DialogResult = IPAddress.None;
        }

        public SettingsViewModel(IPAddress ipAddress, int port) : this()
        {
            SelectedNetworkInterfaceUiInfo = Interfaces.FirstOrDefault(ni => ni.IpAddress.Equals(ipAddress));
            Port = port;
        }

        private void OnOkCommand(IDialogWindow window)
        {
            var result = SelectedNetworkInterfaceUiInfo?.IpAddress ?? IPAddress.None;
            CloseDialogWithResult(window, result);
        }

        private void OnCancelCommand(IDialogWindow window)
        {
            CloseDialogWithResult(window, IPAddress.None);
        }

        private void UpdateInterfaceCollection()
        {
            var helper = new NetworkInterfaceHelper();

            var interfaces = helper.GetIPv4Interfaces().Select(nic => new NetworkInterfaceUiInfo()
            {
                Name = nic.Name,
                IpAddress = helper.GetIpAddress(nic),
                IsReadyToUse = helper.IsReadyToUse(nic)
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

        private void NetworkAddressChangedHandler(object sender, EventArgs e) => UpdateInterfaceCollection();
        private void NetworkAvailabilityChangedHandler(object sender, NetworkAvailabilityEventArgs e) => UpdateInterfaceCollection();

        public void Dispose()
        {
            NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChangedHandler;
            NetworkChange.NetworkAddressChanged -= NetworkAddressChangedHandler;
        }
    }

    public class NetworkInterfaceUiInfo
    {
        public string Name { get; set; }
        public IPAddress IpAddress { get; set; }
        public bool IsReadyToUse { get; set; }
    }
}
