using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;


namespace WebServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static List<Thread> threads = new List<Thread>();
        public static int maxClients=-1;
        public static bool started = false;
        public static int port=-1;

        public static string directory = "config";
        public static string log = @"config/server.log";
        public static string www=null;


        private void Form1_Load(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            lblPath.Text = "Non Selezionato";

            lblStatus.Text = "Inactive";
            lblStatus.ForeColor = Color.Orange;

            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                if (!File.Exists(log))
                    File.Create(log);
            }
            catch (Exception a)
            {
                MessageBox.Show("Errore nella creazione della prima configurazione!\n\n" + a.ToString(), "!ERRORE!");
            }
        }

        class Listener
        {
            public void startListening()
            {
                IPAddress ipAddress = IPAddress.Parse("10.0.0.98");
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

                Socket listener = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(maxClients);

                    while (true)
                    {
                        Socket handler = listener.Accept();

                        Manager manager = new Manager(handler);
                        Thread t = new Thread(new ThreadStart(manager.acceptClient));
                        t.Start();
                        threads.Add(t);
                    }
                }
                catch (Exception a)
                {
                    File.AppendAllText(log, DateTime.Now.ToString() + "\tError\t" + a.ToString() + "\n");
                }
            }

            public void stopListening()
            {
                try
                {
                    foreach (Thread t in threads)
                    {
                        if (t.IsAlive)
                            t.Abort();
                    }
                }
                catch (Exception a)
                {
                    Console.WriteLine(a.Message.ToString());
                    File.AppendAllText(log, DateTime.Now.ToString() + "\tError\t" + a.ToString() + "\n");
                }
            }
        }

        class Manager
        {
            Socket client;
            byte[] bytes = new byte[4096];
            string dataReceive = "";
            string dataSend = "";

            public Manager(Socket client)
            {
                this.client = client;
            }

            public void acceptClient()
            {
                byte[] msg;
                int bytesRec;
                string root = "index.html";
                string[] lines = null;
                string[] requested = null;
                string document = null;
                int length = -1;

                bytesRec = client.Receive(bytes);
                dataReceive = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                lines = dataReceive.Split('\n');
                if (lines[0].StartsWith("GET /"))
                {
                    lock (lines)
                    {
                        requested = lines[0].Split(' ');
                        if (requested[1] == "/")
                        {
                            if (File.Exists(www + root))
                            {
                                document = File.ReadAllText(www + root);
                                length = document.Length;
                                dataSend = "HTTP/1.1 200 OK\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nLast-Modified: " + File.GetLastWriteTime(www + root).ToString() + "\r\nContent-Lenght: " + length + "\r\nContent-Type: text/html\r\n\r\n" + document + "\n\n";
                            }
                            else
                                dataSend = "HTTP/1.1 404 Not found\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nContent-Type: text/html\r\n\r\n";
                        }
                        if (requested[1].Contains(".html"))
                        {
                            if (File.Exists(www + requested[1]))
                            {
                                document = File.ReadAllText(www + requested[1]);
                                length = document.Length;
                                dataSend = "HTTP/1.1 200 OK\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nLast-Modified: " + File.GetLastWriteTime(www + requested[1]).ToString() + "\r\nContent-length: " + length + "\r\nContent-Type: text/html\r\n\r\n" + document + "\n\n";
                            }
                            else
                                dataSend = "HTTP/1.1 404 Not found\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nContent-Type: text/html\r\n\r\n";
                        }
                        if (requested[1].Contains(".css"))
                        {
                            if (File.Exists(www + requested[1]))
                            {
                                document = File.ReadAllText(www + requested[1]);
                                length = document.Length;
                                dataSend = "HTTP/1.1 200 OK\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nLast-Modified: " + File.GetLastWriteTime(www + requested[1]).ToString() + "\r\nContent-length: " + length + "\r\nContent-Type: text/css\r\n\r\n" + document + "\n\n";
                            }
                            else
                                dataSend = "HTTP/1.1 404 Not found\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nContent-Type: text/html\r\n\r\n";
                        }
                        if (requested[1].Contains(".js"))
                        {
                            if (File.Exists(www + requested[1]))
                            {
                                document = File.ReadAllText(www + requested[1]);
                                length = document.Length;
                                dataSend = "HTTP/1.1 200 OK\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nLast-Modified: " + File.GetLastWriteTime(www + requested[1]).ToString() + "\r\nContent-length: " + length + "\r\nContent-Type: text/javascript\r\n\r\n" + document + "\n\n";
                            }
                            else
                                dataSend = "HTTP/1.1 404 Not found\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nContent-Type: text/html\r\n\r\n";
                        }
                        if (requested[1].Contains(".ico") || requested[1].Contains(".png") || requested[1].Contains(".jpg") || requested[1].Contains(".jpeg"))
                        {
                            if (File.Exists(www + requested[1]))
                            {
                                document=File.ReadAllText(www + requested[1]);
                                length = document.Length;
                                dataSend = "HTTP/1.1 200 OK\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nLast-Modified: " + File.GetLastWriteTime(www + requested[1]).ToString() + "\r\nContent-length: " + new FileInfo(www + requested[1]).Length + "\r\nContent-Type: image/jpeg\r\n\r\n";
                                client.SendFile(www + requested[1]);
                            }
                            else
                                dataSend = "HTTP/1.1 404 Not found\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nContent-Type: text/html\r\n";
                        }

                        msg = Encoding.ASCII.GetBytes(dataSend);
                        client.Send(msg);

                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                }
            }
        }


        public void btnStart_Click_1(object sender, EventArgs e)
        {
            if (Convert.ToInt32(txtPort.Text) > 0 && Convert.ToInt32(txtPort.Text) < 65536)
            {
                port = Convert.ToInt32(txtPort.Text);
                if (www != null)
                {
                    if (started == false)
                    {
                        maxClients = Convert.ToInt32(nClients.Value);
                        Listener l = new Listener();
                        Thread ui = new Thread(new ThreadStart(l.startListening));
                        started = true;
                        ui.Start();

                        File.AppendAllText(log, DateTime.Now.ToString() + "\tInfo\tServer Avviato\n");

                        btnStop.Enabled = true;
                        btnBrowse.Enabled = false;

                        lblStatus.Text = "Started";
                        lblStatus.ForeColor = Color.Green;
                    }
                    else
                        MessageBox.Show("Server già in ascolto!", "!ERRORE!");
                }
                else
                    MessageBox.Show("Il percorso selezionato non è valido!", "!ERRORE!"); 
            }
            else
                MessageBox.Show("La porta deve essere compresa tra 0 e 65536!", "!ERRORE!");
            btnStart.Enabled = false;
        }

        private void btnStop_Click_1(object sender, EventArgs e)
        {
            Listener l = new Listener();
            l.stopListening();
            File.AppendAllText(log, DateTime.Now.ToString() + "\tInfo\tServer Fermato\n");
            started = false;

            btnBrowse.Enabled = true;
            btnStart.Enabled = true;

            lblStatus.Text = "Stopped";
            lblStatus.ForeColor = Color.Red;

            btnStop.Enabled = false;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.ShowDialog();

            if (folder.SelectedPath == null)
                MessageBox.Show("Il percorso selezionato non è valido!", "ERRORE");
            else if(Directory.GetFiles(folder.SelectedPath, "*.html").Length==0)
                MessageBox.Show("Il percorso selezionato non è valido!", "ERRORE");
            else
            {
                www = folder.SelectedPath + "/";
                lblPath.Text = www;
            }
        }

        private void btnBrowse_MouseEnter(object sender, EventArgs e)
        {

        }

        private void btnBrowse_MouseHover(object sender, EventArgs e)
        {
            ToolTip ToolTip1 = new ToolTip();
            ToolTip1.SetToolTip(this.btnBrowse, this.btnBrowse.Text);
            ToolTip1.Show("Browse", btnBrowse);
        }
    }
}
