using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Com.Sockets
{

    public class TCPClient
    {
        //----------------------------------------------------------------------------------------------------------------------------------------

        public bool Connected
        {
            get
            {
                return client != null && client.Connected;
            }
        }
        public event EventHandler<MessageEventArgs> MessageReceivedEvent;
        public event EventHandler<EventArgs> OnConnect;
        public event EventHandler<EventArgs> OnDisconnect;


        //----------------------------------------------------------------------------------------------------------------------------------------

        public TCPClient(String _server, Int32 _port)
        {
            Init(_server, _port);
        }

        public void Init(String _server, Int32 _port)
        {
            ready = true;
            server = _server;
            port = _port;

            pendingMessages = new Stack<string>();
            timerConnect = new System.Threading.Timer(TimerConnectCallback, null, 0, 3000);
        }

        public void Close()
        {
            ready = false;
            try
            {
                if (client != null)
                {
                    NetworkStream clientStream = client.GetStream();

                    if (clientStream != null) clientStream.Close();

                    client.Close();
                }

                if (thread != null) thread.Abort();
            }
            catch (Exception)
            {

            }
        }

        public void SendMessage(String msg)
        {
            UnityEngine.Debug.Log("SendMessage(" + server + ":" + port + ") " + msg);

            lock (pendingMessages)
                pendingMessages.Push(msg + "\n");
        }

        //----------------------------------------------------------------------------------------------------------------------------------------

        private void TimerConnectCallback(object state)
        {
            if (ConnectToServer())
            {
                timerConnect.Dispose();
                return;
            }
        }

        private Boolean ConnectToServer()
        {
            try
            {
                client = new TcpClient(server, port);
                timerConnect.Dispose();
                ready = true;

                this.thread = new Thread(new ThreadStart(Worker));
                this.thread.Start();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void Worker()
        {
            while (ready)
            {
                TcpClient tcpClient = (TcpClient)client;
                if (tcpClient == null || !tcpClient.Connected) return;

                NetworkStream clientStream = tcpClient.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();

                clientStream.ReadTimeout = 500;
                clientStream.WriteTimeout = 100;

                byte[] message = new byte[4096];
                int bytesRead;
                int connectionFails = 0;
                string buffer = "";
                if (OnConnect != null) OnConnect(this, new EventArgs());

                while (true)
                {

                    #region Write

                    if (pendingMessages.Count > 0)
                    {
                        string currentMessage = "";
                        lock (pendingMessages)
                            currentMessage = pendingMessages.Pop();

                        byte[] messageToSend = encoder.GetBytes(currentMessage);

                        try
                        {
                            clientStream.Write(messageToSend, 0, messageToSend.Length);
                            clientStream.Flush();
                        }
                        catch (Exception)
                        {
                            pendingMessages.Push(currentMessage);
                        }
                    }

                    #endregion

                    #region Read

                    bytesRead = 0;

                    try
                    {
                        if (clientStream.DataAvailable)
                        {
                            //blocks until a client sends a message
                            bytesRead = clientStream.Read(message, 0, 4096);

                            if (bytesRead != 0)
                            {
                                buffer += encoder.GetString(message, 0, bytesRead);

                                while (buffer.IndexOf("\n") != -1)
                                {
                                    string m = buffer.Substring(0, buffer.IndexOf("\n"));
                                    buffer = buffer.Substring(m.Length + 1);

                                    EventHandler<MessageEventArgs> newEvent = MessageReceivedEvent;
                                    MessageEventArgs _arg = new MessageEventArgs(m);
                                    if (newEvent != null)
                                        newEvent(this, _arg);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }

                    #endregion

                    //Como la propiedad "connected" solo devuelve "false" cuando se hace una operacion y se dispara una excepcion. En todo momento me retorna "true".
                    //Entonces lo que hago es este truquito con los polls que funciona de 10.
                    //"connectionFail" devuelve "true" si la conexion falla y "false" si esta funcionando ok.
                    if (tcpClient.Connected)
                    {
                        bool connectionFail = tcpClient.Client.Poll(100, SelectMode.SelectWrite) &&
                                              tcpClient.Client.Poll(100, SelectMode.SelectRead) &&
                                              (!tcpClient.Client.Poll(100, SelectMode.SelectError)) ? true : false;

                        if (connectionFail)
                            connectionFails++;
                        else
                            connectionFails = 0;

                        if (connectionFails > 10)
                            break;
                    }

                    Thread.Sleep(10);
                }

                try
                {
                    tcpClient.Close();
                    clientStream.Close();
                }
                catch (Exception exception)
                {
                }

                if (OnDisconnect != null) OnDisconnect(this, new EventArgs());

                timerConnect = new System.Threading.Timer(TimerConnectCallback, null, 0, 3000);
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------------

        private static TcpClient client;

        private Thread thread;
        private Boolean ready = false;
        private String server;
        private Int32 port;
        private System.Threading.Timer timerConnect;
        private Stack<String> pendingMessages;

        //----------------------------------------------------------------------------------------------------------------------------------------
    }
}
