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
        public DateTime NextConnectAttempt = default(DateTime);
        #endregion

        #region Threads
        private static Thread ConnectionThread;
        #endregion

        #region Authentication
        public bool Authenticated = false;
        #endregion

        public Connection(ConnectionType type, int id, int port, string ip)
        {
            IP = ip;
            Type = type;
            Index = id;
            Port = port;
        }

        public virtual void Close()
        {
            // Connection
            IP = "";
            Port = 0;
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
        }

        public void Start()
        {
            ConnectionThread = new Thread(new ThreadStart(AttemptConnect));
            ConnectionThread.Start();
        }

        public void AttemptConnect()
        {
            while (!Connected && ConnectionAttemptCount <= 5)
            {
                if (DateTime.Now >= NextConnectAttempt)
                {
                    ++ConnectionAttemptCount;
                    Log.log("Attempt #" + ConnectionAttemptCount.ToString() + ", connecting to " + Type.ToString() + "..", Log.LogType.SYSTEM);
                    Connect();
                    NextConnectAttempt = DateTime.Now.AddSeconds(SecondsBetweenConnectionAttempts);
                }
            }
            // Send authentication packet
            SendData.Authenticate(this);
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
                Log.log("Connection to " + Type.ToString() + ", attempt #" + ConnectionAttemptCount.ToString() + " failed with the following error > " + e.Message + " > Trying again in " + SecondsBetweenConnectionAttempts.ToString() + " seconds.", Log.LogType.ERROR);
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
                Log.log("Connection to " + Type.ToString() + ", attempt #" + ConnectionAttemptCount.ToString() + " failed with the following error > " + e.Message + " > Trying again in " + SecondsBetweenConnectionAttempts.ToString() + " seconds.", Log.LogType.ERROR);
                return;
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
            } catch (Exception e)
            {
                Log.log("An error occurred when receiving data. > " + e.Message, Log.LogType.ERROR);
            }
        }
        public void Disconnect()
        {
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
            Log.log(Type.ToString() + " disconnected.", Log.LogType.CONNECTION);
        }
    }
}
