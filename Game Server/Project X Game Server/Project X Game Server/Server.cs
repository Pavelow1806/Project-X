using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net.Sockets;

namespace Project_X_Game_Server
{
    class Server : Connection
    {
        private DateTime TimeUntilRelease = default(DateTime);

        #region Additional TCP Requirements for Server Listening
        public int Port = 0;
        public StreamReader Reader = null;
        public StreamWriter Writer = null;
        public bool ShouldHandleData = false;
        public byte[] asyncBuff = null;
        #endregion

        #region ConnectionAttempts
        public int ConnectionAttemptCount = 0;
        private int SecondsBetweenConnectionAttempts = 5;
        private const int MaxConnectionAttempts = 5;
        public DateTime NextConnectAttempt = default(DateTime);
        #endregion

        #region Threads
        private Thread AuthenticationThread;
        private static Thread ConnectionThread;
        #endregion

        #region Authentication
        public bool Authenticated = false;
        #endregion

        int LineNumber = -1;

        public Server(ConnectionType type, int id, int port, string ip) :
            base(type, id)
        {
            Port = port;
            IP = ip;
        }

        public override void Start()
        {
            if (Type == ConnectionType.SYNCSERVER)
            {
                TimeUntilRelease = ConnectedTime.AddSeconds(Network.SecondsToAuthenticateBeforeDisconnect);
                AuthenticationThread = new Thread(new ThreadStart(CheckAuthentication));
                AuthenticationThread.Start();
                ConnectionThread = new Thread(new ThreadStart(AttemptConnect));
                ConnectionThread.Start();
            }
            else
            {
                base.Start();
            }
        }

        public override void Close()
        {
            base.Close();
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
        public void CheckAuthentication()
        {
            while (DateTime.Now < TimeUntilRelease || !Authenticated)
            {

            }
            if (Authenticated)
            {
                string msg = "";
                if (Network.instance.SyncServerAuthenticated)
                {
                    msg = "ready for client connections.";
                }
                Log.log("Authentication of " + Type.ToString() + " successful, " + msg, Log.LogType.SUCCESS);
            }
            else
            {
                Log.log("Authentication of Server failed, releasing socket.", Log.LogType.ERROR);
                Close();
            }
            AuthenticationThread.Join();
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
                        Stream.BeginRead(asyncBuff, 0, Network.BufferSize * 2, OnReceive, null);
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
                        Close();
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
                Log.log("An error occurred when receiving data. > " + e.Message, Log.LogType.ERROR);
            }
        }
    }
}
