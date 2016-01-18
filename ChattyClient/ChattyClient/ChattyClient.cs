using ChattyClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattyClient {
    class ChattyClient {
        IMessageListener listener = new SocketIOListener();

        public void SendMessage(Client client, string message) {
        }

        public void SendGroupMessage(Group group, string message) {

        }

        public void SetMessageListener(IMessageListener listener) {

        }

        private string EncryptMessage(Client reciepent, string message) {
            throw new NotImplementedException();
        }

        private string EncryptGroupMessage(Group reciepent, string message) {
            throw new NotImplementedException();
        }
    }
}
