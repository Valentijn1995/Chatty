using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chatty.Model.INotify
{
    public class Message : INotifyPropertyChanged
    {
        public string Username { get; set; }
        public string MessageString { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}