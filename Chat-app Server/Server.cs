using Communicator;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Security.Cryptography;


namespace Chat_app_Server
{
    public partial class Server : Form
    {
        private bool active = true;
        private IPEndPoint iep;
        private TcpListener server;
        private Dictionary<string, string> USER = new Dictionary<string, string>(); // Initialize
        private Dictionary<string, List<string>> GROUP = new Dictionary<string, List<string>>(); // Initialize
        private Dictionary<string, TcpClient> CLIENT = new Dictionary<string, TcpClient>(); // Initialize
        private const string userFileName = "users.json";
        private const string groupFileName = "groups.json";


        public Server()
        {
            InitializeComponent();
            LoadUserData(); // Load user data on server startup
            LoadGroupData();
        }

        private void LoadGroupData()
        {
            if (File.Exists(groupFileName))
            {
                try
                {
                    string json = File.ReadAllText(groupFileName);
                    GROUP = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading group data: {ex.Message}");
                }
            }
            else // Group file doesn't exist - create initial groups
            {
                InitializeGroups();  // Call a separate function to create initial groups
                SaveGroupData();
            }
        }

        private void SaveGroupData()
        {
            try
            {
                string json = JsonSerializer.Serialize(GROUP);
                using (FileStream fileStream = new FileStream(groupFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(json);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving group data: {ex.Message}");
            }

        }

        private void LoadUserData()
        {
            if (File.Exists(userFileName))
            {
                try
                {
                    string json = File.ReadAllText(userFileName);
                    USER = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading user data: {ex.Message}");
                    // Consider creating a default empty file:
                    // File.WriteAllText(userFileName, JsonSerializer.Serialize(new Dictionary<string, string>()));

                }
            }
        }

        private void SaveUserData()
        {
            try
            {
                string json = JsonSerializer.Serialize(USER);

                // Use a file stream with exclusive access to prevent corruption during concurrent writes:
                using (FileStream fileStream = new FileStream(userFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user data: {ex.Message}");
            }
        }

        private void InitializeGroups()  // Separated group initialization logic
        {
            for (int i = 0; i < 5; i++)
            {
                List<string> groupUser = new List<string>();
                for (byte j = 0; j < 3; j++)
                {
                    // Make sure user accounts exist before adding them to groups:
                    string username = ((char)('A' + 3 * i + j)).ToString();
                    if (USER.ContainsKey(username)) // Or another way to check valid users.
                    {
                        groupUser.Add(username);
                    }

                }

                if (USER.ContainsKey("A") && !groupUser.Contains("A")) // Check if user "A" exists
                {
                    groupUser.Add("A");
                }


                GROUP.Add("Group " + i.ToString(), groupUser);
            }
        }


        private void Server_Load(object sender, EventArgs e)
        {
            String IP = null;
            var host = Dns.GetHostByName(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.ToString().Contains('.'))
                {
                    IP = ip.ToString();
                }
            }
            if (IP == null)
            {
                MessageBox.Show("No network adapters with an IPv4 address in the system!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }



        private void userInitialize()
        {
            USER = new Dictionary<String, String>();
            GROUP = new Dictionary<String, List<String>>();
            CLIENT = new Dictionary<String, TcpClient>();

            for (char uName = 'A'; uName <= 'Z'; uName++)
            {
                String pass = "123";
                USER.Add(uName.ToString(), pass);
            }

            for (int i = 0; i < 5; i++)
            {
                List<string> groupUser = new List<string>();
                for (byte j = 0; j < 3; j++)
                {
                    char u = (Char)('A' + 3 * i + j);
                    groupUser.Add(u.ToString());
                }
                if (!groupUser.Contains("A"))
                {
                    groupUser.Add("A");
                }
                GROUP.Add("Group " + i.ToString(), groupUser);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            iep = new IPEndPoint(IPAddress.Any, 8000);
            server = new TcpListener(iep);
            server.Start();

            Thread ServerThread = new Thread(new ThreadStart(ServerStart));
            ServerThread.IsBackground = true;
            ServerThread.Start();
        }

        private void ServerStart()
        {
            try
            {
                AppendRichTextBox("Start accept connect from client!");
                changeButtonEnable(btnStart, false);
                changeButtonEnable(btnStop, true);
                //Clipboard.SetText(txtIP.Text);
                while (active)
                {
                    TcpClient client = server.AcceptTcpClient();
                    var clientThread = new Thread(() => clientService(client));
                    clientThread.Start();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clientService(TcpClient client)
        {
            StreamReader streamReader = new StreamReader(client.GetStream());
            String s = streamReader.ReadLine();
            Json infoJson = JsonSerializer.Deserialize<Json>(s);

            if (infoJson != null)
            {
                switch (infoJson.type)
                {
                    case "SIGNIN":
                        reponseSignin(infoJson, client);
                        break;
                    case "LOGIN":
                        reponseLogin(infoJson, client);
                        break;
                }
            }

            try
            {
                bool threadActive = true;
                while (threadActive && client != null)
                {
                    s = streamReader.ReadLine();
                    infoJson = JsonSerializer.Deserialize<Json>(s);
                    if (infoJson != null && infoJson.content != null)
                    {
                        switch (infoJson.type)
                        {
                            case "MESSAGE":
                                if (infoJson.content != null)
                                {
                                    reponseMessage(infoJson);
                                }
                                break;
                            case "CREATE_GROUP":
                                if (infoJson.content != null)
                                {
                                    createGroup(infoJson);
                                }
                                break;
                            case "FILE":
                                if (infoJson.content != null)
                                {
                                    reponseFile(infoJson, client);
                                }
                                break;
                            case "LOGOUT":
                                if (infoJson.content != null)
                                {
                                    CLIENT[infoJson.content].Close();
                                    CLIENT.Remove(infoJson.content);
                                    AppendRichTextBox(infoJson.content + " logged out.");
                                    threadActive = false;

                                    foreach (String key in CLIENT.Keys)
                                    {
                                        startupClient(CLIENT[key], key);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch
            {
                //client.Close();
            }
        }

        private void reponseSignin(Json infoJson, TcpClient client)
        {
            Account account = JsonSerializer.Deserialize<Account>(infoJson.content);

            if (account != null && account.userName != null && !USER.ContainsKey(account.userName))
            {
                Json notification = new Json("SIGNIN_FEEDBACK", "TRUE");
                sendJson(notification, client);
                AppendRichTextBox(account.userName + " signed in!");

                USER.Add(account.userName, account.password); 
                SaveUserData();                            

                CLIENT.Add(account.userName, client);

                foreach (String key in CLIENT.Keys)
                {
                    startupClient(CLIENT[key], key);
                }
            }
            else
            {
                Json notification = new Json("SIGNIN_FEEDBACK", "FALSE");
                sendJson(notification, client);
                AppendRichTextBox(account.userName + " failed to sign in (username taken or invalid data).");
            }
        }

        private void reponseLogin(Json infoJson, TcpClient client)
        {
            Account account = JsonSerializer.Deserialize<Account>(infoJson.content);
            if (account != null && account.userName != null && USER.ContainsKey(account.userName))
            {
                if (!CLIENT.ContainsKey(account.userName) && USER[account.userName] == account.password)
                {
                    Json notification = new Json("LOGIN_FEEDBACK", "TRUE");
                    sendJson(notification, client);
                    AppendRichTextBox(account.userName + " logged in!");

                    CLIENT.Remove(account.userName);
                    CLIENT.Add(account.userName, client);

                    foreach (String key in CLIENT.Keys)
                    {
                        startupClient(CLIENT[key], key);
                    }
                }
                else
                {   // *** Added this else block ***
                    Json notification = new Json("LOGIN_FEEDBACK", "FALSE");
                    sendJson(notification, client);
                    AppendRichTextBox(account.userName + " cannot login (incorrect password or already logged in).");
                }
            }
            else
            {
                Json notification = new Json("LOGIN_FEEDBACK", "FALSE");
                sendJson(notification, client);
                AppendRichTextBox(account.userName + " can not login!");
            }
        }
  

        private void startupClient(TcpClient client, String name)
        {
            List<String> onlUser = new List<string>(CLIENT.Keys);
            onlUser.Remove(name);

            List<String> group = new List<string>();
            foreach (String key in GROUP.Keys)
            {
                if (GROUP[key].Contains(name))
                {
                    group.Add(key);
                }
            }

            string jsonUser = JsonSerializer.Serialize<List<String>>(onlUser);
            string jsonGroup = JsonSerializer.Serialize<List<String>>(group);

            Startup startup = new Startup(jsonUser, jsonGroup);
            String startupJson = JsonSerializer.Serialize(startup);
            Json json = new Json("STARTUP_FEEDBACK", startupJson);
            sendJson(json, client);
        }

        private void reponseMessage(Json infoJson)
        {
            Messages messages = JsonSerializer.Deserialize<Messages>(infoJson.content);
            if (messages != null && CLIENT.ContainsKey(messages.receiver))
            {
                AppendRichTextBox(messages.sender + " to " + messages.receiver + ": " + messages.message);

                TcpClient receiver = CLIENT[messages.receiver];
                sendJson(infoJson, receiver);
                receiver = CLIENT[messages.sender];
                sendJson(infoJson, receiver);
            }
            else
            {
                if (GROUP.ContainsKey(messages.receiver))
                {
                    AppendRichTextBox(messages.sender + " to " + messages.receiver + ": " + messages.message);
                    foreach (String user in GROUP[messages.receiver])
                    {
                        if (CLIENT.ContainsKey(user))
                        {
                            TcpClient receiver = CLIENT[user];
                            sendJson(infoJson, receiver);
                        }
                    }
                }
            }
        }

        private void createGroup(Json infoJson)
        {
            // In createGroup, check if the group already exists before adding it
            List<string> groupUser = new List<string>();
            Group group = JsonSerializer.Deserialize<Group>(infoJson.content);

            string[] values = group.members.Split(',');

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();


                if (USER.ContainsKey(values[i])) // Only add existing users to groups
                {
                    groupUser.Add(values[i]);

                }



            }

            // Don't add if the group with that name already exists to prevent overwriting.
            if (!GROUP.ContainsKey(group.name))
            {
                GROUP.Add(group.name, groupUser);

                foreach (String key in CLIENT.Keys)
                {
                    startupClient(CLIENT[key], key);
                }
            }
            // Optionally send an error to the client if they tried to create a duplicate group


            SaveGroupData();  //  Save group data to file

        }

        private void reponseFile(Json infoJson, TcpClient client)
        {
            FileMessage fileMessage = JsonSerializer.Deserialize<FileMessage>(infoJson.content);

            try
            {

                int length = Convert.ToInt32(fileMessage.lenght);
                byte[] buffer = new byte[length];
                int received = 0;
                int read = 0;
                int size = 1024;
                int remaining = 0;

                // Read bytes from the client using the length sent from the client    
                while (received < length)
                {
                    remaining = length - received;
                    if (remaining < size)
                    {
                        size = remaining;
                    }

                    read = client.GetStream().Read(buffer, received, size);
                    received += read;
                }

                BufferFile bufferFile = new BufferFile(fileMessage.sender, fileMessage.receiver, buffer, fileMessage.extension);

                String jsonString = JsonSerializer.Serialize(bufferFile);
                Json json = new Json("FILE", jsonString);

                if (CLIENT.ContainsKey(fileMessage.receiver))
                {
                    TcpClient receiver = CLIENT[fileMessage.receiver];
                    sendJson(json, receiver);
                }

                else
                {
                    if (GROUP.ContainsKey(fileMessage.receiver))
                    {
                        foreach (String user in GROUP[fileMessage.receiver])
                        {
                            if (CLIENT.ContainsKey(user))
                            {
                                TcpClient receiver = CLIENT[user];
                                sendJson(json, receiver);
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
        }

        private void sendJson(Json json, TcpClient client)
        {
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(json);
            StreamWriter streamWriter = new StreamWriter(client.GetStream());

            String S = Encoding.ASCII.GetString(jsonUtf8Bytes, 0, jsonUtf8Bytes.Length);

            streamWriter.WriteLine(S);
            streamWriter.Flush();
        }

        private void AppendRichTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendRichTextBox), new object[] { value });
                return;
            }
            rtbDialog.AppendText(value);
            rtbDialog.AppendText(Environment.NewLine);
        }

        private void changeButtonEnable(Button btn, bool enable)
        {
            btn.BeginInvoke(new MethodInvoker(() =>
            {
                btn.Enabled = enable;
            }));
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (CLIENT.Count() > 0)
            {
                MessageBox.Show("The server has " + CLIENT.Count + " user(s) logged in.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            active = false;
            Environment.Exit(0);
        }
    }
}