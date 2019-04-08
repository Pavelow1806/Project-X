﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Project_X_Game_Server
{
    public enum ClientSendPacketNumbers
    {
        Invalid,
        WorldPacket,
        CharacterDetails
    }
    public enum ServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer
    }
    public enum LoginServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer,
        ConfirmWhiteList
    }
    public enum SyncServerSendPacketNumbers
    {
        Invalid,
        AuthenticateSyncServer,
        WorldRequest,
        UpdatePlayerData
    }
    class SendData
    {
        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        int LineNumber = 0;

        public static bool PostUDPMessages = true;

        private static void sendData(ConnectionType destination, string PacketName, int index, byte[] data)
        {
            try
            {
                switch (destination)
                {
                    case ConnectionType.CLIENT:
                        Log.log("Successfully sent packet (" + PacketName + ") to " + destination.ToString(), Log.LogType.SENT);
                        Network.instance.Clients[index].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    case ConnectionType.LOGINSERVER:
                        Log.log("Successfully sent packet (" + PacketName + ") to " + destination.ToString(), Log.LogType.SENT);
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    case ConnectionType.SYNCSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    default:
                        break;
                }
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
            buffer.WriteInteger((int)ConnectionType.GAMESERVER);
            buffer.WriteInteger(packetNumber);
        }
        #region Generic
        public static void Authenticate(Connection server)
        {
            if (!server.Authenticated)
            {
                try
                {
                    ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                    BuildBasePacket((int)ServerSendPacketNumbers.AuthenticateGameServer, ref buffer);
                    buffer.WriteString(Network.instance.AuthenticationCode);
                    sendData(server.Type, ServerSendPacketNumbers.AuthenticateGameServer.ToString(), -1, buffer.ToArray());
                }
                catch (Exception e)
                {
                    Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                    return;
                }
            }
            else
            {
                Log.log("An attempt was made to send an authentication packet, the " + server.Type.ToString() + " is already authenticated.", Log.LogType.ERROR);
            }
        }
        #endregion

        #region Login Server
        public static void ConfirmWhiteList(string ip)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)LoginServerSendPacketNumbers.ConfirmWhiteList, ref buffer);
                buffer.WriteString(ip);
                Log.log("Sent white list confirmation of IP: " + ip, Log.LogType.SENT);
                sendData(ConnectionType.LOGINSERVER, LoginServerSendPacketNumbers.ConfirmWhiteList.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion

        #region Sync Server
        public static void WorldRequest(int LineNumber)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.WorldRequest, ref buffer);
                Log.log(LineNumber, "Requesting world status update from synchronization server..", Log.LogType.SENT);
                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.WorldRequest.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log(LineNumber, "Building World Request packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void UpdatePlayerData(Player player)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.UpdatePlayerData, ref buffer);
                // ID
                buffer.WriteInteger(player.Character_ID);
                // Position
                buffer.WriteFloat(player.x);
                buffer.WriteFloat(player.y);
                buffer.WriteFloat(player.z);
                buffer.WriteFloat(player.r);

                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.UpdatePlayerData.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Update Player Data packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion

        #region Client Communication
        public static void SendUDP_Packet(Connection connection, byte[] data)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress Receiver = IPAddress.Parse(connection.IP.Substring(connection.IP.IndexOf(':')));
                IPEndPoint EndPoint = new IPEndPoint(Receiver, Network.UDPPort);
                socket.SendTo(data, EndPoint);
                if (PostUDPMessages) Log.log("UDP Packet sent to IP: " + connection.IP.Substring(connection.IP.IndexOf(':')).ToString() + " Length: " + data.Length.ToString(), Log.LogType.SENT);
            }
            catch (Exception e)
            {
                Log.log("An error occurred when sending a UDP packet to IP: " + connection.IP.Substring(connection.IP.IndexOf(':')).ToString() + ". > " + e.Message, Log.LogType.ERROR);
            }
        }
        public static void WorldPacket(int index)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.WorldPacket, ref buffer);
                //TBC
                Log.log("Sending initial world packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.WorldPacket.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building initial world packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void CharacterDetails(int index, Player Character)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.CharacterDetails, ref buffer);
                buffer.WriteString(Character.Name);
                buffer.WriteInteger(Character.Level);
                buffer.WriteFloat(Character.x);
                buffer.WriteFloat(Character.y);
                buffer.WriteFloat(Character.z);
                buffer.WriteFloat(Character.r);
                buffer.WriteInteger(Character.Character_ID);
                //buffer.WriteInteger(Character.quests.Count);
                //foreach (Quest quest in Character.quests)
                //{
                //    buffer.WriteInteger(quest.ID);
                //    buffer.WriteString(quest.Title);
                //    buffer.WriteString(quest.StartText);
                //    buffer.WriteString(quest.EndText);
                //    buffer.WriteInteger(quest.Reward);
                //    buffer.WriteInteger(quest.NPCStartID);
                //    buffer.WriteInteger(quest.NPCEndID);
                //    buffer.WriteInteger(quest.ObjectiveTarget);
                //    buffer.WriteInteger(quest.StartRequirementQuestID);
                //    buffer.WriteByte((quest.Complete) ? (byte)1 : (byte)0);
                //    buffer.WriteByte((quest.TurnedIn) ? (byte)1 : (byte)0);
                //    buffer.WriteByte((quest.Active) ? (byte)1 : (byte)0);
                //    buffer.WriteInteger(quest.ObjectiveProgress);
                //}
                Log.log("Sending Character Data packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.CharacterDetails.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Send Character Data packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
                throw;
            }
        }
        public static void SendUDP_WorldUpdate(int index, BufferReturn request)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteInteger((int)request);
            byte[] data = World.instance.PullBuffer(request);
            if (data != null)
            {
                buffer.WriteBytes(data);
                SendUDP_Packet(Network.instance.Clients[index], buffer.ToArray());
            }
        }
        #endregion
    }
}
