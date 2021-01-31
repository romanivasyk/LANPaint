using LANPaint.Model;
using LANPaint.Services.Network;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LANPaint.ViewModels
{
    public class NetConfigViewModel : BindableBase, IDisposable
    {
        private NICInfo _selectedNic;

        public ObservableCollection<NICInfo> Interfaces { get; }
        public NICInfo SelectedNic
        {
            get => _selectedNic;
            set
            {
                if (!Interfaces.Contains(value)) return;
                SetProperty(ref _selectedNic, value);
            }
        }

        public NetConfigViewModel()
        {
            Interfaces = new ObservableCollection<NICInfo>();
            UpdateInterfaceCollection();
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChangedHandler;
            NetworkChange.NetworkAddressChanged += NetworkAddressChangedHandler;
        }

        private void UpdateInterfaceCollection()
        {
            var interfaces = NICHelper.GetInterfaces().Select(nic => new NICInfo()
            {
                Name = nic.Name,
                IpAddress = nic.GetIPProperties().UnicastAddresses.First(information =>
                    information.Address.AddressFamily == AddressFamily.InterNetwork).Address,
                IsReadyToUse = nic.OperationalStatus == OperationalStatus.Up
            });

            Interfaces.Clear();
            foreach (var nicInfo in interfaces)
            {
                Interfaces.Add(nicInfo);
            }
        }

        private void NetworkAddressChangedHandler(object sender, EventArgs e)
        {
            UpdateInterfaceCollection();
        }

        private void NetworkAvailabilityChangedHandler(object sender, NetworkAvailabilityEventArgs e)
        {
            UpdateInterfaceCollection();
        }

        public void Dispose()
        {
            NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChangedHandler;
            NetworkChange.NetworkAddressChanged -= NetworkAddressChangedHandler;
        }
    }
}
