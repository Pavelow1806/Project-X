using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Project_X_Game_Server
{
    public enum ClientProcessPacketNumbers
    {
        Invalid,
        EnterWorld,
        Update
    }
    public enum LoginServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WhiteList
    }
    public enum GameServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer
    }
    public enum SyncServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WorldRequest
    }
    class ProcessData
    {
        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        private static int LineNumber = 0;

        public static void processData(int index, byte[] data)
        {
            lock (lockObj)
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                buffer.WriteBytes(data);

                ConnectionType Source = (ConnectionType)buffer.ReadInteger();
                int PacketNumber = buffer.ReadInteger();
                
                object[] obj;
                switch (Source)
                {
                    case ConnectionType.SYNCSERVER:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(SyncServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[(ConnectionType)index].Socket == null)
                        {
                            return;
                        }
                        Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((SyncServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.SYNCSERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);

                        obj = new object[3];
                        obj[0] = Source;
                        obj[1] = index;
                        obj[2] = data;

                        typeof(ProcessData).InvokeMember(((SyncServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    case ConnectionType.LOGINSERVER:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(LoginServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[ConnectionType.LOGINSERVER].Socket == null)
                        {
                            return;
                        }
                        Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((LoginServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.GAMESERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);

                        obj = new object[3];
                        obj[0] = Source;
                        obj[1] = index;
                        obj[2] = data;

                        typeof(ProcessData).InvokeMember(((LoginServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    case ConnectionType.CLIENT:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(ClientProcessPacketNumbers), PacketNumber) || Network.instance.Clients[index].Socket == null)
                        {
                            return;
                        }
                        //Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((ClientProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.CLIENT.ToString() + ", Processing response..", Log.LogType.RECEIVED);

                        obj = new object[3];
                        obj[0] = Source;
                        obj[1] = index;
                        obj[2] = data;

                        typeof(ProcessData).InvokeMember(((ClientProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ReadHeader(ref ByteBuffer.ByteBuffer buffer)
        {
            ConnectionType Source = (ConnectionType)buffer.ReadInteger();
            int PacketNumber = buffer.ReadInteger();
        }

        #region Server Communication
        private static void AuthenticateServer(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            if (buffer.ReadString() == Network.instance.AuthenticationCode)
            {
                // Confirmed to be the correct server, proceed with unblocking the client communication channels
                Network.instance.Servers[(ConnectionType)index].Authenticated = true;
                Network.instance.SyncServerAuthenticated = true;
                Network.instance.Servers.Add(type, Network.instance.Servers[(ConnectionType)index]);
                Network.instance.Servers.Remove((ConnectionType)index);
                Network.instance.Servers[type].Index = (int)type;
            }
        }
        #endregion
        
        #region Login Server Communication
        private static void WhiteList(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            string ip = buffer.ReadString();
            int LineNumber = Log.log("Checking white list for IP: " + ip.Substring(0, ip.IndexOf(':')) + "..", Log.LogType.RECEIVED);
            if (!Network.instance.CheckWhiteList(ip.Substring(0, ip.IndexOf(':'))))
            {
                Network.instance.WhiteList.Add(ip.Substring(0, ip.IndexOf(':')));
                Log.log(LineNumber, "Client IP: " + ip.Substring(0, ip.IndexOf(':')) + " added successfully, sending confirmation to login server.", Log.LogType.RECEIVED);
                SendData.ConfirmWhiteList(ip.Substring(0, ip.IndexOf(':')));
            }
            else
            {
                Log.log(LineNumber, "Client IP: " + ip.Substring(0, ip.IndexOf(':')) + " was already white listed.", Log.LogType.WARNING);
            }
        }
        #endregion

        #region Synchronization Server Communication
        private static void WorldRequest(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int LineNumber = Log.log("Processing world request packet..", Log.LogType.RECEIVED);
            int Character_Count = buffer.ReadInteger();
            for (int i = 0; i < Character_Count; i++)
            {
                int Character_ID = buffer.ReadInteger();
                World.instance.players.Add(Character_ID, new Player(Character_ID, buffer.ReadString(), buffer.ReadInteger(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat()));
                World.instance.players[Character_ID].Camera_Pos_X = buffer.ReadFloat();
                World.instance.players[Character_ID].Camera_Pos_Y = buffer.ReadFloat();
                World.instance.players[Character_ID].Camera_Pos_Z = buffer.ReadFloat();
                World.instance.players[Character_ID].Camera_Rotation_Y = buffer.ReadFloat();
                Log.log(LineNumber, "Processing world request packet.. Added character " + i.ToString() + "/" + Character_Count.ToString(), Log.LogType.RECEIVED);
            }
            Log.log(LineNumber, "Processing world request packet.. Added characters (" + Character_Count.ToString() + ")", Log.LogType.SUCCESS);
        }
        #endregion

        #region Client Communication
        private static void EnterWorld(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            Network.instance.Clients[index].Username = buffer.ReadString();
            string Character_Name = buffer.ReadString();
            Log.log("Account " + Network.instance.Clients[index].Username + " is entering the world with " + Character_Name + "..", Log.LogType.SUCCESS);
            Player player = World.instance.GetPlayer(Character_Name);
            Network.instance.Clients[index].Character_ID = player.Character_ID;
            SendData.CharacterDetails(index, player);
        }
        private static void Update(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            // ID
            int Character_ID = buffer.ReadInteger();
            // Position
            float X = buffer.ReadFloat();
            float Y = buffer.ReadFloat();
            float Z = buffer.ReadFloat();
            float R = buffer.ReadFloat();
            // Camera Position
            float cX = buffer.ReadFloat();
            float cY = buffer.ReadFloat();
            float cZ = buffer.ReadFloat();
            float cR = buffer.ReadFloat();
            // Animation State
            float Forward = buffer.ReadFloat();
            float Turn = buffer.ReadFloat();
            float Jump = buffer.ReadFloat();
            float JumpLeg = buffer.ReadFloat();
            byte bools = buffer.ReadByte();
            bool Crouch = false;
            bool OnGround = false;
            bool Attacking = false;
            bool Dead = false;
            bool Attacked = false;
            bool Cast = false;
            bool b6 = false; // Unused
            bool b7 = false; // Unused
            BitwiseRefinement.ByteToBools(bools, out Crouch, out OnGround, out Attacking, out Dead, out Attacked, out Cast, out b6, out b7);
            // Target

            World.instance.players[Character_ID].x = X;
            World.instance.players[Character_ID].y = Y;
            World.instance.players[Character_ID].z = Z;
            World.instance.players[Character_ID].r = R;
            World.instance.players[Character_ID].Camera_Pos_X = cX;
            World.instance.players[Character_ID].Camera_Pos_Y = cY;
            World.instance.players[Character_ID].Camera_Pos_Z = cZ;
            World.instance.players[Character_ID].Camera_Rotation_Y = cR;

            World.instance.players[Character_ID].AnimState.Forward = Forward;
            World.instance.players[Character_ID].AnimState.Turn = Turn;
            World.instance.players[Character_ID].AnimState.Jump = Jump;
            World.instance.players[Character_ID].AnimState.JumpLeg = JumpLeg;
            World.instance.players[Character_ID].AnimState.Crouch = Crouch;
            World.instance.players[Character_ID].AnimState.OnGround = OnGround;
            World.instance.players[Character_ID].AnimState.Attacking = Attacking;
            World.instance.players[Character_ID].AnimState.Dead = Dead;
            World.instance.players[Character_ID].AnimState.Attacked = Attacked;
            World.instance.players[Character_ID].AnimState.Cast = Cast;

            if (LineNumber == 0)
            {
                LineNumber = Log.log("Received update from player: " + Character_ID.ToString() + 
                    " x : " + X.ToString() + " y : " + Y.ToString() + " z : " + Z.ToString() + " r : " + R.ToString() + 
                    " cx : " + cX.ToString() + " cy : " + cY.ToString() + " cz : " + cZ.ToString() + " cr : " + cR.ToString());
            }
            else
            {
                Log.log(LineNumber, "Received update from player: " + Character_ID.ToString() +
                    " x : " + X.ToString() + " y : " + Y.ToString() + " z : " + Z.ToString() + " r : " + R.ToString() +
                    " cx : " + cX.ToString() + " cy : " + cY.ToString() + " cz : " + cZ.ToString() + " cr : " + cR.ToString());
            }

            SendData.UpdatePlayerData(World.instance.players[Character_ID]);
        }
        #endregion
    }
}
