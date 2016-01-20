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
        
        void ConfirmUser(string identifier);
        void SendMessage(string identifier, string message);
        void SendMessage(List<string> receivers, string message);
        void Register(string userName, string publicKey);
        void SearchUser(string userName);
        void CreateGroup(string groupName, List<Client> members);
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public long TimeStamp { get; set; }
        public string Identifier { get; set; }
        public bool GroupMessage { get; set; }
    }

    public class UserSearchEventArgs : EventArgs {
        public List<Client> FoundMembers { get; set; }
    }

    public class UserComfirmEventArgs : EventArgs {
        public string UserName { get; set; }
        public string PublicKey { get; set; }
    }

    public class GroupJoinedEventArgs : EventArgs {
        public string GroupName { get; set; }
        public string GroupHash { get; set; }
        public List<Client> Members { get; set; }
    }
}