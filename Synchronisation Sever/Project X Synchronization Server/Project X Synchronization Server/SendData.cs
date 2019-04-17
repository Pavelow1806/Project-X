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
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                // tbl_Characters
                int LineNumber = Log.log("Sending tbl_Characters..", Log.LogType.SENT);

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
                    buffer.WriteFloat(character.Value.Camera_Pos_X);
                    buffer.WriteFloat(character.Value.Camera_Pos_Y);
                    buffer.WriteFloat(character.Value.Camera_Pos_Z);
                    buffer.WriteFloat(character.Value.Camera_Rotation_Y);
                    buffer.WriteInteger(character.Value.Health);
                    buffer.WriteInteger(character.Value.Strength);
                    buffer.WriteInteger(character.Value.Agility);
                    Log.log(LineNumber, "Sending tbl_Characters.. Character ID " + character.Key.ToString() + "/" + Data.tbl_Characters.Count.ToString(), Log.LogType.SENT);
                }
                // tbl_NPC
                LineNumber = Log.log("Sending tbl_NPC..", Log.LogType.SENT);

                buffer.WriteInteger(Data.tbl_NPC.Count);
                foreach (KeyValuePair<int, _NPC> npc in Data.tbl_NPC)
                {
                    buffer.WriteInteger(npc.Key);
                    buffer.WriteInteger(npc.Value.Status);
                    buffer.WriteString(npc.Value.Name);
                    buffer.WriteInteger(npc.Value.Level);
                    buffer.WriteInteger(npc.Value.Gender);
                    buffer.WriteFloat(npc.Value.Pos_X);
                    buffer.WriteFloat(npc.Value.Pos_Y);
                    buffer.WriteFloat(npc.Value.Pos_Z);
                    buffer.WriteFloat(npc.Value.Rotation_Y);
                    buffer.WriteInteger(npc.Value.HP);
                    Log.log(LineNumber, "Sending tbl_NPC.. NPC ID " + npc.Key.ToString() + "/" + Data.tbl_NPC.Count.ToString(), Log.LogType.SENT);
                }
                // tbl_Quests
                LineNumber = Log.log("Sending tbl_Quests..", Log.LogType.SENT);

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

                // 
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
