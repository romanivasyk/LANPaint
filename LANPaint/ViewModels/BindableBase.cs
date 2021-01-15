using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LANPaint.ViewModels
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (storage != null && !storage.Equals(value) || value != null)
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
