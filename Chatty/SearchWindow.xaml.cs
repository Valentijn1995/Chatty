using Chatty.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace Chatty
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private ChattyClient _client;
        public event EventHandler<GroupJoinedEventArgs> GroupCreated;
        public event EventHandler<UserConfirmEventArgs> PrivateChatCreated;


        public SearchWindow(ChattyClient client) {
            _client = client;
            _client.OnUserSearch += OnUserSearch;

            InitializeComponent();
        }

        private void OnUserSearch(object sender, UserSearchEventArgs e) {
            if (e.FoundMembers != null && e.FoundMembers.Count > 0) {
                /*List<Client> members = new List<Client>();
                foreach (Client client in e.FoundMembers) {
                    if (!members.Contains(client))
                        members.Add(client);
                }*/
                //TODO Filter unique clients
                OnDispatcher( new Action(() => {
                    foreach (Client client in e.FoundMembers) {
                        listView_results.Items.Add(client);
                    }
                }));
            }
        }

        private void Btn_Search_Click(object sender, RoutedEventArgs e) {
            string searchValue = Txtbox_Search.Text;
            if(searchValue != null && searchValue.Length > 0) {
                _client.SearchUser(searchValue);
            }

            listView_results.Items.Clear();
        }

        private void Btn_PrivateCon_Click(object sender, RoutedEventArgs e) {
            if(listView_results.SelectedItems != null && listView_results.SelectedItems.Count > 0 && PrivateChatCreated != null) {
                foreach(var item in listView_results.SelectedItems) {
                    Client client = item as Client;
                    PrivateChatCreated(this, new UserConfirmEventArgs() { UserName = client.UserName, PublicKeyHash = client.PublicKeyHash });
                }
                this.Close();
            }
        }

        private void Btn_AddMember_Click(object sender, RoutedEventArgs e) {
            if(listView_results.SelectedItems != null && listView_results.SelectedItems.Count > 0) {

                //var addedClients = listView_group.Items;
                foreach(var item in listView_results.SelectedItems) {
                    //if (!addedClients.Contains((item as Client))) {
                        OnDispatcher(new Action(() => {
                            listView_group.Items.Add(item as Client);
                        }));
                }
                //}
                //TODO Filter unique members
            }
        }

        private void Btn_RemoveMember_Click(object sender, RoutedEventArgs e) {
            if(listView_group.SelectedItems != null && listView_group.SelectedItems.Count > 0) {
                foreach(var item in listView_group.SelectedItems) {
                    OnDispatcher(new Action(() => {
                        listView_group.Items.Remove(item as Client);
                    }));
                }
            }
        }

        private void Btn_GroupCon_Click(object sender, RoutedEventArgs e) {
            string groupName = TxtBox_groupName.Text;
            if (groupName == null || groupName.Length <= 0)
                return;

            if(listView_group.Items.Count >= 2 && GroupCreated != null) {   
                List<Client> clients = new List<Client>();
                foreach(var item in listView_group.Items) {
                    clients.Add(item as Client);
                }
                GroupCreated(this, new GroupJoinedEventArgs() { GroupName = groupName, Members = clients });
                this.Close();
            }
        }

        private void OnDispatcher(Action action) {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }
    }
}