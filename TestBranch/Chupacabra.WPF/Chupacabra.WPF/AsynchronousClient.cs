using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;

namespace KinectoPhone.Desktop
{
    public class AsynchronousClient
    {
        private const int port = 13001;

        // ManualResetEvent instances signal completion.
        private  ManualResetEvent connectDone = new ManualResetEvent(false);
        private  ManualResetEvent sendDone = new ManualResetEvent(false);
        private  ManualResetEvent receiveDone = new ManualResetEvent(false);

        internal event ResponseReceivedEventHandler ResponseReceived;

        BackgroundWorker backgroundWorker = new BackgroundWorker();

        // The response from the remote device.
        private  String response = String.Empty;
        Socket client;
        IPEndPoint remoteEndPoint;

        public AsynchronousClient()
        {
            // The worker that is constantly syncing the state of the 
            // desktop client with the server.
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);

            IPHostEntry ipHost = Dns.GetHostEntry("192.168.1.6");

            var ipv4address = (from c in ipHost.AddressList where c.AddressFamily == AddressFamily.InterNetwork select c).First();

            if (ipv4address != null)
            {
                IPAddress ipAddress = (IPAddress)ipv4address;
                remoteEndPoint = new IPEndPoint(ipAddress, port);

                InitializeClient();
            }
        }

        void InitializeClient()
        {
            // Create a TCP/IP socket.
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            client.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!client.Connected)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                InitializeClient();
            }

            DesktopData cd = (DesktopData)e.Argument;
            Send(client, string.Format("c|{0}", cd.Position));
            sendDone.WaitOne();

            // Receive the response from the remote device.
            Receive(client);
            receiveDone.WaitOne();
        }

        public void SendData(int position)
        {
            if(!backgroundWorker.IsBusy)
                backgroundWorker.RunWorkerAsync(new DesktopData() { Position = position });
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        // Respond to the client in the UI thread to tell him that data was received
                        response = state.sb.ToString();
                        ResponseReceivedEventArgs args = new ResponseReceivedEventArgs();
                        args.Response = response;
                        OnResponseReceived(args);
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        protected void OnResponseReceived(ResponseReceivedEventArgs e)
        {
            if (ResponseReceived != null)
                ResponseReceived(this, e);
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            try
            {
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }
            catch (SocketException e)
            { Debug.WriteLine(e.ToString()); }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to server.", bytesSent);


                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }

    // A delegate type for hooking up change notifications.
    public delegate void ResponseReceivedEventHandler(object sender, ResponseReceivedEventArgs e);

    public class ResponseReceivedEventArgs : EventArgs
    {
        public bool IsError { get; set; }

        public string Response { get; set; }
    }

    public class DesktopData
    {
        public int Position { get; set; }
    }
}
