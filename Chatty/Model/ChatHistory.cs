using Chatty.Model.INotify;
using System;
using System.Collections.Generic;

namespace Chatty.Model
{
    public class ChatHistory : PropChangeObservableCollection<Message>
    {
        public bool IsGroup { get; set; }

        internal void PushMessage(string message, string username, long timeStamp) {
            PushMessage(message, username, new DateTime(timeStamp));
        }

        internal void PushMessage(string message, string username, DateTime timeStamp) {
            this.Add(new Message() { MessageString = message, Username = username, TimeStamp = timeStamp });
        }

        internal void PushMessage(List<Message> messages, string username) {
            messages.ForEach(message => { PushMessage(message.MessageString, message.Username, message.TimeStamp); });
        }
    }
}