using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_X_Login_Server
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

        private Thread ConnectionThread;

        #region Connection
        public int Index = -1;
        public string IP = "";
        public string Username = "";
        public string SessionID = "";
        public bool Connected = false;
        public DateTime ConnectedTime = default(DateTime);
        #endregion

        #region Network
        private byte[] ReadBuff;
        public TcpClient Socket;
        public NetworkStream Stream;
        #endregion

        public Connection(ConnectionType type, int id)
        {
            Type = type;
            Index = id;
        }

        public virtual void Start()
        {
            ConnectedTime = DateTime.Now;
            ConnectionThread = new Thread(new ThreadStart(BeginThread));
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

            // Rejoin main thread
            ConnectionThread.Join();
        }

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
                Log.log("An error occurred when beginning the streams read. > " + e.Message, Log.LogType.ERROR);
            }
        }
        private void HandleAsyncConnection(IAsyncResult result)
        {
            StartAccept();
            OnReceiveData(result);
        }

        public void OnReceiveData(IAsyncResult result)
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
                ProcessData.processData(Index, Bytes);

                Stream.BeginRead(ReadBuff, 0, Socket.ReceiveBufferSize, OnReceiveData, null);
            }
            catch (Exception e)
            {
                // Output error message
                Log.log("An error occured while receiving data. > " + e.Message, Log.LogType.ERROR);

                // Send DB updates
                Database.instance.LogActivity(Username, Activity.DISCONNECT, SessionID);

                // Close the connection
                Close();

                return;
            }
        }
    }
}
