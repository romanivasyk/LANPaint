using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace LANPaint.Dialogs.Service;

public class OwnerWindowLocator
{
    public static IEnumerable<Window> ApplicationWindows => Application.Current.Windows.Cast<object>().Cast<Window>();

    public static Window GetWindowByBindableDataContext(INotifyPropertyChanged viewModel)
    {
        return ApplicationWindows
            .FirstOrDefault(window => ReferenceEquals(window.DataContext, viewModel));
    }
}