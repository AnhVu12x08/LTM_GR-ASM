using Communicator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;
using Application = System.Windows.Forms.Application;

namespace Chat_app_Client
{
    public partial class ChatBox : Form
    {
        private TcpClient server;
        private String name;
        private bool threadActive = true;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private byte[] imageBytes;

        public ChatBox(TcpClient server, String name)
        {
            this.server = server;
            this.name = name;
            InitializeComponent();
        }

        private void ChatBox_Load(object sender, EventArgs e)
        {
            streamReader = new StreamReader(server.GetStream());
            streamWriter = new StreamWriter(server.GetStream());

            this.Text = "Chat app - " + name;
            lblWelcome.Text = "Welcome, " + name;

            var mainThread = new Thread(() => receiveTheard());
            mainThread.Start();
            mainThread.IsBackground = true;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text == "" || txtReceiver.Text == "")
            {
                MessageBox.Show("Empty Fields", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtReceiver.Text == this.Name)
            {
                MessageBox.Show("Could not send message to yourself", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Messages messages = new Messages(this.name, txtReceiver.Text, txtMessage.Text);
            String messageJson = JsonSerializer.Serialize(messages);
            Json json = new Json("MESSAGE", messageJson);
            sendJson(json);

            txtMessage.Clear();
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                btnSend_Click(this.btnSend, e);
            }
        }

        private void tblGroup_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            txtReceiver.Text = tblGroup.Rows[e.RowIndex].Cells[0].Value.ToString();
        }

        private void tblUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string selectedUser = tblUser.Rows[e.RowIndex].Cells["Online"].Value.ToString();
            txtReceiver.Text = selectedUser;

            // Request chat history from the server
            RequestChatHistory(selectedUser);
        }

        private void RequestChatHistory(string otherUser)
        {
            // Send a request to the server for chat history
            Json request = new Json("REQUEST_HISTORY", otherUser); // "REQUEST_HISTORY" is the new type
            sendJson(request);
        }

        private void btnCreateGroup_Click(object sender, EventArgs e)
        {
            new Thread(() => Application.Run(new GroupCreator(server, name))).Start();
            this.Close();
        }

