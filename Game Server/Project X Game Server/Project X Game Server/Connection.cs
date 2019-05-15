using System;
using System.Collections.Generic;
using System.IO;
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
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (Socket != null && Socket.Client != null && Socket.Client.Connected)
                    {
                        if (Socket.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (Socket.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                Disconnect();
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        Disconnect();
                        return false;
                    }
                }
                catch
                {
                    Disconnect();
                    return false;
                }
            }
        }
        public DateTime ConnectedTime = default(DateTime);
        public CommunicationType Communication;
        public StreamReader Reader = null;
        public StreamWriter Writer = null;
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
        private const int MaxConnectionAttempts = -1;
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
                case CommunicationType.Receive:
                    // Start new listener thread for received connections from other servers
                    ConnectionThread = new Thread(new ThreadStart(BeginReceive));
                    break;
                case CommunicationType.Send:
                    // Start new thread for opening the connection from this server
                    ConnectionThread = new Thread(new ThreadStart(AttemptConnect));
                    break;
                default:
                    break;
            }
            SessionID = Index.ToString("000") + " - " + IP + " - " + ConnectedTime.ToString("yyyy/MM/dd hh:mm:ss");
            ConnectionThread.Start();
        }
        public virtual void Disconnect()
        {
            // Connection
            Log.log("Connection to " + Type.ToString() + " : " + IP.ToString() + " disconnected, socket " + Index.ToString() + " now free.", Log.LogType.CONNECTION);

            if (Type == ConnectionType.CLIENT)
            {
                IP = "";
            }
            Username = "";
            ConnectedTime = default(DateTime);
            Connected = false;

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
            if (Reader != null)
            {
                Reader.Close();
                Reader = null;
            }
            if (Writer != null)
            {
                Writer.Close();
                Writer = null;
            }
            Log.log(Type.ToString() + " disconnected.. " + ((Type != ConnectionType.CLIENT) ? " Attempting to connect again.." : ""), Log.LogType.CONNECTION);
            Start();
            // Rejoin main thread
            ConnectionThread.Join();
        }
        #endregion

        #region Send (Connect to another server)
        public void AttemptConnect()
        {
            while ((!Connected && ConnectionAttemptCount < MaxConnectionAttempts) || (!Connected && MaxConnectionAttempts == -1))
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
                Thread.Sleep(50);
            }
            if (Connected)
            {
                // Send authentication packet
                SendData.Authenticate(this);
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
                if (ConnectionAttemptCount >= MaxConnectionAttempts && MaxConnectionAttempts > -1)
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
                        Disconnect();
                        return;
                    }
                    else
                    {
                        Socket.NoDelay = true;
                        Socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                        Stream = Socket.GetStream();
                        Stream.BeginRead(ReadBuff, 0, Network.BufferSize * 2, OnReceive, null);
                        ConnectedTime = DateTime.Now;
                        Log.log("Connection to " + Type.ToString() + ", Successful.", Log.LogType.CONNECTION);
                        Connected = true;
                    }
                }
            }
            catch (Exception e)
            {
                if (ConnectionAttemptCount >= MaxConnectionAttempts && MaxConnectionAttempts > -1)
                {
                    Log.log(LineNumber, "Connection to " + Type.ToString() + " unsuccessful, the retry attempts reached the maximum (" + MaxConnectionAttempts.ToString() + "), type connect [SERVER NAME] to reattempt.", Log.LogType.ERROR);
                }
                else
                {
                    Log.log(LineNumber, "Connection to " + Type.ToString() + ": Attempt " + ConnectionAttemptCount.ToString() + " / " + MaxConnectionAttempts.ToString() + " failed. Trying again in " + SecondsBetweenConnectionAttempts.ToString() + " seconds.", Log.LogType.SYSTEM);
                }
            }
        }
        #endregion

        #region Receive (Received connection from another server/client)
        public void BeginReceive()
        {
            try
            {
                Socket.SendBufferSize = Network.BufferSize;
                Socket.ReceiveBufferSize = Network.BufferSize;
                Stream = Socket.GetStream();
                Array.Resize(ref ReadBuff, Socket.ReceiveBufferSize);
                StartAccept();
            }
            catch (Exception e)
            {
                Log.log("Connection lost to " + Type.ToString(), Log.LogType.ERROR);
                Disconnect();
            }
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
                Log.log("An error occurred when beginning the streams read. > " + e.Message, Log.LogType.ERROR);
            }
        }
        private void HandleAsyncConnection(IAsyncResult result)
        {
            StartAccept();
            OnReceive(result);
        }
        #endregion

        #region General Functions
        void OnReceive(IAsyncResult result)
        {
            lock (lockObj)
            {
                try
                {
                    if (Socket != null)
                    {
                        if (Socket == null)
                            return;

                        int readBytes = Stream.EndRead(result);
                        byte[] newBytes = null;
                        Array.Resize(ref newBytes, readBytes);
                        Buffer.BlockCopy(ReadBuff, 0, newBytes, 0, readBytes);

                        if (readBytes <= 0)
                        {
                            Disconnect();
                            return;
                        }

                        ProcessData.processData(Index, newBytes);

                        if (Socket == null)
                            return;
                        Stream.BeginRead(ReadBuff, 0, Socket.ReceiveBufferSize, OnReceive, null);
                    }
                }
                catch (Exception e)
                {
                    Log.log("An error occurred when receiving data. > " + e.Message, Log.LogType.ERROR);
                    Disconnect();
                }
            }
        }
        #endregion
    }
}
