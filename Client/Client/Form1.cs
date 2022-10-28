using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static bool check=false;
        public static bool started = false;
        
        public static string log = @"client.log";




        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(log))
                File.Create(log);
        }

        public void listening()
        {
            started = !started;

            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 800);

                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                Random stringa_casuale = new Random();

                Manager m = new Manager(sender, ipAddress, remoteEP);

                if (started == true)
                    m.start();
                else
                    m.stop();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                File.AppendAllText(log, DateTime.Now.ToString() + "\tError\tGeneric Error: " + e.ToString() + "\n");
            }
        }

        

        class Manager
        {
            Socket socket;
            IPAddress ipAddress;
            IPEndPoint IPEndPoint;

            public Manager(Socket socket, IPAddress iPAddress, IPEndPoint iPEndPoint)
            {
                this.socket = socket;
                this.ipAddress = iPAddress;
                this.IPEndPoint = iPEndPoint;
            }

            public void start()
            {
                byte[] bytes = new byte[1024];
                int count = 0;
                string dataReceived = "";
                string dataSend = "";
                Random stringa_casuale = new Random();


                try
                {
                    socket.Connect(IPEndPoint);         //To quit connection, use "Quit$"

                    Console.WriteLine("Socket connected to {0}",
                        socket.RemoteEndPoint.ToString());

                    Console.WriteLine(ipAddress.ToString());

                    dataSend = ipAddress.ToString() + ";";

                    while (dataReceived != "Quit$")
                    {


                        byte[] msg = Encoding.ASCII.GetBytes(dataSend);              //("This is a test<EOF>");

                        int bytesSent = socket.Send(msg);
                        dataReceived = "";

                        while (dataReceived.IndexOf("$") == -1)
                        {
                            int bytesRec = socket.Receive(bytes);
                            dataReceived += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        }
                        Console.WriteLine("Messaggio ricevuto: " + dataReceived);
                        Thread.Sleep(1000);
                        count++;
                    }
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    File.AppendAllText(log, DateTime.Now.ToString() + "\tError\tArgumentNull Exception: " + ane.ToString() + "\n");
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    File.AppendAllText(log, DateTime.Now.ToString() + "\tError\nSocket Exception: " + se.ToString() + "\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    File.AppendAllText(log, DateTime.Now.ToString() + "\tError\tUnexpected Exception: " + e.ToString() + "\n");
                }
            }

            public void stop()
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }



        private void btnStartStop_Click(object sender, EventArgs e)
        {
            Thread ui = new Thread(listening);
            ui.Start();
            check = true;

            if (check == true)
            {

            }
        }
    }
}
