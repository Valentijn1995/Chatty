using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chatty.Model.INotify
{
    public class ChatItem : INotifyPropertyChanged
    {
        private bool _highlighted;

        public string Value { get; set; }
        public string Identifier { get; set; }
        public bool Highlighted
        {
            get { return _highlighted; }
            set
            {
                _highlighted = value;
                NotifyPropertyChanged("Highlighted");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
