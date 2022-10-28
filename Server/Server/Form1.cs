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

        public static string directory = "config";
        public static string serverLog = @"config/server.log";


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                if(!File.Exists(serverLog))
                    File.Create(serverLog);
            }
            catch(Exception a)
            {
                MessageBox.Show("Errore nella creazione della prima configurazione!", "!!ERRORE!!");
            }
        }


        class Listener
        {
            public void startListening()
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 800);

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
                        File.AppendAllText(serverLog, DateTime.Now.ToString() + "\tInfo\tNuova connessione in entrata\n");
                    }
                }
                catch (Exception a)
                {
                    Console.WriteLine(a.ToString());
                    MessageBox.Show(a.ToString(), "!ERRORE!");
                    File.AppendAllText(serverLog, DateTime.Now.ToString() + "\tError\t" + a.ToString() + "\n");
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
                    File.AppendAllText(serverLog, DateTime.Now.ToString() + "\tError\t" + a.ToString() + "\n");
                }
            }
        }

        class Manager
        {
            Socket client;
            byte[] bytes = new byte[1024];
            string data = "";

            public Manager(Socket client)
            {
                this.client = client;
            }

            public void acceptClient()
            {
                string[] doc = client.RemoteEndPoint.ToString().Split(':');
                File.AppendAllText(serverLog, DateTime.Now.ToString() + "\tInfo\tConnessione accettata per " + doc[0] + " su porta " + doc[1] + "\n");

                while (data != "Quit$")
                {

                    Console.WriteLine(client.RemoteEndPoint.ToString());

                    data = "";
                    while (data.IndexOf("$") == -1)
                    {
                        int bytesRec = client.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    }

                    Console.WriteLine("Messaggio ricevuto : {0}", data);

                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    client.Send(msg);
                }
                //client.Shutdown(SocketShutdown.Both);
                //client.Close();
                data = "";
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (started!=true)
            {
                Listener l = new Listener();
                Thread ui = new Thread(new ThreadStart(l.startListening));
                ui.Start();
                started = true;
                File.AppendAllText(serverLog, DateTime.Now.ToString() + "\tInfo\tServer Avviato\n");
            }
            else
                MessageBox.Show("Server già in ascolto!", "!ERRORE!");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Listener l = new Listener();
            l.stopListening();
            File.AppendAllText(serverLog, DateTime.Now.ToString() + "\tInfo\tServer Stoppato\n");
        }

        
    }
}
