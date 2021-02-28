using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using LANPaint.Services.FirewallConfiguration.EntryParser;
using LANPaint.Services.FirewallConfiguration.Model;
using LANPaint.Services.FirewallConfiguration.Netsh;
using Action = LANPaint.Services.FirewallConfiguration.Model.Action;

namespace LANPaint.Services.FirewallConfiguration
{
    public class FirewallConfigurationService
    {
        private readonly INetshWrapper _netshWrapper;
        private readonly string _inboundRuleArgument;
        private readonly string _outboundRuleArgument;
        private readonly string _checkExistingRulesArgument;
        private readonly string _currentExecutable;

        public FirewallConfigurationService(INetshWrapper netshWrapper)
        {
            _netshWrapper = netshWrapper ?? throw new ArgumentNullException(nameof(netshWrapper));
            _currentExecutable = GetExecutablePath();
            _inboundRuleArgument =
                $"advfirewall firewall add rule name=LANPaint dir=in action=allow program=\"{_currentExecutable}\" profile=any localip=Any " +
                $"remoteip=localsubnet protocol=UDP interfacetype=any";
            _outboundRuleArgument =
                $"advfirewall firewall add rule name=LANPaint dir=out action=allow program=\"{_currentExecutable}\" profile=any localip=Any " +
                $"remoteip=localsubnet protocol=UDP interfacetype=any";
            _checkExistingRulesArgument = "advfirewall firewall show rule name=LANPaint verbose";
        }

        public bool Configure()
        {
            return _netshWrapper.Execute(_inboundRuleArgument) && _netshWrapper.Execute(_outboundRuleArgument);
        }

        public bool IsConfigured()
        {
            var output = _netshWrapper.ExecuteWithOutput(_checkExistingRulesArgument);
            var firewallEntryParser = new FirewallEntryParser();
            var firewallEntries = firewallEntryParser.ParseVerbose(output.ToList());

            return firewallEntries.Where(entry => entry.Direction == Direction.In).Any(entry =>
                       entry.Enabled == true &&
                       entry.Action == Action.Allow || entry.Action == Action.Bypass &&
                       string.Equals(entry.Program.ToUpper(), _currentExecutable.ToUpper()) &&
                       entry.Protocol == ProtocolType.Udp && entry.Profile.Contains(Profile.Domain) ||
                       (entry.Profile.Contains(Profile.Private) && entry.Profile.Contains(Profile.Public)) &&
                       string.Equals(entry.LocalIp, "Any", StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(entry.RemoteIp, "LocalSubnet", StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(entry.LocalPort, "Any", StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(entry.RemotePort, "Any", StringComparison.InvariantCultureIgnoreCase))
                   &&
                   firewallEntries.Where(entry => entry.Direction == Direction.Out).Any(entry =>
                       entry.Enabled == true &&
                       entry.Action == Action.Allow || entry.Action == Action.Bypass &&
                       string.Equals(entry.Program.ToUpper(), _currentExecutable.ToUpper()) &&
                       entry.Protocol == ProtocolType.Udp && entry.Profile.Contains(Profile.Domain) ||
                       (entry.Profile.Contains(Profile.Private) && entry.Profile.Contains(Profile.Public)) &&
                       string.Equals(entry.LocalIp, "Any", StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(entry.RemoteIp, "LocalSubnet", StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(entry.LocalPort, "Any", StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(entry.RemotePort, "Any", StringComparison.InvariantCultureIgnoreCase));
        }

        private static string GetExecutablePath()
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var executable = Assembly.GetExecutingAssembly().GetName().Name + ".exe";
            return Path.Combine(assemblyDirectory, executable);
        }
    }
}