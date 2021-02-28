using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using LANPaint.Services.FirewallConfiguration.Model;
using Action = LANPaint.Services.FirewallConfiguration.Model.Action;

namespace LANPaint.Services.FirewallConfiguration.EntryParser
{
    public class FirewallEntryParser : IFirewallEntryParser
    {
        public IEnumerable<FirewallEntry> Parse(List<string> lines)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FirewallEntry> ParseVerbose(List<string> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            var entries = new List<FirewallEntry>();
            var linesToSkip = 0;
            while (true)
            {
                var entryLines = lines.Skip(linesToSkip)
                    .SkipWhile(line => !(line.Contains("Rule Name:") && line.Contains("LANPaint")))
                    .TakeWhile(line => line != string.Empty).ToList();
                linesToSkip += entryLines.Count;
                if (entryLines.Count == 0) break;
                entries.Add(ParseVerboseEntry(entryLines));
            }

            return entries;
        }

        private FirewallEntry ParseVerboseEntry(IEnumerable<string> entriesAsLines)
        {
            if (entriesAsLines == null) throw new ArgumentNullException(nameof(entriesAsLines));
            var entry = new FirewallEntry();
            foreach (var line in entriesAsLines)
            {
                var key = string.Concat(line.TakeWhile(ch => ch != ':'));
                switch (key.ToUpperInvariant())
                {
                    case "ENABLED":
                    {
                        entry.Enabled = line.Contains("Yes");
                        break;
                    }
                    case "DIRECTION":
                    {
                        entry.Direction = Enum.Parse<Direction>(string.Concat(line.SkipWhile(ch => ch != ':').Skip(1)
                            .SkipWhile(ch => ch == ' ')));
                        break;
                    }
                    case "PROFILES":
                    {
                        entry.Profile = string
                            .Concat(line.SkipWhile(ch => ch != ':').Skip(1).SkipWhile(ch => ch == ' ')).Split(',')
                            .Select(Enum.Parse<Profile>).ToList();
                        break;
                    }
                    case "LOCALIP":
                    {
                        entry.LocalIp = string.Concat(line.SkipWhile(ch => ch != ':').Skip(1).SkipWhile(ch => ch == ' '));
                        break;
                    }
                    case "REMOTEIP":
                    {
                        entry.RemoteIp = string.Concat(line.SkipWhile(ch => ch != ':').Skip(1).SkipWhile(ch => ch == ' '));
                        break;
                    }
                    case "PROTOCOL":
                    {
                        entry.Protocol =
                            Enum.Parse<ProtocolType>(
                                string.Concat(line.SkipWhile(ch => ch != ':').Skip(1).SkipWhile(ch => ch == ' ')), true);
                        break;
                    }
                    case "LOCALPORT":
                    {
                        entry.LocalPort = string.Concat(line.SkipWhile(ch => ch != ':').Skip(1).SkipWhile(ch => ch == ' '));
                        break;
                    }
                    case "REMOTEPORT":
                    {
                        entry.RemotePort = string.Concat(line.SkipWhile(ch => ch != ':').Skip(1).SkipWhile(ch => ch == ' '));
                        break;
                    }
                    case "PROGRAM":
                    {
                        entry.Program = string.Concat(line.SkipWhile(ch => ch != ':').Skip(1).SkipWhile(ch => ch == ' '));
                        break;
                    }
                    case "ACTION":
                    {
                        entry.Action =
                            Enum.Parse<Action>(string.Concat(line.SkipWhile(ch => ch != ':').Skip(1).SkipWhile(ch => ch == ' ')),
                                true);
                        break;
                    }
                }
            }

            return entry;
        }
    }
}