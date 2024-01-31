using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GPAS.FilterSearch
{
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }

        public bool SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (property != null)
            {
                if (property.Equals(value)) return false;
            }

            property = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
