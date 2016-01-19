using Chatty.Model;
using Chatty.Model.INotify;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Chatty
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChattyClient _client;
        private UserManager _manager;
        private Dictionary<string, List<Message>> _openMessages;

        public MainWindow() {
            InitializeComponent();

            _client = new ChattyClient();
            _client.SetMessageListener(new SocketIOListener("https://localhost:2000"));
            _client.Listener.OnMessageReceived += Listener_OnMessageReceived;
            _client.Listener.OnUserConfirm += Listener_OnUserConfirm;
            _client.Listener.OnGroupJoined += Listener_OnGroupJoined;

            _manager = new UserManager();
        }

        private void Listener_OnGroupJoined(object sender, GroupJoinedEventArgs e) {
            var group = new Group() { Identifier = e.GroupName, ClientList = e.Members };
            _manager.AddGroup(group);
            listView_Clients.Items.Add(new ChatItem() { Identifier = group.Identifier, Value = group.GroupName });
        }

        private void Listener_OnUserConfirm(object sender, UserComfirmEventArgs e) {
            var client = new Client() { PublicKey = e.PublicKey, UserName = e.UserName };      //TODO Change server info
            _manager.AddClient(client);
            listView_Clients.Items.Add(new ChatItem() { Identifier = client.PublicKeyHash, Value = client.UserName });
            _manager.GetChatHistory(client.PublicKeyHash).PushMessage(RetrieveMessages(client.PublicKeyHash), client.UserName);
        }

        private void Listener_OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            if(e.GroupMessage && _manager.GetGroup(e.Identifier) != null) {
            }
            else {
                if(_manager.GetClient(e.Identifier) == null) {
                    _client.ConfirmUser(e.Identifier);
                    SaveMessage(e.Identifier, e.Message, e.TimeStamp);
                }
                else {
                    Client client = _manager.GetClient(e.Identifier);
                    _manager.GetChatHistory(e.Identifier).PushMessage(e.Message, client.UserName, e.TimeStamp);
                }
            }
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e) {
            string message = Textbox_Message.Text;
            if(IsCurrentChatGroup()) {
                _client.SendGroupMessage(null, message);
            }
            else {
                _client.SendMessage(null, message);     //TODO Null should be the hash of the receivant
            }
            (listView_Chat.ItemsSource as ChatHistory).PushMessage(message, "Me", DateTime.Now);
        }
        
        private void Highlight(string identifier, bool value) {
            foreach(object item in listView_Clients.Items) {
                if((item as ChatItem).Identifier == identifier) {
                    (item as ChatItem).Highlighted = value;
                }
            }
        }

        private bool IsCurrentChatGroup() {
            if(listView_Chat.ItemsSource != null) {
                return (listView_Chat.ItemsSource as ChatHistory).IsGroup;
            }
            return false;
        }
        
        private void SaveMessage(string identifier, string message, long timeStamp) {
            if(_openMessages == null)
                _openMessages = new Dictionary<string, List<Message>>();

            if(_openMessages.ContainsKey(identifier)) {
                _openMessages[identifier].Add(new Message() { MessageString = message, TimeStamp = new DateTime(timeStamp) });
            }
            else {
                _openMessages.Add(identifier, new List<Message>() { new Message() { MessageString = message, TimeStamp = new DateTime(timeStamp) } });
            }
        }

        private List<Message> RetrieveMessages(string identifier) {
            if(_openMessages != null && _openMessages.ContainsKey(identifier))
                return _openMessages[identifier];

            return null;
        }
    }
}
