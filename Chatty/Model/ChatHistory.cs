using Chatty.Model.INotify;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace Chatty.Model
{
    public class ChatHistory : PropChangeObservableCollection<Message>
    {
        public bool IsGroup { get; set; }
        public string Identifier { get; set; }

        internal void PushMessage(string message, string username, long timeStamp) {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                this.Add(new Message() { MessageString = message, Username = username, TimeStamp = timeStamp });
            }));
        }

        internal void PushMessage(List<Message> messages, string username) {
            if(messages != null && messages.Count > 0) 
                messages.ForEach(message => { PushMessage(message.MessageString, username, message.TimeStamp); });
        }
    }
}