        private void btnPicture_Click(object sender, EventArgs e)
        {
            if (txtReceiver.Text == "")
            {
                MessageBox.Show("Receiver field is empty", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Thread dialogThread = new Thread(() =>
            {
                try
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        String fName = ofd.FileName;
                        String path = "";
                        fName = fName.Replace("\\", "/");
                        while (fName.IndexOf("/") > -1)
                        {
                            path += fName.Substring(0, fName.IndexOf("/") + 1);
                            fName = fName.Substring(fName.IndexOf("/") + 1);
                        }

                        FileMessage message = new FileMessage(this.name, txtReceiver.Text, File.ReadAllBytes(path + fName).Length.ToString(), Path.GetExtension(ofd.FileName));

                        Json json = new Json("FILE", JsonSerializer.Serialize(message));
                        sendJson(json);

                        server.Client.SendFile(path + fName);

                        AppendRichTextBox(this.name, message.receiver, "The file was sent.", "");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("An Error Occured", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            dialogThread.SetApartmentState(ApartmentState.STA);
            dialogThread.Start();
            dialogThread.IsBackground = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Json json = new Json("LOGOUT", this.name);
            sendJson(json);
            new Thread(() => Application.Run(new Login())).Start();
            threadActive = false;
            this.Close();
        }

        private void receiveTheard()
        {
            while (server != null && threadActive)
            {
                try
                {
                    String jsonString = streamReader.ReadLine();
                    Json? infoJson = JsonSerializer.Deserialize<Json?>(jsonString);

                    switch (infoJson?.type) // Add null check here
                    {
                        case "STARTUP_FEEDBACK":
                            cleanDataGridView(tblGroup);
                            cleanDataGridView(tblUser);

                            Startup startup = JsonSerializer.Deserialize<Startup>(infoJson.content);

                            List<string> groups = JsonSerializer.Deserialize<List<String>>(startup.group);
                            foreach (String group in groups)
                            {
                                addDataInDataGridView(tblGroup, new string[] { group });
                            }

                            List<string> users = JsonSerializer.Deserialize<List<String>>(startup.onlUser);
                            foreach (String user in users)
                            {
                                addDataInDataGridView(tblUser, new string[] { user });
                            }
                            break;
                        case "MESSAGE":
                            if (infoJson.content != null)
                            {

                                Messages message = JsonSerializer.Deserialize<Messages?>(infoJson.content);
                                if (message != null)
                                {
                                    if (message.sender != this.name)
                                    {
                                        AppendRichTextBox(message.sender, message.receiver, message.message, "");
                                    }
                                    else AppendRichTextBox(message.sender, message.receiver, message.message, "");
                                }
                            }
                            break;

                        case "CHAT_HISTORY": // New case to handle chat history from server
                            if (infoJson.content != null)
                            {
                                string historyJson = infoJson.content;
                                List<Messages> chatHistory = JsonSerializer.Deserialize<List<Messages>>(historyJson);
                                DisplayChatHistory(chatHistory);
                            }

                            break;
                        case "FILE":
                            if (infoJson.content != null)
                            {
                                BufferFile bufferFile = JsonSerializer.Deserialize<BufferFile>(infoJson.content);
                                List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".JPE", ".GIF", ".PNG" };

                                if (ImageExtensions.Contains(bufferFile.extension.ToUpper()))
                                {
                                    Thread staThread = new Thread(() =>
                                    {
                                        try
                                        {
                                            using (MemoryStream ms = new MemoryStream(imageBytes))
                                            {
                                                if (ms.Length > 0)
                                                {
                                                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                                                    Clipboard.SetImage(image);

                                                    rtbDialog.BeginInvoke(new MethodInvoker(() =>
                                                    {
                                                        rtbDialog.Paste();
                                                        AppendRichTextBox(bufferFile.sender, bufferFile.receiver, "Shared an image:", "");
                                                    }));
                                                }
                                                else
                                                {
                                                    Console.WriteLine("MemoryStream is empty. No image to display.");
                                                    AppendRichTextBox(bufferFile.sender, bufferFile.receiver, "Error displaying image. No image data found.", "");
                                                }
                                            }
                                        }
                                        catch (ArgumentException ex)
                                        {
                                            Console.WriteLine("Error loading image: " + ex.Message);
                                            AppendRichTextBox(bufferFile.sender, bufferFile.receiver, "Error displaying image. Invalid image data.", "");
                                        }

                                        catch (Exception ex)
                                        {

                                            Console.WriteLine("Unexpected error: " + ex.Message);
                                        }
                                    });

                                    staThread.SetApartmentState(ApartmentState.STA);
                                    staThread.Start();
                                    staThread.Join();
                                }
                                else
                                {
                                    string fileName = Path.Combine(Environment.CurrentDirectory, $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}__{bufferFile.sender}{bufferFile.extension}");

                                    try
                                    {
                                        if (File.Exists(fileName))
                                            File.Delete(fileName);

                                        using (FileStream fStream = new FileStream(fileName, FileMode.Create))
                                        {
                                            fStream.Write(bufferFile.buffer, 0, bufferFile.buffer.Length);
                                        }
                                        AppendRichTextBox(bufferFile.sender, bufferFile.receiver, $"Shared the {bufferFile.extension} file in ", Environment.CurrentDirectory);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Error saving file: " + ex.Message);
                                    }
                                }
                            }

                            break;
                    }

                }
                catch (Exception ex)
                {
                    // Handle exceptions appropriately (log, display an error message, etc.)
                    MessageBox.Show($"Error in receiveThread: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    threadActive = false; // Or consider reconnecting
                }
            }
        }

        private void DisplayChatHistory(List<Messages> chatHistory)
        {
            rtbDialog.Clear(); // Clear existing messages

            if (chatHistory != null)
            {

                foreach (Messages message in chatHistory)
                {
                    AppendRichTextBox(message.sender, message.receiver, message.message, "");
                }
            }

        }

        private void AppendRichTextBox(string sender, string receiver, string message, string link)
        {
            rtbDialog.BeginInvoke(new MethodInvoker(() =>
            {
                Font currentFont = rtbDialog.SelectionFont;

                //Username
                rtbDialog.SelectionStart = rtbDialog.TextLength;
                rtbDialog.SelectionLength = 0;
                rtbDialog.SelectionColor = Color.Red;
                rtbDialog.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, FontStyle.Bold);
                rtbDialog.AppendText(sender + "<" + receiver + ">");
                rtbDialog.SelectionColor = rtbDialog.ForeColor;

                rtbDialog.AppendText(": ");

                //Message
                rtbDialog.SelectionStart = rtbDialog.TextLength;
                rtbDialog.SelectionLength = 0;
                rtbDialog.SelectionColor = Color.Green;
                rtbDialog.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, FontStyle.Regular);
                rtbDialog.AppendText(message);
                rtbDialog.SelectionColor = rtbDialog.ForeColor;

                rtbDialog.AppendText(" ");

                //link
                rtbDialog.SelectionStart = rtbDialog.TextLength;
                rtbDialog.SelectionLength = 0;
                rtbDialog.SelectionColor = Color.Blue;
                rtbDialog.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, FontStyle.Underline);
                rtbDialog.AppendText(link);
                rtbDialog.SelectionColor = rtbDialog.ForeColor;

                //image
                rtbDialog.SelectionStart = rtbDialog.GetFirstCharIndexOfCurrentLine();
                rtbDialog.SelectionLength = 0;

                if (sender == this.name)
                {
                    rtbDialog.SelectionAlignment = HorizontalAlignment.Right;
                }
                else rtbDialog.SelectionAlignment = HorizontalAlignment.Left;

                rtbDialog.AppendText(Environment.NewLine);
            }));
        }

        private void cleanDataGridView(DataGridView dataGridView)
        {
            dataGridView.BeginInvoke(new MethodInvoker(() =>
            {
                dataGridView.Rows.Clear();
            }));

        }

        private void addDataInDataGridView(DataGridView dataGridView, String[] array)
        {
            dataGridView.Invoke(new Action(() => { dataGridView.Rows.Add(array); }));
        }

        private void sendJson(Json json)
        {
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(json);
            String S = Encoding.ASCII.GetString(jsonUtf8Bytes, 0, jsonUtf8Bytes.Length);

            streamWriter.WriteLine(S);
            streamWriter.Flush();
        }


        private void ChatBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            Json json = new Json("LOGOUT", this.name);
            sendJson(json);
            threadActive = false;
        }

        private void rtbDialog_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
