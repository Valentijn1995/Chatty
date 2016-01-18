using Chatty.Model;
using System;

namespace Chatty
{
    class ChattyClient {
        public IMessageListener Listener { get; protected set; }

        public void SendMessage(Client client, string message) {
            Listener.SendMessage(client.PublicKey, message);
        }

        public void SendGroupMessage(Group group, string message) {
            Listener.SendGroupMessage(group.GroupId, message);
        }

        public void SetMessageListener(IMessageListener listener) {
            Listener = listener;
        }

        private string EncryptMessage(Client reciepent, string message) {
            throw new NotImplementedException();
        }

        private string EncryptGroupMessage(Group reciepent, string message) {
            throw new NotImplementedException();
        }
    }
}