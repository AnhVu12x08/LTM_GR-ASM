using Communicator;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat_app_Client
{
    public partial class GroupCreator : Form
    {
        private TcpClient server;
        private string name;
        private ChatBox parentChatBox;
        private StreamWriter streamWriter; // StreamWriter for communication

        public GroupCreator(TcpClient server, string name, ChatBox parent)
        {
            InitializeComponent();
            this.server = server;
            this.name = name;
            this.parentChatBox = parent;
            this.streamWriter = new StreamWriter(server.GetStream());
        }
        // method to take data from users.json from server and display it in the members_listtextbox
        public void displayMembers()
        {
            try
            {
                string relativePath = @"..\..\..\..\Chat-app Server\bin\Debug\net6.0-windows\users.json";
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(baseDirectory, relativePath); // Use Path.Combine!

                string jsonString = File.ReadAllText(fullPath);
                Dictionary<string, string> users = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);



                if (users != null)
                {
                    List<string> usernamesToAdd = users.Keys.ToList();
                    members_listcheckbox.Invoke((MethodInvoker)delegate
                    {
                        members_listcheckbox.Items.AddRange(usernamesToAdd.ToArray());
                    });
                }
                else
                {
                    Console.WriteLine("Error: Could not deserialize users from users.json"); //Or appropriate error handling.
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading or deserializing users.json: {ex.Message}");
                //Handle the exception appropriately (e.g., log, show a message box)

            }
        }


        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (members_listcheckbox.CheckedItems.Count < 2 || txtGroupName.Text == "")
            {
                MessageBox.Show("Please select 2 members and enter a group name", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            StreamWriter streamWriter = new StreamWriter(server.GetStream());

            //change this data of members_listcheckbox to look like the data of txtMembers
            string selectedMembers = string.Join(", ", members_listcheckbox.CheckedItems.Cast<string>());


            Group group = new Group(txtGroupName.Text, selectedMembers); // Use selectedMembers here

            String jsonString = JsonSerializer.Serialize(group);
            Json json = new Json("CREATE_GROUP", jsonString);

            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(json);
            String S = Encoding.ASCII.GetString(jsonUtf8Bytes, 0, jsonUtf8Bytes.Length);

            streamWriter.WriteLine(S);
            streamWriter.Flush();

            this.DialogResult = DialogResult.OK;
            this.Close(); // Close the GroupCreator dialog


        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void GroupCreator_Load(object sender, EventArgs e)
        {
            displayMembers();
        }
    }
}
