using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Login_Server
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
    class SendData : Data
    {
        private static void sendData(ConnectionType destination)
        {
            try
            {
                buffer.WriteBytes(data);
                switch (destination)
                {
                    case ConnectionType.GAMESERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                        break;
                    case ConnectionType.CLIENT:
                        Network.instance.Clients[Index].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                        break;
                    case ConnectionType.SYNCSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
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
    }
}
