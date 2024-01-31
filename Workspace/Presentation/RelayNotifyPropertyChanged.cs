using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GPAS.Workspace.Presentation
{
    public class RelayNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void NotifyPropertyChanged<T>(T oldvalue, T newvalue, [CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(this, new PropertyChangedExtendedEventArgs<T>(oldvalue, newvalue, propertyName));
        }

        public bool SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            if (property == null && value == null)
                return false;

            if (property != null)
            {
                if (property.Equals(value)) return false;
            }

            T oldValue = property;
            property = value;
            NotifyPropertyChanged(oldValue, value, propertyName);
            return true;
        }
    }
}
