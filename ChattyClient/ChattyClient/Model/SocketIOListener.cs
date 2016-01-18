using System;

namespace ChattyClient.Model {
    class SocketIOListener : IMessageListener {
        public void OnMessageReceived(string clientId, string message) {
            throw new NotImplementedException();
        }

        public void OnGroupMessageReceived(string groupId, string message) {
            throw new NotImplementedException();
        }
    }
}
