using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Server
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
        public static Dictionary<string, string> login = new Dictionary<string, string>();
        public static Dictionary<string, int> cookies = new Dictionary<string, int>();

        public static string accessList = @"access.csv";
        public static string directory = "config";
        public static string log = @"config/server.log";
        public static string wwwData = "www";
        public static string cookie = @"config/cookie.conf";


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                if(!File.Exists(log))
                    File.Create(log);
            }
            catch(Exception a)
            {
                MessageBox.Show("Errore nella creazione della prima configurazione! \n" + a.ToString(), "!ERRORE!");
            }

            if(!started)
                btnStop.Enabled = false;

            /*
            string[] lines = File.ReadAllLines(accessList);
            foreach(var line in lines)
            {
                string[] tmp = line.Split(';');
                login.Add(tmp[0], tmp[1]);
            }
            */


            /*
            string[] lines = File.ReadAllLines(cookie);
            foreach(var line in lines)
            {
                string[] doc = line.Split(';');
                cookies.Add(doc[0], Convert.ToInt32(doc[1]));
            }
            */

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
            string http = @"http.txt";
            //bool logged = false;
            Random r = new Random();

            public Manager(Socket client)
            {
                this.client = client;
            }

            public void acceptClient()
            {
                byte[] msg;
                int bytesRec;

                string[] doc = client.RemoteEndPoint.ToString().Split(':');
                //File.AppendAllText(log, DateTime.Now.ToString() + "\tInfo\tConnessione accettata per " + doc[0] + " su porta " + doc[1] + "\n");

                bytesRec = client.Receive(bytes);
                dataReceive = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                string[] message = dataReceive.Split('\n');

                Console.WriteLine(dataReceive);

                lock (http)
                {
                    using (StreamWriter sw = new StreamWriter(@"http.txt"))
                        sw.Write(dataReceive);
                }

                while (dataReceive != "Quit$")
                {
                   

                    //byte[] msg = Encoding.ASCII.GetBytes(dataSend);
                    //client.Send(msg);
                }
                

                //client.Shutdown(SocketShutdown.Both);
                //client.Close();

                dataReceive = "";
                dataSend = "";
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (started==false)
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

        private void btnStop_Click(object sender, EventArgs e)
        {
            Listener l = new Listener();
            l.stopListening();
            File.AppendAllText(log, DateTime.Now.ToString() + "\tInfo\tServer Stoppato\n");
        }

        
    }
}
