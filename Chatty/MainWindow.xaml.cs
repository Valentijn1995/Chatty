using Chatty.Model;
using Chatty.Model.INotify;
using System;
using System.Windows;
using System.Windows.Threading;

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

        public MainWindow() {
            _client = new ChattyClient();
            _manager = new UserManager();

            InitializeComponent();
            
            UserWindow window = new UserWindow();
            window.ProfileSelected += (sender, e) => {
                _user = e.Profile;
                Connect();
            };
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
            Group group = new Group() { GroupHash = e.GroupName, ClientList = e.Members };
            _manager.AddGroup(group);
            OnDispatcher(new Action(() => {
                listView_Clients.Items.Add(new ChatItem() { Identifier = group.GroupHash, Value = group.GroupName });
            }));

            Highlight(e.GroupHash, true);
        }

        private void OnUserConfirm(object sender, UserConfirmEventArgs e) {
            Client client = new Client() { PublicKey = e.PublicKey, UserName = e.UserName };
            OnDispatcher(new Action(() => {
                listView_Clients.Items.Add(new ChatItem() { Identifier = client.PublicKeyHash, Value = client.UserName });
            }));
            _manager.AddClient(client);
            _manager.GetChatHistory(client.PublicKeyHash).PushMessage(_manager.RetrieveMessages(client.PublicKeyHash), client.UserName);
            Highlight(e.PublicKeyHash, true);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            e.TimeStamp = ConvertTicks(e.TimeStamp);
            e.Message = SecurityManager.DecryptText(e.Message, _user.PrivateKey);
            if (e.GroupMessage) {
                if(_manager.GetGroup(e.Identifier) != null) {                       //Known Group
                    Client client = _manager.GetClient(e.Identifier);
                    _manager.GetChatHistory(e.Identifier).PushMessage(e.Message, client.UserName, e.TimeStamp);
                }
                else {                                                              //Unkown Group
                    //TODO Get group from server or is this already done on connect?
                }
            }
            else {
                if (_manager.GetClient(e.Identifier) != null) {                     //Known Client
                    Client client = _manager.GetClient(e.Identifier);
                    _manager.GetChatHistory(e.Identifier).PushMessage(e.Message, client.UserName, e.TimeStamp);
                }
                else {                                                              //Unknown Client
                    _client.ConfirmUser(e.Identifier);
                    _manager.SaveMessage(e.Identifier, e.Message, e.TimeStamp);
                }
            }
            Highlight(e.Identifier, true);
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e) {
            string message = Textbox_Message.Text;
            if (listView_Chat.ItemsSource != null && listView_Chat.ItemsSource.GetType() == typeof(ChatHistory)
                    && message != null && message.Length > 0) {
                if (IsCurrentChatGroup()) {
                    Group group = _manager.GetGroup((listView_Chat.ItemsSource as ChatHistory).Identifier);
                    _client.SendGroupMessage(group, message);
                }
                else {
                    Client client = _manager.GetClient((listView_Chat.ItemsSource as ChatHistory).Identifier);
                    _client.SendMessage(client, message);
                }
                (listView_Chat.ItemsSource as ChatHistory).PushMessage(message, _user.UserName, DateTime.Now.Ticks); 
            }
        }

        private void Button_Search_Click(object sender, RoutedEventArgs e) {
            SearchWindow window = new SearchWindow(_client);
            window.GroupCreated += (senderObj, args) => { _client.CreateGroup(args.GroupName, args.Members); };
            window.PrivateChatCreated += (senderObj, args) => { _client.ConfirmUser(args.PublicKeyHash); };
            window.ShowDialog();
        }

        private void listView_Clients_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var item = listView_Clients.SelectedItem;
            if(item != null && item.GetType() == typeof(ChatItem)) {
                if(_manager.IsGroup((item as ChatItem).Identifier)) {
                    Group group = _manager.GetGroup((item as ChatItem).Identifier);
                    if(group != null) {
                        OnDispatcher(new Action(() => {
                            listView_Chat.ItemsSource = _manager.GetChatHistory(group.GroupHash);
                            Label_Chatname.Text = $"Currently chatting in {group.GroupName}";
                        }));
                    }
                }
                else {
                    Client client = _manager.GetClient((item as ChatItem).Identifier);
                    if(client != null) {                                                
                        OnDispatcher(new Action(() => {
                            listView_Chat.ItemsSource = _manager.GetChatHistory(client.PublicKeyHash);
                            Label_Chatname.Text = $"Currently chatting with {client.UserName}";
                        }));
                    }
                }
                Highlight((item as ChatItem).Identifier, false);
            }
        }

        #endregion Events

        #region GUI Helpers

        private void Highlight(string identifier, bool value) {
            foreach(object item in listView_Clients.Items) {
                if(item.GetType() == typeof(ChatItem) && (item as ChatItem).Identifier == identifier) {
                    (item as ChatItem).Highlighted = value;
                }
            }
        }

        private bool IsCurrentChatGroup() {
            if(listView_Chat.ItemsSource != null && listView_Chat.ItemsSource.GetType() == typeof(ChatHistory)) {
                return (listView_Chat.ItemsSource as ChatHistory).IsGroup;
            }
            return false;
        }

        #endregion GUI Helpers

        private void OnDispatcher(Action action) {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        private long ConvertTicks(long timeStamp) => (timeStamp * 10000) + 621355968000000000;
    }
}