using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using static Faction_TerritoriesV2.NexusAPI;

namespace Faction_TerritoriesV2
{
    public class NexusAPI
    {
        public ushort CrossServerModID;

        /*  For recieving custom messages you have to register a message handler with a different unique ID then what you use server to client. (It should be the same as this class)
         *  
         *  NexusAPI(5432){
         *  CrossServerModID = 5432
         *  }
         *  
         *  
         *  Register this somewhere in your comms code. (This will only be raised when it recieves a message from another server)
         *  MyAPIGateway.Multiplayer.RegisterMessageHandler(5432, MessageHandler);
         */




        public NexusAPI(ushort SocketID)
        {
            CrossServerModID = SocketID;
        }

        public static bool IsRunningNexus()
        {
            return false;
        }

        public static bool IsPlayerOnline(long IdentityID)
        {
            return false;
        }

        private static List<object[]> GetSectorsObject()
        {
            List<object[]> APISectors = new List<object[]>();
            return APISectors;
        }

        private static List<object[]> GetAllOnlinePlayersObject()
        {
            List<object[]> OnlinePlayers = new List<object[]>();
            return OnlinePlayers;
        }

        private static List<object[]> GetAllOnlineServersObject()
        {
            List<object[]> Servers = new List<object[]>();
            return Servers;

        }

        private static object[] GetThisServerObject()
        {
            object[] OnlinePlayers = new object[6];
            return OnlinePlayers;
        }


        public static Server GetThisServer()
        {
            object[] obj = GetThisServerObject();
            return new Server((string)obj[0], (int)obj[1], (short)obj[2], (int)obj[3], (int)obj[4], (List<ulong>)obj[5]);
        }

        public static List<Sector> GetSectors()
        {
            List<object[]> Objs = GetSectorsObject();

            List<Sector> Sectors = new List<Sector>();
            foreach (var obj in Objs)
            {
                Sectors.Add(new Sector((string)obj[0], (string)obj[1], (int)obj[2], (bool)obj[3], (Vector3D)obj[4], (double)obj[5], (int)obj[6]));
            }
            return Sectors;
        }

        public static List<Player> GetAllOnlinePlayers()
        {
            List<object[]> Objs = GetAllOnlinePlayersObject();

            List<Player> Players = new List<Player>();
            foreach (var obj in Objs)
            {
                Players.Add(new Player((string)obj[0], (ulong)obj[1], (long)obj[2], (int)obj[3]));
            }
            return Players;
        }

        public static List<Server> GetAllOnlineServers()
        {
            List<object[]> Objs = GetAllOnlineServersObject();

            List<Server> Servers = new List<Server>();
            foreach (var obj in Objs)
            {
                Servers.Add(new Server((string)obj[0], (int)obj[1], (int)obj[2], (float)obj[3], (int)obj[4], (List<ulong>)obj[5]));
            }
            return Servers;
        }


        public static void BackupGrid(List<MyObjectBuilder_CubeGrid> GridObjectBuilders, long OnwerIdentity)
        {
            return;
        }

        public static void SendMessageToDiscord(string message, string ChannelID = null)
        {
            return;
        }

        public void SendMessageToServer(int ServerID, byte[] Message)
        {
            return;
        }

        public void SendMessageToAllServers(byte[] Message)
        {
            return;
        }





        public class Sector
        {
            public readonly string Name;

            public readonly string IPAddress;

            public readonly int Port;

            public readonly bool IsGeneralSpace;

            public readonly Vector3D Center;

            public readonly double Radius;

            public readonly int ServerID;

            public Sector(string Name, string IPAddress, int Port, bool IsGeneralSpace, Vector3D Center, double Radius, int ServerID)
            {
                this.Name = Name;
                this.IPAddress = IPAddress;
                this.Port = Port;
                this.IsGeneralSpace = IsGeneralSpace;
                this.Center = Center;
                this.Radius = Radius;
                this.ServerID = ServerID;
            }

        }

        public class Player
        {

            public readonly string PlayerName;

            public readonly ulong SteamID;

            public readonly long IdentityID;

            public readonly int OnServer;

