using System.Collections.Generic;

namespace LANPaint.Services.FirewallConfiguration.Netsh
{
    public interface INetshWrapper
    {
        /// <summary>
        /// Executes Netsh with passed arguments and returns resulting output as Enumerable of lines.
        /// </summary>
        /// <param name="arguments">Netsh arguments.</param>
        /// <returns>Output as Enumerable of lines.</returns>
        IEnumerable<string> ExecuteWithOutput(string arguments);

        /// <summary>
        /// Executes Netsh with passed arguments.
        /// </summary>
        /// <param name="arguments">Netsh arguments.</param>
        /// <returns>Returns Netsh exit code.</returns>
        bool Execute(string arguments);
    }
}