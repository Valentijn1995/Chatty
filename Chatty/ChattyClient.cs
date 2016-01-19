using Chatty.Model;
using System.Linq;

namespace Chatty
{
    class ChattyClient {
        public IMessageListener Listener { get; protected set; }

        public void SendMessage(Client client, string message) {
            Listener.SendMessage(client.PublicKey, message);
        }

        public void SendGroupMessage(Group group, string message) {
            Listener.SendMessage(group.ClientList.Select(client => client.PublicKeyHash).ToList() , message);
        }

        internal void ConfirmUser(string identifier) {
            Listener.ConfirmUser(identifier);
        }

        public void SetMessageListener(IMessageListener listener) {
            Listener = listener;
        }
    }
}