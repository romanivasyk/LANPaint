using System.Collections.Generic;
using LANPaint.Services.FirewallConfiguration.Model;

namespace LANPaint.Services.FirewallConfiguration.EntryParser
{
    public interface IFirewallEntryParser
    {
        IEnumerable<FirewallEntry> Parse(List<string> lines);
        IEnumerable<FirewallEntry> ParseVerbose(List<string> lines);
    }
}