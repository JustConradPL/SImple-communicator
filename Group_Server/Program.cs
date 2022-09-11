using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Group_Server
{
    class Program
    {

        /// <summary>
        /// local port
        /// </summary>
        private const int PORT = 5050;

        /// <summary>
        /// Time format day-month-year hour-minute
        /// </summary>
        private const string TimeFormat = "dd-MM-yy HH:mm";

        /// <summary>
        /// Server socket
        /// </summary>
        private static Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Header size
        /// </summary>
        private const int HEADER = 64;

        /// <summary>
        /// IP of server
        /// </summary>
        private static string _hostIP;

        /// <summary>
        /// server endpoint
        /// </summary>
        private static IPEndPoint _hostAddr;

        // Use these for dealing with messages
        static List<Message> MessageHistory = new List<Message>();
        static Queue<Message> Messages = new Queue<Message>();
        private static byte[] msgRec = new byte[16];


        //list of users that already connected
        static List<Socket> Sockets = new List<Socket>();
        static Dictionary<string, string> users = new Dictionary<string, string>();
        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

        static void Main(string[] args)
        {
            StartUp();
        }//-----------------------------------------------------------------------------

        private static void StartUp()
        {
            #region Get Basic info
#pragma warning disable
            IPHostEntry iPHostEntry = Dns.GetHostByName(Dns.GetHostName());
            _hostIP = iPHostEntry.AddressList[0].ToString();
            _hostAddr = new IPEndPoint(IPAddress.Parse(_hostIP), PORT);
#pragma warning enable
            #endregion

            try
            {
                Print(string.Format("Booting server at {0}...", _hostIP), ConsoleColor.Green);
                _server.Bind(_hostAddr);

                Print("Starting listening...", ConsoleColor.Green);
                _server.Listen(25);

                Thread thread = new Thread(Core);
                thread.Start();
                Thread thread1 = new Thread(DeleteDisconnected);
                thread1.Start();
                _server.BeginAccept(new AsyncCallback(AcceptCallback), null);
                //Console.ReadLine();
            }
            catch (Exception e)
            {
                Print(string.Format("[SERVER FATAL ERROR] {0}", e.Message), ConsoleColor.Red);
            }
        }//-------------------------------------------------------------------------------

        private static void AcceptCallback(IAsyncResult AR)
        {
            string message = string.Empty;
            Socket client = _server.EndAccept(AR);
            Sockets.Add(client);
            IPEndPoint address = client.LocalEndPoint as IPEndPoint;
            Print(string.Format("[SERVER] client connected from {0}", address.Address.ToString()), ConsoleColor.Green);
            if (!users.Keys.Contains(address.Address.ToString()))
            {
                users.Add(address.Address.ToString(), $"Unkown User{users.Count + 1}");
                Print("[SERVER] client added to users", ConsoleColor.Green);
                message = "Hello my dude. Rememember to be nice to each other";
            }
            else
                message = "Hello again";
            Print("[SERVER] Sending message list back to user",ConsoleColor.Green);

            //Send Message

            foreach (var item in MessageHistory)
            {
                SendMessage(item.message, client, true);
            }
            
            //Resume accepting
            Print(string.Format("Currently connected:\t{0}", Sockets.Count));
            _server.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }//-------------------------------------------------------------------------

        /// <summary>
        /// Check if socket is still connected 
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static bool IsSocketAvailable(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }//----------------------------------

        private static int SendMessage(string msg, Socket client, bool log = false)
        {
            //encode basic stuff as well as a message
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            byte[] head = new byte[HEADER];
            head = Encoding.UTF8.GetBytes(buffer.Length.ToString());

            try
            {
                client.Send(head);
                Thread.Sleep(100);
                client.Send(buffer);
                return buffer.Length;
            }
            catch (Exception e)
            {
                if (log)
                    Print(string.Format("[SERVER ERROR] {0}", e.Message), ConsoleColor.Red);
                return -1;
            }
        }//------------------------------------------------------------------------------------

        private static void Core()
        {
            while (true)
            {
                string msg = Console.ReadLine();
                MessageHistory.Add(new Message(msg, _hostIP, DateTime.Now));
            Start:
                foreach (var item in Sockets)
                {
                    if (IsSocketAvailable(item) == false)
                    {
                        Print(string.Format("{0} disconnected from server", ((IPEndPoint)item.LocalEndPoint).Address.ToString()), ConsoleColor.Yellow);
                        Sockets.Remove(item);
                        goto Start;
                    }
                    SendMessage(msg, item);
                }
            }
        }//------------------------------------------------------------------------------------

        private static void DeleteDisconnected()
        {
            while (true)
            {
                foreach (var item in Sockets)
                {
                    if (IsSocketAvailable(item) == false)
                    {
                        Print(string.Format("{0} disconnected from server", ((IPEndPoint)item.LocalEndPoint).Address.ToString()), ConsoleColor.Yellow);
                        Sockets.Remove(item);
                        break;
                    }
                }
                Thread.Sleep(10000);
            }
        }//-----------------------------------------------------------------------------------

        private static void Print(string msg, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

    }
}
