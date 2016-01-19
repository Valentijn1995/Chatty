using Chatty.Model;
using System;
using System.Collections.Generic;

namespace Chatty
{
    interface IMessageListener {
        event EventHandler<UserSearchEventArgs> OnUserSearch;
        event EventHandler<UserComfirmEventArgs> OnUserConfirm;
        event EventHandler<GroupJoinedEventArgs> OnGroupJoined;
        event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        void SendMessage(Client client, string message);
        void SendMessage(string identifier, string message);
        void SendMessage(List<Client> clients, string message);
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public long TimeStamp { get; set; }
        public string Identifier { get; set; }
    }

    public class UserSearchEventArgs : EventArgs {
        public List<Client> FoundMembers { get; set; }
    }

    public class UserComfirmEventArgs : EventArgs {
        public string PublicKey { get; set; }
    }

    public class GroupJoinedEventArgs : EventArgs {
        public string GroupName { get; set; }
        public List<Client> Members { get; set; }
    }
}