            public Player(string PlayerName, ulong SteamID, long IdentityID, int OnServer)
            {
                this.PlayerName = PlayerName;
                this.SteamID = SteamID;
                this.IdentityID = IdentityID;
                this.OnServer = OnServer;
            }
        }

        public class Server
        {
            public readonly string Name;
            public readonly int ServerID;
            public readonly int MaxPlayers;
            public readonly float ServerSS;
            public readonly int TotalGrids;
            public readonly List<ulong> ReservedPlayers;


            public Server(string Name, int ServerID, int MaxPlayers, float SimSpeed, int TotalGrids, List<ulong> ReservedPlayers)
            {
                this.Name = Name;
                this.ServerID = ServerID;
                this.MaxPlayers = MaxPlayers;
                this.ServerSS = SimSpeed;
                this.TotalGrids = TotalGrids;
                this.ReservedPlayers = ReservedPlayers;
            }

        }

        public class CrossServerMessage
        {
            public readonly int ToServerID;
            public readonly int FromServerID;
            public readonly ushort UniqueMessageID;
            public readonly byte[] Message;

            public CrossServerMessage(ushort UniqueMessageID, int ToServerID, int FromServerID, byte[] Message)
            {
                this.UniqueMessageID = UniqueMessageID;
                this.ToServerID = ToServerID;
                this.FromServerID = FromServerID;
                this.Message = Message;
            }

            public CrossServerMessage() { }
        }
    }

    public static class NexusComms
    {
        public static void SendChatAllServers(ModMessage message)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.modMessage = message;

            CommsPackage package = new CommsPackage(NexusDataType.Chat, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            Session.Instance.Nexus.SendMessageToAllServers(sendData);
            // MyLog.Default.WriteLineAndConsole($"NexusAPI: Nexus Message Sent");
            //MyAPIGateway.Multiplayer.SendMessageToServer(Session.Instance.ModId, sendData);
        }

        public static void RequestTerritoryStatus()
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.nexusServerId = Session.Instance.NexusServer.ServerID;

            CommsPackage package = new CommsPackage(NexusDataType.SettingsRequested, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            Session.Instance.Nexus.SendMessageToAllServers(sendData);
        }

        public static void SendTerritoryStatus(int serverId)
        {
            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                ObjectContainer objectContainer = new ObjectContainer();
                objectContainer.settings = item;

                CommsPackage package = new CommsPackage(NexusDataType.SettingsSent, objectContainer);
                var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
                Session.Instance.Nexus.SendMessageToServer(serverId, sendData);
            }
        }

        public static void UpdateTerritoryStatusToAll()
        {
            ObjectContainer oc = new ObjectContainer();
            foreach (var item in Session.Instance.claimBlocks.Values)
                oc.listSettings.Add(item);

            CommsPackage package = new CommsPackage(NexusDataType.UpdateSettings, oc);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            Session.Instance.Nexus.SendMessageToAllServers(sendData);
        }

        public static void MessageHandler(byte[] data)
        {
            try
            {
                //MyLog.Default.WriteLineAndConsole($"NexusAPI: Nexus Message Received");
                var package = MyAPIGateway.Utilities.SerializeFromBinary<CommsPackage>(data);
                if (package == null) return;

                if (package.NexusType == NexusDataType.Chat)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    MyVisualScriptLogicProvider.SendChatMessageColored($"<@&{encasedData.modMessage.RoleID}> [{encasedData.modMessage.RoleName}]: {encasedData.modMessage.Message}", encasedData.modMessage.Color, encasedData.modMessage.Author, 0, "Red");
                    return;
                }

                if (package.NexusType == NexusDataType.SettingsRequested)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    SendTerritoryStatus(encasedData.nexusServerId);
                    return;
                }

                if (package.NexusType == NexusDataType.SettingsSent)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    Session.Instance.crossServerClaimSettings.Add(encasedData.settings);
                    return;
                }

                if (package.NexusType == NexusDataType.UpdateSettings)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    Session.Instance.crossServerClaimSettings.Clear();
                    Session.Instance.crossServerClaimSettings = encasedData.listSettings;

                    Utils.UpdateTerritoryStatus();
                }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
