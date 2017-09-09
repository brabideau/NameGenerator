using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameGenerator
{
    public class NameOrigin: INotifyPropertyChanged
    {
        public string PlaceName { get; set; }
        public string Surnames { get; set; }
        public string U_Names { get; set; }
        public string F_Names { get; set; }
        public string M_Names { get; set; }
        public bool IsChecked { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
