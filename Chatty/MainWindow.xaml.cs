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
        private User _user;
        private ChattyClient _client;
        private UserManager _manager;
        private Dictionary<string, List<Message>> _openMessages;

        public MainWindow() {
            _client = new ChattyClient();
            _manager = new UserManager();

            InitializeComponent();
            
            UserWindow window = new UserWindow();
            window.ProfileSelected += OnProfileSelected;
            window.ShowDialog();
        }

        private void Connect(string adress = "http://localhost:3000") {
            _client.Initialize(adress);
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnUserConfirm += OnUserConfirm;
            _client.OnGroupJoined += OnGroupJoined;
            _client.Register(_user.UserName, _user.PublicKey);
        }

        #region Events

        private void OnGroupJoined(object sender, GroupJoinedEventArgs e) {
            var group = new Group() { GroupHash = e.GroupName, ClientList = e.Members };
            _manager.AddGroup(group);
            listView_Clients.Items.Add(new ChatItem() { Identifier = group.GroupHash, Value = group.GroupName });
        }

        private void OnUserConfirm(object sender, UserConfirmEventArgs e) {
            var client = new Client() { PublicKey = e.PublicKey, UserName = e.UserName };
            _manager.AddClient(client);
            listView_Clients.Items.Add(new ChatItem() { Identifier = client.PublicKeyHash, Value = client.UserName });
            _manager.GetChatHistory(client.PublicKeyHash).PushMessage(RetrieveMessages(client.PublicKeyHash), client.UserName);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            if (e.GroupMessage) {
                if(_manager.GetGroup(e.Identifier) != null) {                       //Known Group
                    Client client = _manager.GetClient(e.Identifier);
                    _manager.GetChatHistory(e.Identifier).PushMessage(e.Message, client.UserName, e.TimeStamp);
                }
                else {                                                              //Unkown Group
                    //TODO Get group from server
                }
            }
            else {
                if (_manager.GetClient(e.Identifier) != null) {                     //Known Client
                    Client client = _manager.GetClient(e.Identifier);
                    _manager.GetChatHistory(e.Identifier).PushMessage(e.Message, client.UserName, e.TimeStamp);
                }
                else {                                                              //Unknown Client
                    _client.ConfirmUser(e.Identifier);
                    SaveMessage(e.Identifier, e.Message, e.TimeStamp);
                }
            }
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e) {
            if (listView_Chat.ItemsSource != null) {
                string message = Textbox_Message.Text;
                if (IsCurrentChatGroup()) {
                    Group group = _manager.GetGroup((listView_Chat.ItemsSource as ChatHistory).Identifier);
                    _client.SendGroupMessage(group, message);
                }
                else {
                    _client.SendMessage((listView_Chat.ItemsSource as ChatHistory).Identifier, message);
                }
                (listView_Chat.ItemsSource as ChatHistory).PushMessage(message, _user.UserName, DateTime.Now); 
            }
        }

        private void Button_Create_Click(object sender, RoutedEventArgs e) {
            SearchWindow window = new SearchWindow(_client);
            window.GroupCreated += OnGroupJoined;
            window.PrivateChatCreated += OnUserConfirm;
            window.ShowDialog();
        }

        private void OnProfileSelected(object sender, ProfileSelectedArgs e) {
            _user = e.Profile;
            Connect();
        }

        #endregion Events

        #region GUI Helpers

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

        #endregion GUI Helpers

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