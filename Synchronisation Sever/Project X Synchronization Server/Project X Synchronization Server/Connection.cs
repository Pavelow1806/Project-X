using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_X_Synchronization_Server
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

        #region Connection
        public int Index = -1;
        public int Port = 0;
        public string IP = "";
        public bool Connected = false;
        public DateTime ConnectedTime = default(DateTime);
        #endregion

        #region Network
        public TcpClient Socket = null;
        public NetworkStream Stream = null;
        public StreamReader Reader = null;
        public StreamWriter Writer = null;
        public byte[] asyncBuff = null;
        public bool ShouldHandleData = false;
        #endregion

        #region ConnectionAttempts
        public int ConnectionAttemptCount = 0;
        private int SecondsBetweenConnectionAttempts = 5;
        private const int MaxConnectionAttempts = -1;
        public DateTime NextConnectAttempt = default(DateTime);
        #endregion

        #region Threads
        private static Thread ConnectionThread;
        #endregion

        #region Authentication
        public bool Authenticated = false;
        #endregion

        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        int LineNumber = -1;

        public Connection(ConnectionType type, int id, int port, string ip)
        {
            IP = ip;
            Type = type;
            Index = id;
            Port = port;
        }

        public void Start()
        {
            ConnectionThread = new Thread(new ThreadStart(AttemptConnect));
            ConnectionThread.Start();
        }

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
                Array.Resize(ref asyncBuff, Network.BufferSize * 2);
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
                        Stream = Socket.GetStream();
                        Stream.BeginRead(asyncBuff, 0, Network.BufferSize * 2, OnReceive, null);
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
        void OnReceive(IAsyncResult result)
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
                    Buffer.BlockCopy(asyncBuff, 0, newBytes, 0, readBytes);

                    if (readBytes == 0)
                    {
                        Disconnect();
                        return;
                    }

                    ProcessData.processData(newBytes);

                    ShouldHandleData = true;

                    if (Socket == null)
                        return;
                    Stream.BeginRead(asyncBuff, 0, Network.BufferSize * 2, OnReceive, null);
                }
            }
            catch (Exception e)
            {
                Log.log("An error occurred when receiving data, trying to connect again.. > " + e.Message, Log.LogType.ERROR);
                Disconnect();
            }
        }
        public void Disconnect()
        {
            lock (lockObj)
            {
                if (Connected)
                {
                    if (Type == ConnectionType.CLIENT)
                    {
                        // Connection
                        IP = "";
                        Port = 0;
                    }
                    ConnectedTime = default(DateTime);

                    // Network
                    Connected = false;
                    if (Socket != null)
                    {
                        Socket.Close();
                        Socket = null;
                    }
                    if (Stream != null)
                    {
                        Stream.Close();
                        Stream = null;
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
                    ConnectionAttemptCount = 0;
                    Authenticated = false;
                    Connected = false;
                    Log.log(Type.ToString() + " disconnected, Attempting to connect again..", Log.LogType.CONNECTION);
                    Start();
                }
            }
        }
    }
}
