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

namespace WebServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static int clients = 10;
        public static List<Thread> threads = new List<Thread>();
        public static bool started = false;
        //public static Dictionary<string, string> login = new Dictionary<string, string>();
        //public static Dictionary<string, int> cookies = new Dictionary<string, int>();

        //public static string accessList = @"access.csv";
        public static string directory = "config";
        public static string log = @"config/server.log";
        public static string wwwData = "www";
        //public static string cookie = @"config/cookie.conf";


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                if (!File.Exists(log))
                    File.Create(log);
            }
            catch (Exception a)
            {
                MessageBox.Show("Errore nella creazione della prima configurazione! \n" + a.ToString(), "!ERRORE!");
            }
        }


        class Listener
        {
            public void startListening()
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 80);

                Socket listener = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                Console.WriteLine("Timeout : {0}", listener.ReceiveTimeout);

                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(clients);

                    while (true)
                    {
                        Console.WriteLine("Waiting for a connection...");

                        Socket handler = listener.Accept();

                        Manager manager = new Manager(handler);
                        Thread t = new Thread(new ThreadStart(manager.acceptClient));
                        t.Start();
                        threads.Add(t);
                    }
                }
                catch (Exception a)
                {
                    MessageBox.Show(a.ToString(), "!ERRORE!");
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
            byte[] bytes = new byte[1024];
            string dataReceive = "";
            string dataSend = "";
            //string http = @"http.txt";


            public Manager(Socket client)
            {
                this.client = client;
            }

            public void acceptClient()
            {
                byte[] msg;
                int bytesRec;
                string root = "/index.html";

                bytesRec = client.Receive(bytes);
                dataReceive = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                string[] lines = dataReceive.Split('\n');   //Divido tutto il contenuto della richiesta, utilizzando '\n'
                string[] requested = lines[0].Split((char)32);   //Divido il contenuto della prima riga, utilizzando ' '

                //Console.WriteLine(lines[0]);
                //foreach (var i in requested)
                //Console.WriteLine(i);

                string line=null;


                if (requested[1]=="/")
                {
                    if (File.Exists(wwwData + root))
                    {
                        line = File.ReadAllText(wwwData + root);
                        int lenght = line.Length;
                        dataSend = "HTTP/1.1 200 OK\r\nDate: "+DateTime.Now.ToString()+"\r\nServer: WebNett\r\nLast-Modified: "+File.GetLastWriteTime(wwwData + root).ToString()+"\r\nContent-Lenght: " + lenght + "\r\nKeep-Alive: timeout=10, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n" + line;
                    }
                    else
                    {
                        dataSend = "HTTP/1.1 404 Not found\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nKeep-Alive: timeout=10, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n";
                    }
                }
                else
                {
                    if (File.Exists(wwwData + requested[1]))
                    {
                        line = File.ReadAllText(wwwData + requested[1]);
                        int lenght = line.Length;
                        dataSend = "HTTP/1.1 200 OK\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nLast-Modified: " + File.GetLastWriteTime(wwwData + requested[1]).ToString() + "\r\nContent-Lenght: " + lenght + "\r\nKeep-Alive: timeout=10, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n" + line;
                    }
                    else
                        dataSend = "HTTP/1.1 404 Not found\r\nDate: "+DateTime.Now.ToString()+"\r\nServer: WebNett\r\nKeep-Alive: timeout=10, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n";

                }


                //dataSend = "HTTP/1.1 200 OK\r\nDate: " + DateTime.Now.ToString() + "\r\nServer: WebNett\r\nLast-Modified: " + File.GetLastWriteTime(wwwData + root).ToString() +
                //"\r\nContent-Lenght: " + lenght + "\r\nKeep-Alive: timeout=10, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n" + line;

                msg = Encoding.ASCII.GetBytes(dataSend);
                client.Send(msg);

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }


        private void btnStart_Click_1(object sender, EventArgs e)
        {
            if (started == false)
            {
                Listener l = new Listener();
                Thread ui = new Thread(new ThreadStart(l.startListening));
                started = true;
                ui.Start();
                File.AppendAllText(log, DateTime.Now.ToString() + "\tInfo\tServer Avviato\n");
                btnStop.Enabled = true;
            }
            else
            {
                MessageBox.Show("Server già in ascolto!", "!ERRORE!");
            }
        }

        private void btnStop_Click_1(object sender, EventArgs e)
        {
            Listener l = new Listener();
            l.stopListening();
            File.AppendAllText(log, DateTime.Now.ToString() + "\tInfo\tServer Stoppato\n");
        }
    }
}
