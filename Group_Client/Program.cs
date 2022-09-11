using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Group_Client
{
    class Program
    {
        /// <summary>
        /// Server IP
        /// Tutaj było moje IP ale nie mam zamiaru ci go podawać
        /// </summary>
        private const string ServerIP = "---------";

        /// <summary>
        /// buffer for receiving size of message
        /// </summary>
        private static byte[] msgSize;

        /// <summary>
        /// local socket
        /// </summary>
        private static Socket _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// describes size (in bytes) of first message protocol
        /// read more about how do I send messages
        /// </summary>
        private const int HEADER = 64;

        /// <summary>
        /// local port
        /// </summary>
        private const int PORT = 5050;

        static void Main(string[] args)
        {
            StartUp();
            Console.ReadLine();
        }//-------------------------------------------------------------------




        private static void StartUp()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), PORT);

            //connect to server
            try
            {
                Print(string.Format("Connecting to {0}...", ServerIP), ConsoleColor.Green);
                _client.Connect(iPEndPoint);

                if (_client.Connected)
                    Print("Connected", ConsoleColor.Green);
                msgSize = new byte[HEADER];
                _client.BeginReceive(msgSize, 0, HEADER, SocketFlags.None, new AsyncCallback(HeaderReceivedCallback), null);
                /*while (true)
                {
                }*/
            }
            catch (SocketException e)
            {
                Print("[SERVER IS NOT RUNNING]", ConsoleColor.Red);
                Console.ReadLine();
                Print(e.Message, ConsoleColor.Red);
            }
            catch (Exception e)
            {
                Print(string.Format("[FATAL ERROR OCCURED] {0}", e.Message), ConsoleColor.Red);
            }
        }//--------------------------------------------------------------------------------------

        private static void Print(string msg, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
        }//--------------------------------------------------------------------------------------

        private static void HeaderReceivedCallback(IAsyncResult ar)
        {
            int byteSize = _client.EndReceive(ar);
            Print("Message received from [SERVER]", ConsoleColor.Yellow);
            Socket server = ar.AsyncState as Socket;
            string data = Encoding.UTF8.GetString(msgSize).Trim();
            byte[] buffer = new byte[int.Parse(data)];
            //_client.ReceiveBufferSize = buffer.Length;
            _client.Receive(buffer);

            string message = Encoding.UTF8.GetString(buffer);
            Print(message);
            _client.BeginReceive(msgSize, 0, HEADER, SocketFlags.None, new AsyncCallback(HeaderReceivedCallback), null);
        }//---------------------------------------------------------------------------------------
    }
}
