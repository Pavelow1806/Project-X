using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Project_X_Synchronization_Server
{
    public enum ClientSendPacketNumbers
    {
        Invalid,
        LoginResponse,
        RegistrationResponse,
        CharacterList
    }
    public enum ServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer
    }
    public enum LoginServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer
    }
    public enum GameServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer,
        WorldRequest
    }
    public enum SyncServerTable
    {
        tbl_Characters,
        tbl_NPC,
        tbl_Quests,
        tbl_Collectables,
        tbl_Spawn_Positions,
        tbl_Quest_Log,
        tbl_Experience
    }
    class SendData
    {
        private static void sendData(ConnectionType destination, string PacketName, byte[] data)
        {
            try
            {
                switch (destination)
                {
                    case ConnectionType.GAMESERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    case ConnectionType.LOGINSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    default:
                        break;
                }
                Log.log("Successfully sent packet (" + PacketName + ") to " + destination.ToString(), Log.LogType.SENT);
            }
            catch (Exception e)
            {
                // Output error message
                Log.log("An error occured when attempting to send data:", Log.LogType.ERROR);
                Log.log("     Destination   > " + destination.ToString(), Log.LogType.ERROR);
                Log.log("     Error Message > " + e.Message, Log.LogType.ERROR);
            }

        }

        private static void BuildBasePacket(int packetNumber, ref ByteBuffer.ByteBuffer buffer)
        {
            buffer.WriteInteger((int)ConnectionType.SYNCSERVER);
            buffer.WriteInteger(packetNumber);
        }
        #region Generic
        public static void Authenticate(Connection connection)
        {
            if (!connection.Authenticated)
            {
                try
                {
                    ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                    BuildBasePacket((int)ServerSendPacketNumbers.AuthenticateGameServer, ref buffer);
                    buffer.WriteString(Network.instance.AuthenticationCode);
                    sendData(connection.Type, ServerSendPacketNumbers.AuthenticateGameServer.ToString(), buffer.ToArray());
                }
                catch (Exception e)
                {
                    Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                    return;
                }
            }
            else
            {
                Log.log("An attempt was made to send an authentication packet, the " + connection.Type.ToString() + " is already authenticated.", Log.LogType.ERROR);
            }
        }
        #endregion

        #region Game Server
        public static void WorldRequest()
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                Log.log("Sending world request response.", Log.LogType.SENT);

                float TimeBetween = 0.5f;
                DateTime NextPacket = DateTime.Now;

                // tbl_Characters
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                int LineNumber = Log.log("Sending tbl_Characters..", Log.LogType.SENT);
                buffer.WriteInteger((int)SyncServerTable.tbl_Characters);
                buffer.WriteInteger(Data.tbl_Characters.Count);
                foreach (KeyValuePair<int, _Characters> character in Data.tbl_Characters)
                {
                    buffer.WriteInteger(character.Key);
                    buffer.WriteString(character.Value.Character_Name);
                    buffer.WriteInteger(character.Value.Character_Level);
                    buffer.WriteInteger(character.Value.Gender);
                    buffer.WriteFloat(character.Value.Pos_X);
                    buffer.WriteFloat(character.Value.Pos_Y);
                    buffer.WriteFloat(character.Value.Pos_Z);
                    buffer.WriteFloat(character.Value.Rotation_Y);
                    buffer.WriteInteger(character.Value.Health);
                    buffer.WriteInteger(character.Value.Strength);
                    buffer.WriteInteger(character.Value.Agility);
                    buffer.WriteInteger(character.Value.Experience);
                    buffer.WriteFloat(character.Value.Camera_Pos_X);
                    buffer.WriteFloat(character.Value.Camera_Pos_Y);
                    buffer.WriteFloat(character.Value.Camera_Pos_Z);
                    buffer.WriteFloat(character.Value.Camera_Rotation_Y);
                    Log.log(LineNumber, "Sending tbl_Characters.. Character ID " + character.Key.ToString() + "/" + Data.tbl_Characters.Count.ToString(), Log.LogType.SENT);
                }
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString(), buffer.ToArray());

                NextPacket = DateTime.Now.AddSeconds(TimeBetween);
                while (DateTime.Now < NextPacket)
                {

                }

                // tbl_NPC
                buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                LineNumber = Log.log("Sending tbl_NPC..", Log.LogType.SENT);
                buffer.WriteInteger((int)SyncServerTable.tbl_NPC);
                buffer.WriteInteger(Data.tbl_NPC.Count);
                foreach (KeyValuePair<int, _NPC> npc in Data.tbl_NPC)
                {
                    buffer.WriteInteger(npc.Key);
                    buffer.WriteInteger(npc.Value.Status);
                    buffer.WriteString(npc.Value.Name);
                    buffer.WriteInteger(npc.Value.Respawn_Time);
                    buffer.WriteInteger(npc.Value.Level);
                    buffer.WriteInteger(npc.Value.Gender);
                    buffer.WriteInteger(npc.Value.HP);
                    Log.log(LineNumber, "Sending tbl_NPC.. NPC ID " + npc.Key.ToString() + "/" + Data.tbl_NPC.Count.ToString(), Log.LogType.SENT);
                }
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString(), buffer.ToArray());

                NextPacket = DateTime.Now.AddSeconds(TimeBetween);
                while (DateTime.Now < NextPacket)
                {

                }

                // tbl_Quests
                buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                LineNumber = Log.log("Sending tbl_Quests..", Log.LogType.SENT);
                buffer.WriteInteger((int)SyncServerTable.tbl_Quests);
                buffer.WriteInteger(Data.tbl_Quests.Count);
                foreach (KeyValuePair<int, _Quests> quest in Data.tbl_Quests)
                {
                    buffer.WriteInteger(quest.Key);
                    buffer.WriteString(quest.Value.Title);
                    buffer.WriteString(quest.Value.Start_Text);
                    buffer.WriteString(quest.Value.End_Text);
                    buffer.WriteInteger(quest.Value.Reward_ID);
                    buffer.WriteInteger(quest.Value.NPC_Start_ID);
                    buffer.WriteInteger(quest.Value.NPC_End_ID);
                    buffer.WriteInteger(quest.Value.Objective_Target);
                    buffer.WriteInteger(quest.Value.Start_Requirement_Quest_ID);
                    buffer.WriteInteger(quest.Value.Item_Objective_ID);
                    buffer.WriteInteger(quest.Value.NPC_Objective_ID);
                    Log.log(LineNumber, "Sending tbl_Quests.. Quest ID " + quest.Key.ToString() + "/" + Data.tbl_Quests.Count.ToString(), Log.LogType.SENT);
                }
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString(), buffer.ToArray());

                NextPacket = DateTime.Now.AddSeconds(TimeBetween);
                while (DateTime.Now < NextPacket)
                {

                }

                // tbl_Collectables
                buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                LineNumber = Log.log("Sending tbl_Collectables..", Log.LogType.SENT);
                buffer.WriteInteger((int)SyncServerTable.tbl_Collectables);
                buffer.WriteInteger(Data.tbl_Collectables.Count);
                foreach (KeyValuePair<int, _Collectables> col in Data.tbl_Collectables)
                {
                    buffer.WriteInteger(col.Key);
                    buffer.WriteString(col.Value.Collectable_Name);
                    buffer.WriteInteger(col.Value.Respawn_Time);
                    Log.log(LineNumber, "Sending tbl_Collectables.. Collectable ID " + col.Key.ToString() + "/" + Data.tbl_Collectables.Count.ToString(), Log.LogType.SENT);
                }
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString(), buffer.ToArray());

                NextPacket = DateTime.Now.AddSeconds(TimeBetween);
                while (DateTime.Now < NextPacket)
                {

                }

                // tbl_Spawn_Positions
                buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                LineNumber = Log.log("Sending tbl_Spawn_Positions..", Log.LogType.SENT);
                buffer.WriteInteger((int)SyncServerTable.tbl_Spawn_Positions);
                buffer.WriteInteger(Data.tbl_Spawn_Positions.Count);
                foreach (KeyValuePair<int, _Spawn_Positions> sp in Data.tbl_Spawn_Positions)
                {
                    buffer.WriteInteger(sp.Key);
                    buffer.WriteFloat(sp.Value.Pos_X);
                    buffer.WriteFloat(sp.Value.Pos_Y);
                    buffer.WriteFloat(sp.Value.Pos_Z);
                    buffer.WriteFloat(sp.Value.Rotation_Y);
                    buffer.WriteInteger(sp.Value.NPC_ID);
                    buffer.WriteInteger(sp.Value.Collectable_ID);
                    Log.log(LineNumber, "Sending tbl_Spawn_Positions.. Spawn ID " + sp.Key.ToString() + "/" + Data.tbl_Spawn_Positions.Count.ToString(), Log.LogType.SENT);
                }
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString(), buffer.ToArray());

                NextPacket = DateTime.Now.AddSeconds(TimeBetween);
                while (DateTime.Now < NextPacket)
                {

                }

                // tbl_Quest_Log
                buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                LineNumber = Log.log("Sending tbl_Quest_Log..", Log.LogType.SENT);
                buffer.WriteInteger((int)SyncServerTable.tbl_Quest_Log);
                buffer.WriteInteger(Data.tbl_Quest_Log.Count);
                foreach (KeyValuePair<int, _Quest_Log> ql in Data.tbl_Quest_Log)
                {
                    buffer.WriteInteger(ql.Key);
                    buffer.WriteInteger(ql.Value.Character_ID);
                    buffer.WriteInteger(ql.Value.Quest_ID);
                    buffer.WriteInteger(ql.Value.Quest_Status);
                    buffer.WriteInteger(ql.Value.Progress);
                    Log.log(LineNumber, "Sending tbl_Quest_Log.. Quest Log ID " + ql.Key.ToString() + "/" + Data.tbl_Quest_Log.Count.ToString(), Log.LogType.SENT);
                }
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString(), buffer.ToArray());

                NextPacket = DateTime.Now.AddSeconds(TimeBetween);
                while (DateTime.Now < NextPacket)
                {

                }

                // tbl_Experience
                buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                LineNumber = Log.log("Sending tbl_Experience..", Log.LogType.SENT);
                buffer.WriteInteger((int)SyncServerTable.tbl_Experience);
                buffer.WriteInteger(Data.tbl_Experience.Count);
                foreach (KeyValuePair<int, _Experience> ex in Data.tbl_Experience)
                {
                    buffer.WriteInteger(ex.Key);
                    buffer.WriteInteger(ex.Value.Level);
                    buffer.WriteInteger(ex.Value.Experience);
                    buffer.WriteInteger(ex.Value.Strength);
                    buffer.WriteInteger(ex.Value.Agility);
                    buffer.WriteInteger(ex.Value.HP);
                    Log.log(LineNumber, "Sending tbl_Experience.. Experience ID " + ex.Key.ToString() + "/" + Data.tbl_Experience.Count.ToString(), Log.LogType.SENT);
                }
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString(), buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building World update packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion
    }
}
