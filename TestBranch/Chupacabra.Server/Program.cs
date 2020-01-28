using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace KinectoPhone.Server
{

    public class StateObject
    {
        public Socket WorkSocket = null;
        public const int BUFFER_SIZE = 100;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder ContentString = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        public const int PORT_NUMBER = 13001;

        public static ManualResetEvent completed = new ManualResetEvent(false);
        static int angle, position;

        public static void StartListening()
        {
            byte[] dataBuffer = new Byte[StateObject.BUFFER_SIZE];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PORT_NUMBER);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(2);

                while (true)
                {
                    completed.Reset();
                    Console.WriteLine("Async transmission started.");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback),listener);
                    completed.WaitOne();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            completed.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.ContentString.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                content = state.ContentString.ToString();

                Console.WriteLine("DATA LENGTH [RECEIVED] = {0} bytes. DATA = {1}", content.Length, content);

                ParseContent(content);
                Send(handler);
            }
        }

        private static void ParseContent(string content)
        {
            var values = content.Split('|');
            if (content.StartsWith("c"))
            {
                position = int.Parse(values[1]);
            }
            else if (content.StartsWith("p"))
            {
                angle = int.Parse(values[1]);
            }
        }

        private static void Send(Socket handler)
        {
            // The generalized string broadcasted by the server does not contain a platform
            // identifiying prefix (p or c)
            string data = string.Format("{0}|{1}", angle, position);

            byte[] byteData = Encoding.UTF8.GetBytes(data);

            Console.WriteLine("DATA LENGTH [SENT] = {0} bytes. DATA = {1}", data.Length, data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}
