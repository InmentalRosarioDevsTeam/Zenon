using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Com.Sockets
{
    public class ClientEventArgs : EventArgs
    {
        public TcpClient client = null;

        public ClientEventArgs(TcpClient pClient)
        {
            this.client = pClient;
        }
    }

    public class TCPServer
    {
        //---------------------------------------------------------------------------------------------------

        public int maxClients = 1;

        public event EventHandler<MessageEventArgs> MessageReceivedEvent;
        public event EventHandler<ClientEventArgs> OnClientConnectedEvent;
        public event EventHandler<ClientEventArgs> OnClientDisconnectedEvent;

        static public TCPServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TCPServer();
                }
                return _instance;
            }
        }

        public int port
        {
            get
            {
                return _port;
            }
        }

		public int clients
		{
			get
			{
				return _clients;
			}
		}

        public float videoProcess = 0;

        //---------------------------------------------------------------------------------------------------

        public TCPServer()
        {
        }
        public void Initialize(int port)
        {
            if (initialized) return;

            this._port = port;

			this.pendingMessagesForClients = new Dictionary<TcpClient, Stack<byte[]>> ();

            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.encoder = new UTF8Encoding();
            this.listenThread.Start();
            initialized = true;
        }
		public void SendMessage(string pMessage)
        {
            if (!initialized) return;

			SendMessage(encoder.GetBytes(pMessage + "\n") );
        }
		public void SendMessage(byte[] pMessage)
        {
            
            if (!initialized) return;

			lock (pendingMessagesForClients) {
				foreach (KeyValuePair<TcpClient,Stack<byte[]>> kv in pendingMessagesForClients) {
					kv.Value.Push (pMessage);
				}
			}
           
        }
        public void Close()
        {
            if (!initialized) return;

            ready = false;
            initialized = false;

            try
            {
                tcpListener.Stop();
                listenThread.Abort();
            }
            catch (Exception exc)
            {
            }
            
        }

        //---------------------------------------------------------------------------------------------------

        static private TCPServer _instance;

        private UTF8Encoding encoder;
        private TcpListener tcpListener;
        private Thread listenThread;
        private Boolean ready = true;
		private Dictionary<TcpClient,Stack<byte[]>> pendingMessagesForClients;

        private bool initialized = false;
        private int _clients = 0;
        private int _port = 0;

        //---------------------------------------------------------------------------------------------------

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (ready)
            {
                if (_clients < maxClients)
                {
                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

					lock (pendingMessagesForClients) {
						pendingMessagesForClients.Add (client, new Stack<byte[]> ());
					}

                    if (OnClientConnectedEvent != null)
                        OnClientConnectedEvent(this, new ClientEventArgs(client));

                    //create a thread to handle communication 
                    //with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);

                    _clients++;
                }
            }
        }
        private void HandleClientComm(object client)
        {
            
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            clientStream.ReadTimeout = 3000;
            clientStream.WriteTimeout = 1000;

            byte[] message = new byte[4096];
            int bytesRead;
            int connectionFails = 0;
            bool sendAvaliable = true;
			string buffer = "";

			Stack<Byte[]> pendingMessages = pendingMessagesForClients [tcpClient];

            while (ready)
            {
				
                #region Write

				lock(pendingMessages)
				{
					if (sendAvaliable && 
                        pendingMessages != null && 
                        pendingMessages.Count > 0)
	                {
	                    byte[] messageToSend = pendingMessages.Pop();
	  
	                    try
	                    {
	                        clientStream.Write(messageToSend, 0, messageToSend.Length);
	                        clientStream.Flush();
	                    }
	                    catch (ObjectDisposedException)
	                    {
	                        break;
	                    }
	                    catch (IOException)
	                    {
	                        break;
	                    }
	                    catch (Exception exc)
	                    {
	                        pendingMessages.Push(messageToSend);
	                        break;
	                    }
	                }
				}
                #endregion
				
                #region Read
                
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message

                    if (clientStream.DataAvailable)
                    {
                        bytesRead = clientStream.Read(message, 0, message.Length);

                        //sendAvaliable = bytesRead > 0;

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
                catch (Exception e)
                {
					UnityEngine.Debug.LogError(e.Message);
                    UnityEngine.Debug.LogError(e.StackTrace);
                    break;
                }
                
                #endregion

                //Como la propiedad "connected" solo devuelve "false" cuando se hace una operacion y se dispara una excepcion. En todo momento me retorna "true".
                //Entonces lo que hago es este truquito con los polls que funciona de 10.
                //"connectionFail" devuelve "true" si la conexion falla y "false" si esta funcionando ok.
                if (tcpClient.Connected)
                {
					bool connectionFail = bytesRead == 0 && 
										  tcpClient.Client.Poll(10, SelectMode.SelectWrite) &&
                                          tcpClient.Client.Poll(10, SelectMode.SelectRead) &&
                                          ( !tcpClient.Client.Poll(10, SelectMode.SelectError) ) ? true : false;

                    if (connectionFail)
                        connectionFails++;
                    else
                        connectionFails = 0;

                    if (connectionFails > 10)
                        break;
                }
                
                Thread.Sleep(30);
            }

            try
            {
                if (OnClientDisconnectedEvent != null)
                    OnClientDisconnectedEvent(this, new ClientEventArgs(tcpClient));

                _clients--;
                tcpClient.Close();
                clientStream.Close();
            }
            catch (Exception)
            {
            }
        }

        //---------------------------------------------------------------------------------------------------
    }
}

