﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace b3dteam_app.View.UserUtilities
{
    /// <summary>
    /// Interaction logic for AddContact.xaml
    /// </summary>
    public partial class AddContact : Window
    {
        private Users UsersWindow;
        public AddContact(Users usersWindow)
        {
            this.UsersWindow = usersWindow;
            InitializeComponent();
            SetButtonsStatus(false);
        }

        private void textbox_FindBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(textbox_FindBox.Text.Trim().Length == 0)
            {
                SetButtonsStatus(false);
            }
            else
            {
                SetButtonsStatus(true);
            }
        }

        private async void button_Add_Click(object sender, RoutedEventArgs e)
        {
            var clickedItem = listview_Users.SelectedItem as TextBlock;

            if(clickedItem == null || clickedItem.Text == "No users were found")
            {
                return;
            }
            else
            {
                SetButtonsStatus(false);
                var user = await Users.ChatEngine.GetUser(int.Parse(clickedItem.Text.Split('#')[0]));

                if(user == null)
                {
                    SetButtonsStatus(true);
                    MessageBox.Show("Error occured");
                }
                else if(user != null && user.userid == Users.ClientUser.userid)
                {
                    SetButtonsStatus(true);
                    MessageBox.Show("You can't add self");
                }
                else
                {
                    if (Users.ClientUser.IsPrivateChatRoomWithUser(user))
                    {
                        MessageBox.Show("You have alerady talk with this user!");
                    }
                    else
                    {
                        if (await UsersWindow.AddContactToList(user))
                        {
                            MessageBox.Show("User has been added to your contact list. Wait a while for refresh.");
                            listview_Users.Items.Remove(listview_Users.SelectedItem);
                        }
                        else
                        {
                            MessageBox.Show("Error durning creating new room");
                        }
                    }

                    SetButtonsStatus(true);
                }
            }

        }

        private async void button_Find_Click(object sender, RoutedEventArgs e)
        {
            listview_Users.Items.Clear();

            listview_Users.Items.Add(new TextBlock() { Text = "Finding..." });

            SetButtonsStatus(false);
            await Task.Delay(100);

            int res = -1;
            if (int.TryParse(textbox_FindBox.Text, out res))
            {
                var user = (await helper.SQLManager.GetUser(res));

                if(user == null)
                {
                    SetButtonsStatus(true);
                    AddNoItemsFoundText();
                    return;
                }
                listview_Users.Items.Clear();
                listview_Users.Items.Add(new TextBlock() { Text = $"{user.userid}# {user.login}" });
            }
            else
            {
                bool firstLoop = false;
                (await helper.SQLManager.GetUsers()).
                        Where(p => 
                            (p.login.ToLower()
                                .Contains(textbox_FindBox.Text.ToLower())))
                                        .Where(p => !Users.ClientUser.IsPrivateChatRoomWithUser(new ChatManager.User() { userid = p.userid }))
                                            .ToList()
                                                .ForEach(p =>
                                                        {
                                                            if (firstLoop == false)
                                                            {
                                                                listview_Users.Items.Clear();
                                                                firstLoop = true;
                                                            }
                                                            listview_Users.Items.Add(new TextBlock() { Text = $"{p.userid}# {p.login}" });
                                                        });

                if(firstLoop == false)
                {
                    listview_Users.Items.Clear();
                }
            }

            if(listview_Users.Items.Count == 0)
            {
                AddNoItemsFoundText();
            }

            SetButtonsStatus(true);
        }

        private void AddNoItemsFoundText()
        {
            listview_Users.Items.Add(new TextBlock() { Text = "No users were found" });
        }
        private void SetButtonsStatus(bool enabled)
        {
            button_Add.IsEnabled = enabled;
            button_Find.IsEnabled = enabled;
        }
    }
}
