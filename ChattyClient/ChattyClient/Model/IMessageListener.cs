using ChattyClient.Model;

namespace ChattyClient {
    interface IMessageListener {
        void OnMessageReceived(string clientId, string message);
        void OnGroupMessageReceived(string clientId, string message);
    }
}