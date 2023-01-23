using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Events.NET
{
    public class Property : INotifyPropertyChanged
    {
        public string Name = "";

        private ObservableCollection<string> values = new ObservableCollection<string>();

        public ObservableCollection<string> Values
        {
            get { return values; }
            set { values = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
