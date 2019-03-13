using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    public enum ConnectionType
    {
        GAMESERVER,
        CLIENT,
        LOGINSERVER,
        SYNCSERVER
    }
    class Connection
    {
        public ConnectionType Type;

        protected Thread ConnectionThread;

        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        #region Connection
        public int Index = -1;
        public string IP = "";
        public int Port = 0;
        public string Username = "";
        public string SessionID = "";
        public bool Connected = false;
        public DateTime ConnectedTime = default(DateTime);
        public CommunicationType Communication;
        #endregion

        #region Network
        protected byte[] ReadBuff;
        public TcpClient Socket;
        public NetworkStream Stream;
        #endregion

        int LineNumber = -1;
        #region Authentication
        public bool Authenticated = false;
        #endregion
        public bool ShouldHandleData = false;
        public int ConnectionAttemptCount = 0;
        private int SecondsBetweenConnectionAttempts = 5;
        private const int MaxConnectionAttempts = 5;
        public DateTime NextConnectAttempt = default(DateTime);


        public Connection(ConnectionType type, int id, CommunicationType communication)
        {
            Type = type;
            Index = id;
            Communication = communication;
        }

        #region Generic Connectivity Functions
        public virtual void Start()
        {
            ConnectedTime = DateTime.Now;
            switch (Communication)
            {
                case CommunicationType.Listen:
                    // Start new listener thread
                    ConnectionThread = new Thread(new ThreadStart(BeginThread));
                    break;
                case CommunicationType.Send:
                    // Start new thread for 
                    ConnectionThread = new Thread(new ThreadStart(AttemptConnect));
                    break;
                default:
                    break;
            }
            ConnectionThread.Start();
        }
        public virtual void Close()
        {
            // Connection
            IP = "";
            Username = "";
            ConnectedTime = default(DateTime);

            // Network
            ReadBuff = null;
            if (Stream != null)
            {
                Stream.Close();
                Stream = null;
            }
            if (Socket != null)
            {
                Socket.Close();
                Socket = null;
            }
            if (Type == ConnectionType.GAMESERVER || Type == ConnectionType.SYNCSERVER)
            {
                Network.instance.Servers.Remove(Type);
            }
            // Rejoin main thread
            ConnectionThread.Join();
        }
        #endregion

        public void BeginThread()
        {
            Socket.SendBufferSize = Network.BufferSize;
            Socket.ReceiveBufferSize = Network.BufferSize;
            Stream = Socket.GetStream();
            Array.Resize(ref ReadBuff, Socket.ReceiveBufferSize);
            StartAccept();
        }

        private void StartAccept()
        {
            try
            {
                if (Stream != null)
                {
                    if (Connected)
                    {
                        Stream.BeginRead(ReadBuff, 0, Socket.ReceiveBufferSize, HandleAsyncConnection, null);
                    }
                }
            }
            catch (Exception e)
            {
                //Log.log("An error occurred when beginning the streams read. > " + e.Message, Log.LogType.ERROR);
            }
        }
        private void HandleAsyncConnection(IAsyncResult result)
        {
            StartAccept();
            OnReceiveData(result);
        }

        public void OnReceiveData(IAsyncResult result)
        {
            lock (lockObj)
            {
                try
                {
                    int ReadBytes = Stream.EndRead(result);
                    if (Socket == null)
                    {
                        return;
                    }
                    if (ReadBytes <= 0)
                    {
                        Close();
                        return;
                    }
                    byte[] Bytes = null;
                    Array.Resize(ref Bytes, ReadBytes);
                    Buffer.BlockCopy(ReadBuff, 0, Bytes, 0, ReadBytes);

                    // Process the packet
                    ProcessData.processData(Bytes);

                    Stream.BeginRead(ReadBuff, 0, Socket.ReceiveBufferSize, OnReceiveData, null);
                }
                catch (Exception e)
                {
                    // Output error message
                    Log.log("An error occured while receiving data. Closing connection to " + Type.ToString() + ((Type == ConnectionType.CLIENT) ? " Index " + Index.ToString() : "."), Log.LogType.ERROR);

                    // Close the connection
                    Close();

                    return;
                }
            }
        }
        public void AttemptConnect()
        {
            while (!Connected && ConnectionAttemptCount < MaxConnectionAttempts)
            {
                if (DateTime.Now >= NextConnectAttempt)
                {
                    ++ConnectionAttemptCount;
                    if (LineNumber == -1)
                    {
                        LineNumber = Log.log("Attempt #" + ConnectionAttemptCount.ToString() + ", connecting to " + Type.ToString() + "..", Log.LogType.SYSTEM);
                    }
                    else
                    {
                        Log.log(LineNumber, "Attempt #" + ConnectionAttemptCount.ToString() + ", connecting to " + Type.ToString() + "..", Log.LogType.SYSTEM);
                    }
                    Connect();
                    NextConnectAttempt = DateTime.Now.AddSeconds(SecondsBetweenConnectionAttempts);
                }
            }
            if (Connected)
            {
                // Send authentication packet
                SendData.Authenticate((Server)this);
            }
            else
            {
                Log.log(LineNumber, "Connection to " + Type.ToString() + " unsuccessful, the retry attempts reached the maximum (" + MaxConnectionAttempts.ToString() + "), type connect [SERVER NAME] to reattempt.", Log.LogType.ERROR);
            }
            //Rejoin the thread
            ConnectionThread.Join();
        }
        public void Connect()
        {
            try
            {
                if (Socket != null)
                {
                    if (Socket.Connected || Connected)
                    {
                        Log.log(Type.ToString() + " is already connected.", Log.LogType.CONNECTION);
                        return;
                    }
                    Socket.Close();
                    Socket = null;
                }
                Socket = new TcpClient();
                Socket.ReceiveBufferSize = Network.BufferSize;
                Socket.SendBufferSize = Network.BufferSize;
                Socket.NoDelay = false;
                Array.Resize(ref ReadBuff, Network.BufferSize * 2);
                Socket.BeginConnect(IP, Port, new AsyncCallback(ConnectCallback), Socket);
            }
            catch (Exception e)
            {
                if (ConnectionAttemptCount >= 5)
                {
                    Log.log(LineNumber, "Connection to " + Type.ToString() + " unsuccessful, the retry attempts reached the maximum (" + MaxConnectionAttempts.ToString() + "), type connect [SERVER NAME] to reattempt.", Log.LogType.ERROR);
                }
                else
                {
                    Log.log(LineNumber, "Connection to " + Type.ToString() + ": Attempt " + ConnectionAttemptCount.ToString() + " / " + MaxConnectionAttempts.ToString() + " failed. Trying again in " + SecondsBetweenConnectionAttempts.ToString() + " seconds.", Log.LogType.SYSTEM);
                }
            }
        }
        void ConnectCallback(IAsyncResult result)
        {
            try
            {
                if (Socket != null)
                {
                    Socket.EndConnect(result);
                    if (Socket.Connected == false)
                    {
                        Connected = false;
                        Close();
                        return;
                    }
                    else
                    {
                        Socket.NoDelay = true;
                        Stream = Socket.GetStream();
                        Stream.BeginRead(ReadBuff, 0, Network.BufferSize * 2, OnReceiveData, null);
                        ConnectedTime = DateTime.Now;
                        Log.log("Connection to " + Type.ToString() + ", Successful.", Log.LogType.CONNECTION);
                        Connected = true;
                    }
                }
            }
            catch (Exception e)
            {
                if (ConnectionAttemptCount >= 5)
                {
                    Log.log(LineNumber, "Connection to " + Type.ToString() + " unsuccessful, the retry attempts reached the maximum (" + MaxConnectionAttempts.ToString() + "), type connect [SERVER NAME] to reattempt.", Log.LogType.ERROR);
                }
                else
                {
                    Log.log(LineNumber, "Connection to " + Type.ToString() + ": Attempt " + ConnectionAttemptCount.ToString() + " / " + MaxConnectionAttempts.ToString() + " failed. Trying again in " + SecondsBetweenConnectionAttempts.ToString() + " seconds.", Log.LogType.SYSTEM);
                }
            }
        }
    }
}
