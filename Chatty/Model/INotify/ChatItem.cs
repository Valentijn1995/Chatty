using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chatty.Model.INotify
{
    public class ChatItem : INotifyPropertyChanged
    {
        public string Value { get; set; }
        public bool Highlighted { get; set; }
        public string Identifier { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
