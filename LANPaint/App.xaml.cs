using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LANPaint.Views;

namespace LANPaint
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string[] Args { get; private set; }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Args = e.Args;
        }
    }
}
