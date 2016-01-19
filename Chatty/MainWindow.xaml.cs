using Chatty.Model;
using Chatty.Model.INotify;
using System.Windows;

namespace Chatty
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChattyClient _client;
        private ClientManager _manager;

        public MainWindow() {
            InitializeComponent();

            _client = new ChattyClient();
            _client.SetMessageListener(new SocketIOListener("....Adress...."));
            _client.Listener.OnMessageReceived += Listener_OnMessageReceived;
            _client.Listener.OnUserSearch += Listener_OnUserSearch;
            _client.Listener.OnUserConfirm += Listener_OnUserConfirm;
            _client.Listener.OnGroupJoined += Listener_OnGroupJoined;

            _manager = new ClientManager();
        }

        private void Listener_OnGroupJoined(object sender, GroupJoinedEventArgs e) {
            _manager.AddGroup(new Group() { GroupId = e.GroupName, ClientList = e.Members });
        }

        private void Listener_OnUserConfirm(object sender, UserComfirmEventArgs e) {
            throw new System.NotImplementedException();
        }

        private void Listener_OnUserSearch(object sender, UserSearchEventArgs e) {
            throw new System.NotImplementedException();
        }

        private void Listener_OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            throw new System.NotImplementedException();
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e) {
            string message = Textbox_Message.Text;
            if(/*isGroupMessage*/ true) {
                _client.SendGroupMessage(null, message);
            }
            else {
                _client.SendMessage(null, message);
            }
            //Push to chat
        }

        
        private void Highlight(string identifier, bool value) {
            foreach(object item in listView_Clients.Items) {
                if((item as ChatItem).Identifier == identifier) {
                    (item as ChatItem).Highlighted = value;
                }
            }
        }
    }
}
