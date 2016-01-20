using Chatty.Model;
using System.Linq;
using System;

namespace Chatty
{
    class ChattyClient {
        public IMessageListener Listener { get; protected set; }

        public void SendMessage(string identifier, string message) {
            Listener.SendMessage(identifier, message);
        }

        public void SendGroupMessage(Group group, string message) {
            Listener.SendMessage(group.ClientList.Select(client => client.PublicKeyHash).ToList() , message);
        }

        internal void ConfirmUser(string identifier) {
            Listener.ConfirmUser(identifier);
        }

        internal void SearchUser(string userName) {
            Listener.SearchUser(userName);
        }

        internal void Register(string userName, string publicKey) {
            Listener.Register(userName, publicKey);
        }
        
        public void SetMessageListener(IMessageListener listener) {
            Listener = listener;
        }
    }
}