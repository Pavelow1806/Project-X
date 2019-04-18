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
    public enum SyncServerTable
    {
        tbl_Characters,
        tbl_NPC,
        tbl_Quests,
        tbl_Collectables,
        tbl_Spawn_Positions
    }
    class ProcessData
    {
        #region Locking
        private static readonly object lockObj = new object();
        private static readonly object lockWorldObj = new object();
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
                Log.log(LineNumber, "Client IP: " + ip.Substring(0, ip.IndexOf(':')) + " was already white listed, sending reconfirmation to login server.", Log.LogType.WARNING);
                SendData.ConfirmWhiteList(ip.Substring(0, ip.IndexOf(':')));
            }
        }
        #endregion

        #region Synchronization Server Communication
        private static void WorldRequest(ConnectionType type, int index, byte[] data)
        {
            lock (lockWorldObj)
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                buffer.WriteBytes(data);
                ReadHeader(ref buffer);

                SyncServerTable table = (SyncServerTable)buffer.ReadInteger();
                switch (table)
                {
                    case SyncServerTable.tbl_Characters:
                        // tbl_Characters
                        int LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Characters..", Log.LogType.RECEIVED);
                        int Character_Count = buffer.ReadInteger();
                        for (int i = 0; i < Character_Count; i++)
                        {
                            int Character_ID = buffer.ReadInteger();
                            if (!World.instance.players.ContainsKey(Character_ID))
                            {
                                World.instance.players.Add(Character_ID, new Player(Character_ID, buffer.ReadString(), buffer.ReadInteger(), (Gender)buffer.ReadInteger(),
                                    buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(),
                                    0.0f, 0.0f, 0.0f, buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger()));
                                World.instance.players[Character_ID].type = EntityType.Player;
                                World.instance.players[Character_ID].Camera_Pos_X = buffer.ReadFloat();
                                World.instance.players[Character_ID].Camera_Pos_Y = buffer.ReadFloat();
                                World.instance.players[Character_ID].Camera_Pos_Z = buffer.ReadFloat();
                                World.instance.players[Character_ID].Camera_Rotation_Y = buffer.ReadFloat();
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added character " + i.ToString() + "/" + Character_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedPlayers = true;
                        Log.log(LineNumber, "Successfully added characters (" + Character_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_NPC:
                        // tbl_NPC
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_NPC..", Log.LogType.RECEIVED);
                        int NPC_Count = buffer.ReadInteger();
                        for (int i = 0; i < NPC_Count; i++)
                        {
                            int NPC_ID = buffer.ReadInteger();
                            if (!World.instance.NPCs.ContainsKey(NPC_ID))
                            {
                                World.instance.NPCs.Add(NPC_ID, new NPC(NPC_ID, (NPCStatus)buffer.ReadInteger(), buffer.ReadString(), buffer.ReadInteger(), buffer.ReadInteger(),
                                    (Gender)buffer.ReadInteger(), buffer.ReadInteger()));
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added NPC " + i.ToString() + "/" + NPC_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedNPCs = true;
                        Log.log(LineNumber, "Successfully added NPCs (" + NPC_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_Quests:
                        // tbl_Quests
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Quests..", Log.LogType.RECEIVED);
                        int Quest_Count = buffer.ReadInteger();
                        for (int i = 0; i < Quest_Count; i++)
                        {
                            int Quest_ID = buffer.ReadInteger();
                            if (!World.instance.quests.ContainsKey(Quest_ID))
                            {
                                World.instance.quests.Add(Quest_ID, new Quest(Quest_ID, buffer.ReadString(), buffer.ReadString(), buffer.ReadString(),
                                    buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger()));
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added quest " + i.ToString() + "/" + Quest_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedQuests = true;
                        Log.log(LineNumber, "Successfully added quests (" + Quest_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_Collectables:
                        // tbl_Collectables
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Collectables..", Log.LogType.RECEIVED);
                        int Collectable_Count = buffer.ReadInteger();
                        for (int i = 0; i < Collectable_Count; i++)
                        {
                            int Collectable_ID = buffer.ReadInteger();
                            if (!World.instance.collectables.ContainsKey(Collectable_ID))
                            {
                                World.instance.collectables.Add(Collectable_ID, new Collectable(Collectable_ID, buffer.ReadString(), buffer.ReadInteger(), 0.0f, 0.0f, 0.0f, 0.0f));
                            }                                
                            Log.log(LineNumber, "Processing world request packet.. Added collectable " + i.ToString() + "/" + Collectable_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedCollectables = true;
                        Log.log(LineNumber, "Successfully added collectables (" + Collectable_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_Spawn_Positions:
                        // tbl_Spawn_Positions
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Spawn_Positions..", Log.LogType.RECEIVED);
                        int Spawn_Count = buffer.ReadInteger();
                        for (int i = 0; i < Spawn_Count; i++)
                        {
                            int Spawn_ID = buffer.ReadInteger();
                            if (!World.instance.spawns.ContainsKey(Spawn_ID))
                            {
                                World.instance.spawns.Add(Spawn_ID, new Spawn(Spawn_ID, buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(),
                                    buffer.ReadFloat(), buffer.ReadInteger(), buffer.ReadInteger()));
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added spawn " + i.ToString() + "/" + Spawn_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedSpawns = true;
                        Log.log(LineNumber, "Successfully added spawn (" + Spawn_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    default:
                        break;
                }
                World.instance.Initialise();
            }
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
            player.InWorld = true;
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
            // Velocity
            float vx = buffer.ReadFloat();
            float vy = buffer.ReadFloat();
            float vz = buffer.ReadFloat();
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
            EntityType TargetType = (EntityType)buffer.ReadInteger();
            int TargetID = buffer.ReadInteger();
            
            Player player = World.instance.players[Character_ID];
            // Position
            player.x = X;
            player.y = Y;
            player.z = Z;
            player.r = R;
            // Velocity
            player.vx = vx;
            player.vy = vy;
            player.vz = vz;
            // Camera Position and Rotation
            player.Camera_Pos_X = cX;
            player.Camera_Pos_Y = cY;
            player.Camera_Pos_Z = cZ;
            player.Camera_Rotation_Y = cR;
            // Animations
            player.AnimState.Forward = Forward;
            player.AnimState.Turn = Turn;
            player.AnimState.Jump = Jump;
            player.AnimState.JumpLeg = JumpLeg;
            player.AnimState.Crouch = Crouch;
            player.AnimState.OnGround = OnGround;
            player.AnimState.Attacking = Attacking;
            player.AnimState.Dead = Dead;
            player.AnimState.Attacked = Attacked;
            player.AnimState.Cast = Cast;
            // Target
            player.TargetType = (int)TargetType;
            player.TargetID = TargetID;

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

            SendData.UpdatePlayerData(player);
        }
        #endregion
    }
}
