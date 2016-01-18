using Chatty.Model;
using System;
using System.Collections.Generic;

namespace Chatty
{
    interface IMessageListener {
        event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        event EventHandler<GroupMessageReceivedEventArgs> OnGroupMessageReceived;

        void SendMessage(string identifier, string message);
        void SendGroupMessage(string identifier, string message);
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string UserName { get; set; }
        public string Identifier { get; set; }
    }

    public class GroupMessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string Identifier { get; set; }
        public HashSet<Client> Clients { get; set; }
    }
}