using System;
using System.Windows;
using LANPaint.Dialogs.Service;
using LANPaint.Services.FirewallConfiguration;
using LANPaint.Services.FirewallConfiguration.Netsh;

namespace LANPaint
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var firewallConfiguration = new FirewallConfigurationService(new NetshWrapper());
            string message = null;
            if (!firewallConfiguration.IsConfigured())
            {
                message = firewallConfiguration.Configure()
                    ? string.Empty
                    : "Firewall Rules is not configured. Network functionality may not work properly.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message, "Firewall Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}