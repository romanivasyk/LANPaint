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
        private Window _mainWindow;
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _mainWindow = new MainWindow(e.Args[0]);
            _mainWindow.Show();
        }
    }
}
