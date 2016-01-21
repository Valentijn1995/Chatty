using Chatty.Model;
using System;
using System.Collections.Generic;
using System.Windows;

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
            if(e.FoundMembers != null && e.FoundMembers.Count > 0) 
                listView_results.ItemsSource = e.FoundMembers;
        }

        private void Btn_Search_Click(object sender, RoutedEventArgs e) {
            string searchValue = Txtbox_Search.Text;
            if(searchValue != null && searchValue.Length > 0) {
                _client.SearchUser(searchValue);
            }
        }

        private void Btn_PrivateCon_Click(object sender, RoutedEventArgs e) {
            if(listView_results.SelectedItems != null && listView_results.SelectedItems.Count > 0 && PrivateChatCreated != null) {
                foreach(var item in listView_results.SelectedItems) {
                    Client client = item as Client;
                    PrivateChatCreated(this, new UserConfirmEventArgs() { UserName = client.UserName, PublicKey = client.PublicKey });
                }
                this.Close();
            }
        }

        private void Btn_AddMember_Click(object sender, RoutedEventArgs e) {
            if(listView_results.SelectedItems != null && listView_results.SelectedItems.Count > 0) {
                foreach(var item in listView_results.SelectedItems) {
                    listView_results.Items.Add(item as Client);
                }
            }
        }

        private void Btn_RemoveMember_Click(object sender, RoutedEventArgs e) {
            if(listView_group.SelectedItems != null && listView_group.SelectedItems.Count > 0) {
                foreach(var item in listView_group.SelectedItems) {
                    listView_group.Items.Remove(item as Client);
                }
            }
        }

        private void Btn_GroupCon_Click(object sender, RoutedEventArgs e) {
            if(listView_group.Items.Count > 1 && GroupCreated != null) {
                List<Client> clients = new List<Client>();
                foreach(var item in listView_group.SelectedItems) {
                    clients.Add(item as Client);
                }
                GroupCreated(this, new GroupJoinedEventArgs() { GroupName = "", Members = clients });   //TODO Add groupname
                this.Close();
            }
        }
    }
}