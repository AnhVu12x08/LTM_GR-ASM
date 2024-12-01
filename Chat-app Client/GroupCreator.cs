using Communicator;
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
        private ChatBox parentChatBox; // Reference to the ChatBox form that opened this dialog
        private StreamWriter streamWriter; // StreamWriter for communication

        // Constructor receiving the parent ChatBox:
        public GroupCreator(TcpClient server, string name, ChatBox parent)
        {
            InitializeComponent();
            this.server = server;
            this.name = name;
            this.parentChatBox = parent;
            this.streamWriter = new StreamWriter(server.GetStream()); // Initialize StreamWriter
        }


        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (txtGroupName.Text == "" || txtMembers.Text == "")
            {
                MessageBox.Show("Empty Fields", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            StreamWriter streamWriter = new StreamWriter(server.GetStream());

            Group group = new Group(txtGroupName.Text, txtMembers.Text);
            String jsonString = JsonSerializer.Serialize(group);
            Json json = new Json("CREATE_GROUP", jsonString);

            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(json);
            String S = Encoding.ASCII.GetString(jsonUtf8Bytes, 0, jsonUtf8Bytes.Length);

            streamWriter.WriteLine(S);
            streamWriter.Flush();

            this.DialogResult = DialogResult.OK;  // Important!
            this.Close(); // Close the GroupCreator dialog

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void GroupCreator_Load(object sender, EventArgs e)
        {

        }
    }
}
