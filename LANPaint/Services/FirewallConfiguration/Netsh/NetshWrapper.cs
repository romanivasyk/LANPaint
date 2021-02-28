using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LANPaint.Services.FirewallConfiguration.Netsh
{
    public class NetshWrapper : INetshWrapper
    {
        private readonly ProcessStartInfo _processStartInfo;

        public NetshWrapper()
        {
            _processStartInfo = new ProcessStartInfo("netsh.exe")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        public NetshWrapper(ProcessStartInfo startInfo)
        {
            _processStartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));
        }
        
        ///<inheritdoc/>
        public IEnumerable<string> ExecuteWithOutput(string arguments)
        {
            using var process = new Process {StartInfo = _processStartInfo};
            process.Start();
            process.StandardInput.WriteLine(arguments);
            process.StandardInput.Flush();
            process.StandardInput.Close();

            var output = new List<string>();
            while (!process.StandardOutput.EndOfStream)
            {
                output.Add(process.StandardOutput.ReadLine());
            }

            process.WaitForExit();

            return output;
        }

        ///<inheritdoc/>
        public bool Execute(string arguments)
        {
            using var process = new Process {StartInfo = _processStartInfo};
            process.Start();
            process.StandardInput.WriteLine(arguments);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.StandardOutput.Close();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
    }
}