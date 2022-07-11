using ProtoBuf;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using static Faction_TerritoriesV2.NexusAPI;

namespace Faction_TerritoriesV2
{
    public enum NexusDataType
    {
        FactionJoin,
        FactionLeave,
        FactionRemove,
        FactionEdited,
        Chat,
        SettingsRequested,
        SettingsSent,
        UpdateSettings
    }

    public enum SyncType
    {
        DetailInfo,
        Timer,
        SiegeTimer,
        Emissive,
        AddProductionPerk,
        EnableProductionPerk,
        DisableProductionPerk,
        SyncProductionAttached,
        SyncProductionRunning

    }

    public enum DataType
    {
        MyString,
        ClaimSettings,
        InitClaim,
        InitSiege,
        Sync,
        SingleSync,
        ColorGps,
        RemoveClaimSettings,
        SendClientSettings,
        RequestSettings,
        UpdateDetailInfo,
        UpdateBlockText,
        CreateTrigger,
        RemoveTrigger,
        SendGps,
        AddTerritory,
        SendAudio,
        UpdateEmissives,
        ResetTerritory,
        SyncBillboard,
        SyncParticle,
        UpdateProduction,
        SyncProduction,
        AddProduction,
        RemoveProduction,
        ManualTerritory,
        InitFinalSiege,
        ConsumeDelayTokens,
        UpdateSafeZoneAllies,
        DisableSafeZone,
        EnableSafeZone,
        ResetModData,
        PBMonitor,
        BuyAddInstallation,
        SyncInstallations,
        UpdateRelationAllies,
        RefreshConfig,
        RequestConfig,
        SendConfig,
        UpdateInstallation,
        SetRep
    }

    [ProtoContract]
    public class ObjectContainer
    {
        [ProtoMember(1)]
        public long entityId = 0;
        [ProtoMember(2)]
        public long playerId = 0;
        [ProtoMember(3)]
        public long claimBlockId = 0;
        [ProtoMember(4)]
        public ClaimBlockSettings settings = new ClaimBlockSettings();
        [ProtoMember(5)]
        public string stringData;
        [ProtoMember(6)]
        public Vector3D location = new Vector3D(0, 0, 0);
        [ProtoMember(7)]
        public string factionTag;
        [ProtoMember(8)]
        public ulong steamId;
        [ProtoMember(9)]
        public SyncType syncType;
        [ProtoMember(10)]
        public float floatingNum;
        [ProtoMember(11)]
        public long fromFaction;
        [ProtoMember(12)]
        public long toFaction;
        [ProtoMember(13)]
        public ModMessage modMessage = new ModMessage();
        [ProtoMember(14)]
        public int nexusServerId;
        [ProtoMember(15)]
        public InstallationType installationType;
        [ProtoMember(16)]
        public Installations installation;
        [ProtoMember(17)]
        public VRage.Game.MyRelationsBetweenFactions relation;
        [ProtoMember(18)]
        public bool boolean;
        [ProtoMember(19)]
        public TerritoryConfig territoryConfig;
        [ProtoMember(20)]
        public int integer;
        [ProtoMember(21)]
        public List<ClaimBlockSettings> listSettings;
    }

    [ProtoContract]
    public class CommsPackage
    {
        [ProtoMember(1)]
        public DataType Type;

        [ProtoMember(2)]
        public byte[] Data;

        [ProtoMember(3)]
        public NexusDataType NexusType;

        public CommsPackage()
        {
            Type = DataType.MyString;
            Data = new byte[0];
        }

        public CommsPackage(DataType type, ObjectContainer objectContainer)
        {
            Type = type;
            Data = MyAPIGateway.Utilities.SerializeToBinary(objectContainer);
        }

        public CommsPackage(DataType type, AudioPackage audioPackage)
        {
            Type = type;
            Data = MyAPIGateway.Utilities.SerializeToBinary(audioPackage);
        }

        public CommsPackage(NexusDataType type, ObjectContainer objectContainer)
        {
            NexusType = type;
            Data = MyAPIGateway.Utilities.SerializeToBinary(objectContainer);
        }
    }

    public static class Comms
    {
        /*public static void AddToPlayerList(long playerId)
        {
            ObjectContainer oc = new ObjectContainer()
            {
                playerId = playerId
            };

            CommsPackage package = new CommsPackage(DataType.AddPlayerToList, oc);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }*/

        public static void SetReputationWithFaction(long playerId, long factionId, int amt)
        {
            ObjectContainer oc = new ObjectContainer()
            {
                playerId = playerId,
                toFaction = factionId,
                integer = amt
            };

            CommsPackage package = new CommsPackage(DataType.SetRep, oc);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
        }

        public static void UpdateInstallations(ClaimBlockSettings settings, Installations installation, bool add)
        {
            ObjectContainer oc = new ObjectContainer()
            {
                claimBlockId = settings.EntityId,
                installation = installation,
                boolean = add
            };

            CommsPackage package = new CommsPackage(DataType.UpdateInstallation, oc);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);

            if (MyAPIGateway.Multiplayer.IsServer)
                settings._server._save = true;
        }

        public static void SendClientConfig(TerritoryConfig config, ulong steamId = 0)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.territoryConfig = config;

            CommsPackage package = new CommsPackage(DataType.SendConfig, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);

            if (steamId == 0)
            {
                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                foreach (var player in players)
                {
                    MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
                }
            }
            else
            {
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, steamId);
            }
        }

        public static void RequestConfigs(ulong steamId)
        {
            ObjectContainer oc = new ObjectContainer()
            {
                steamId = steamId
            };

            CommsPackage package = new CommsPackage(DataType.RequestConfig, oc);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void RefreshConfig(long id)
        {
            ObjectContainer oc = new ObjectContainer()
            {
                claimBlockId = id
            };

            CommsPackage package = new CommsPackage(DataType.RefreshConfig, oc);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateAlliesRelation(ClaimBlockSettings settings, VRage.Game.MyRelationsBetweenFactions relation)
        {
            ObjectContainer oc = new ObjectContainer()
            {
                settings = settings,
                relation = relation
            };

            CommsPackage package = new CommsPackage(DataType.UpdateRelationAllies, oc);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SyncInstallations(Installations settings)
        {
            ObjectContainer oc = new ObjectContainer()
            {
                installation = settings
            };
            
            CommsPackage package = new CommsPackage(DataType.SyncInstallations, oc);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
        }

        public static void BuyAndAddInstallation(IMyTerminalBlock block, InstallationType installationType, bool isAdmin = false)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = block.EntityId;
            objectContainer.installationType = installationType;
            objectContainer.boolean = isAdmin;

            CommsPackage package = new CommsPackage(DataType.BuyAddInstallation, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void DisablePBMonitor(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.PBMonitor, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void ResetClientModData(ulong steamId)
        {
            ObjectContainer objectContainer = new ObjectContainer();

            CommsPackage package = new CommsPackage(DataType.ResetModData, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, steamId);
        }

        public static void InitClaimToServer(long jdID, long claimBlockId, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = jdID;
            objectContainer.playerId = playerId;
            objectContainer.claimBlockId = claimBlockId;

            CommsPackage package = new CommsPackage(DataType.InitClaim, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void InitSiegeToServer(long jdID, long claimBlockId, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = jdID;
            objectContainer.playerId = playerId;
            objectContainer.claimBlockId = claimBlockId;

            CommsPackage package = new CommsPackage(DataType.InitSiege, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        /*public static void FinalInitSiegeToServer(long jdID, long claimBlockId, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = jdID;
            objectContainer.playerId = playerId;
            objectContainer.claimBlockId = claimBlockId;

            CommsPackage package = new CommsPackage(DataType.InitFinalSiege, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }*/

        public static void ManualTerritorySet(ClaimBlockSettings settings, string tag)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.factionTag = tag;

            CommsPackage package = new CommsPackage(DataType.ManualTerritory, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendTriggerToServer(IMyTerminalBlock block, ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.claimBlockId = block.EntityId;
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.CreateTrigger, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendRemoveTriggerToServer(IMyTerminalBlock block)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.claimBlockId = block.EntityId;

            CommsPackage package = new CommsPackage(DataType.RemoveTrigger, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SyncSettingsToOthers(ClaimBlockSettings settings, long claimBlockId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.claimBlockId = claimBlockId;

            CommsPackage package = new CommsPackage(DataType.Sync, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);

            if (MyAPIGateway.Session.IsServer)
            {
                ClaimBlockSettings mySettings;
                if (!Session.Instance.claimBlocks.TryGetValue(claimBlockId, out mySettings)) return;

                mySettings._server._save = true;
            }
                //Session.Instance.SaveClaimData(claimBlockId);

        }

        /*public static void SyncParticles(ClaimBlockSettings settings, IMyPlayer client)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.SyncParticles, objectContainer);
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);

            foreach (var player in players)
            {
                if (player == client) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                //if (Vector3D.Distance(player.GetPosition(), settings.BlockPos) > 3000) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }
        }*/

        /*public static void CreateGpsOnClient(string description, Vector3D pos, long entityId, long playerId, ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.stringData = description;
            objectContainer.location = pos;
            objectContainer.entityId = entityId;
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(DataType.CreateGps, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            IMyPlayer toClient = Triggers.GetPlayerFromId(playerId);
            if (toClient == null) return;

            MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, toClient.SteamUserId);
        }*/

        public static void SendChangeColorToServer(string description, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.stringData = description;
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(DataType.ColorGps, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendRemoveBlockToOthers(long entityId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = entityId;

            CommsPackage package = new CommsPackage(DataType.RemoveClaimSettings, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
            /*List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/
        }

        public static void UpdateDetailInfo(string info, long entityId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = entityId;
            objectContainer.stringData = info;

            CommsPackage package = new CommsPackage(DataType.UpdateDetailInfo, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            //List<IMyPlayer> players = new List<IMyPlayer>();
            //MyAPIGateway.Players.GetPlayers(players);

            long senderId = 0;
            if (!Session.Instance.isDedicated)
                senderId = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;

            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);


            /*foreach (var player in players)
            {
                if (senderId == player.IdentityId) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/
        }

        public static void SyncSettingType(ClaimBlockSettings settings, IMyPlayer client, SyncType syncType)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.syncType = syncType;

            CommsPackage package = new CommsPackage(DataType.SingleSync, objectContainer);
            //List<IMyPlayer> players = new List<IMyPlayer>();
            // MyAPIGateway.Players.GetPlayers(players);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);

            /*foreach (var player in players)
            {
                if (player == client) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/

            if (MyAPIGateway.Session.IsServer)
            {
                settings._server._save = true;
                //Session.Instance.SaveClaimData(settings);
            }

            //else
            //MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateBlockText(ClaimBlockSettings settings, long playerId = 0)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(DataType.UpdateBlockText, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendAudioToClient(AudioPackage audio)
        {
            //ObjectContainer objectContainer = new ObjectContainer();
            //objectContainer.stringData = clip;

            CommsPackage package = new CommsPackage(DataType.SendAudio, audio);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);

            if (audio.player == null && audio.playerId != 0)
                audio.player = Triggers.GetPlayerFromId(audio.playerId);

            if (audio.player == null) return;
            MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, audio.player.SteamUserId);
        }

        public static void SendGpsToClient(long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(DataType.SendGps, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void AddTerritoryToClient(long playerId, ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.playerId = playerId;
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.AddTerritory, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateEmissiveToClients(long claimId, IMyPlayer omitPlayer)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.claimBlockId = claimId;

            CommsPackage package = new CommsPackage(DataType.UpdateEmissives, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
            /*List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (omitPlayer == player) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/
        }

        public static void SendClientsSettings(ClaimBlockSettings settings, ulong steamId = 0)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.SendClientSettings, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);

            if (steamId == 0)
            {
                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                foreach (var player in players)
                {
                    //if (MyAPIGateway.Session.LocalHumanPlayer == player) continue;
                    //if (player.SteamUserId <= 0 || player.IsBot) continue;
                    MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
                }
            }
            else
            {
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, steamId);
            }
        }

        public static void RequestSettings(ulong steamId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.steamId = steamId;

            CommsPackage package = new CommsPackage(DataType.RequestSettings, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendResetToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.ResetTerritory, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateProductionMultipliers(long blockId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.claimBlockId = blockId;

            CommsPackage package = new CommsPackage(DataType.UpdateProduction, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendApplyProductionPerkToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.AddProduction, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendRemoveProductionPerkToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.RemoveProduction, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SyncBillBoard(long entityId, IMyPlayer omitPlayer, string factionTag = "")
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = entityId;
            objectContainer.factionTag = factionTag;

            CommsPackage package = new CommsPackage(DataType.SyncBillboard, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
            /*List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (omitPlayer == player) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/
        }

        public static void SyncParticleEffect(string effect, Vector3D pos, ClaimBlockSettings settings = null, Installations installation = null, float size = 0)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.stringData = effect;
            objectContainer.location = pos;

            if (settings != null)
                objectContainer.settings = settings;

            if (installation != null)
                objectContainer.installation = installation;

            if (size != 0)
                objectContainer.floatingNum = size;

            CommsPackage package = new CommsPackage(DataType.SyncParticle, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            //MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                //if (player.SteamUserId <= 0 || player.IsBot) continue;
                if (Vector3D.Distance(player.GetPosition(), pos) > 10000) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }
        }

        public static void SyncProductionPerk(long blockId, string type, float value)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = blockId;
            objectContainer.stringData = type;
            objectContainer.floatingNum = value;

            CommsPackage package = new CommsPackage(DataType.SyncProduction, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
        }

        public static void ConsumeDelayTokens(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.ConsumeDelayTokens, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateSafeZoneAllies(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.UpdateSafeZoneAllies, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void DisableSafeZoneToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.DisableSafeZone, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void EnableSafeZoneToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.EnableSafeZone, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        /*public static void SendNexusFactionChange(NexusDataType nexusDataType, long fromFaction, long toFaction, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.fromFaction = fromFaction;
            objectContainer.toFaction = toFaction;
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(nexusDataType, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            Session.Instance.Nexus.SendMessage(sendData);
        }*/

        public static void MessageHandler(byte[] data)
        {
            try
            {
                var package = MyAPIGateway.Utilities.SerializeFromBinary<CommsPackage>(data);
                if (package == null) return;

                // To Server
                if (package.Type == DataType.InitClaim)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyEntity entity = null;
                    ClaimBlockSettings settings;

                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings)) return;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity)) return;
                    if (!Utils.TakeTokens(entity, settings)) return;

                    settings.IsClaiming = true;
                    settings.Timer = settings.TerritoryConfig._claimingConfig._claimingTime;
                    settings.PlayerClaimingId = encasedData.playerId;
                    //settings.PlayerClaiming = Triggers.GetPlayerFromId(encasedData.playerId);
                    settings.JDClaimingId = encasedData.entityId;
                    settings.JDClaiming = entity;
                    Utils.DrainAllJDs(entity);
                    settings.GetTerritoryStatus = TerritoryStatus.Claiming;

                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);
                    Vector3D pos = settings.BlockPos;
                    if (faction != null)
                        new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Claiming Territory: {settings.UnclaimName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.Timer)} to claim", "[Faction Territories]", Color.Red);
                    return;
                }

                // To Server
                if (package.Type == DataType.InitSiege)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyEntity entity = null;
                    ClaimBlockSettings settings;

                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings)) return;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity)) return;
                    if (!Utils.TakeTokens(entity, settings)) return;

                    Utils.MonitorSafeZonePBs(settings);

                    settings.IsSieging = true;
                    settings.SiegeTimer = Utils.GetTotalSiegeTime(settings);
                    settings.PlayerSiegingId = encasedData.playerId;
                    settings.JDSiegingId = encasedData.entityId;
                    settings.JDSieging = entity;
                    Utils.DrainAllJDs(entity);
                    Utils.RemoveSafeZone(settings);
                    Utils.AddSafeZone(settings, false);
                    settings.GetTerritoryStatus = TerritoryStatus.Sieging;

                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(encasedData.playerId);
                    Vector3D pos = settings.BlockPos;
                    if (faction != null)
                        new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                    else
                    {
                        IMyPlayer myPlayer = Triggers.GetPlayerFromId(encasedData.playerId);
                        if (myPlayer == null) return;

                        new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({myPlayer.DisplayName}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);

                    }

                    /*if (settings.IsSieged)
                    {
                        settings.IsSiegingFinal = true;
                        settings.SiegeTimer = settings.SiegeFinalTimer;
                        settings.PlayerSiegingId = encasedData.playerId;
                        settings.JDSiegingId = encasedData.entityId;
                        settings.JDSieging = entity;
                        Utils.DrainAllJDs(entity);
                        Utils.RemoveSafeZone(settings);
                        Utils.AddSafeZone(settings, false);

                        IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(encasedData.playerId);
                        Vector3D pos = settings.BlockPos;
                        if (faction != null)
                            //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                            new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Final Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                        else
                        {
                            IMyPlayer myPlayer = Triggers.GetPlayerFromId(encasedData.playerId);
                            if (myPlayer == null) return;
                            //MyVisualScriptLogicProvider.SendChatMessageColored($"({myPlayer.DisplayName}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                            new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({myPlayer.DisplayName}) Final Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);

                        }
                    }*/


                    return;
                }

                // To Server
                /*if (package.Type == DataType.InitFinalSiege)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyEntity entity = null;
                    ClaimBlockSettings settings;

                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings)) return;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity)) return;
                    if (!Utils.TakeTokens(entity, settings)) return;

                    Utils.MonitorSafeZonePBs(settings);

                    settings.ReadyToSiege = false;
                    settings.IsSiegingFinal = true;
                    settings.SiegeTimer = settings.SiegeFinalTimer;
                    settings.PlayerSiegingId = encasedData.playerId;
                    settings.JDSiegingId = encasedData.entityId;
                    settings.JDSieging = entity;
                    Utils.DrainAllJDs(entity);
                    Utils.RemoveSafeZone(settings);
                    Utils.AddSafeZone(settings, false);

                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(encasedData.playerId);
                    Vector3D pos = settings.BlockPos;
                    if (faction != null)
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Final Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                    else
                    {
                        IMyPlayer myPlayer = Triggers.GetPlayerFromId(encasedData.playerId);
                        if (myPlayer == null) return;
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({myPlayer.DisplayName}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({myPlayer.DisplayName}) Final Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);

                    }

                    return;
                }*/

                // To Server
                if (package.Type == DataType.ManualTerritory)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings mySettings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out mySettings)) return;

                    Utils.ManuallySetTerritory(mySettings, encasedData.factionTag);
                }

                // To Everyone
                if (package.Type == DataType.Sync)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    //ClaimBlockSettings temp = null;
                    //if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out temp)) return;

                    ClaimBlockSettings.SyncSettings(encasedData.settings, encasedData.claimBlockId);

                    //ServerData hold = temp.Server;
                    //Session.Instance.claimBlocks[encasedData.settings.EntityId] = encasedData.settings;
                    //Session.Instance.claimBlocks[encasedData.settings.EntityId].Server = hold;

                    if (MyAPIGateway.Multiplayer.IsServer)
                    {
                        ClaimBlockSettings mySettings;
                        if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out mySettings)) return;
                        mySettings._server._save = true;
                        //Session.Instance.SaveClaimData(encasedData.claimBlockId);
                        return;
                    }

                    return; // <---- THIS FIXED IT!!!
                }

                // To Everyone
                if (package.Type == DataType.SingleSync)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings mySettings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out mySettings)) return;

                    if (encasedData.syncType == SyncType.DetailInfo)
                    {
                        mySettings._detailInfo = encasedData.settings.DetailInfo;

                        IMyEntity entity = null;
                        if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.settings.EntityId, out entity)) return;
                        var block = entity as IMyTerminalBlock;
                        if (block == null) return;

                        block.RefreshCustomInfo();
                        ActionControls.RefreshControls(block, false);
                        //return;
                    }

                    if (encasedData.syncType == SyncType.Emissive)
                    {
                        mySettings._emissiveState = encasedData.settings.BlockEmissive;

                        IMyEntity block;
                        if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.settings.EntityId, out block)) return;
                        Utils.SetEmissive(mySettings.BlockEmissive, block as IMyBeacon);
                        //return;
                    }

                    if (encasedData.syncType == SyncType.SiegeTimer)
                    {
                        mySettings._siegeTimer = encasedData.settings.SiegeTimer;
                        //return;
                    }

                    if (encasedData.syncType == SyncType.Timer)
                    {
                        mySettings._timer = encasedData.settings.Timer;
                        //return;
                    }

                    /*if (encasedData.syncType == SyncType.AddProductionPerk)
                    {
                        mySettings._perks = encasedData.settings.GetPerks;
                        //return;
                    }*/

                    /*if (encasedData.syncType == SyncType.EnableProductionPerk)
                    {
                        mySettings._perks[PerkType.Production].enabled = true;
                        //return;
                    }*/

                    /*if (encasedData.syncType == SyncType.DisableProductionPerk)
                    {
                        if (mySettings._perks.ContainsKey(PerkType.Production))
                        {
                            mySettings._perks[PerkType.Production].enabled = false;
                        }

                        //return;
                    }*/

                    /*if (encasedData.syncType == SyncType.SyncProductionAttached)
                    {
                        if (mySettings._perks.ContainsKey(PerkType.Production) && encasedData.settings._perks.ContainsKey(PerkType.Production))
                        {
                            mySettings._perks[PerkType.Production].perk.productionPerk.attachedEntities = encasedData.settings._perks[PerkType.Production].perk.productionPerk.attachedEntities;
                        }

                        //return;
                    }*/

                    /*if (encasedData.syncType == SyncType.SyncProductionRunning)
                    {
                        if (mySettings._perks.ContainsKey(PerkType.Production) && encasedData.settings._perks.ContainsKey(PerkType.Production))
                        {
                            mySettings._perks[PerkType.Production].perk.productionPerk.ProductionRunning = encasedData.settings._perks[PerkType.Production].perk.productionPerk.ProductionRunning;
                        }

                        //return;
                    }*/

                    if (MyAPIGateway.Multiplayer.IsServer)
                    {
                        mySettings._server._save = true;
                        //Session.Instance.SaveClaimData(encasedData.settings);
                    }

                    return;
                }

                // To Server
                if (package.Type == DataType.ColorGps)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    MyVisualScriptLogicProvider.SetGPSColor(encasedData.stringData, Color.Red, encasedData.playerId);
                    return;
                }

                // To Everyone
                if (package.Type == DataType.RemoveClaimSettings)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    if (Session.Instance.claimBlocks.ContainsKey(encasedData.entityId))
                        Session.Instance.claimBlocks.Remove(encasedData.entityId);

                    return;
                }

                // To Everyone
                if (package.Type == DataType.UpdateDetailInfo)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyEntity entity = null;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity)) return;

                    var block = entity as IMyTerminalBlock;
                    if (block == null) return;

                    block.RefreshCustomInfo();
                    ActionControls.RefreshControls(block);
                    return;
                }

                // To Everyone
                if (package.Type == DataType.SyncBillboard)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyEntity entity = null;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity)) return;

                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(encasedData.factionTag);

                    Utils.SetScreen(entity as IMyBeacon, faction);
                    return;
                }

                // To Everyone
                if (package.Type == DataType.SyncParticle)
                {
                    if (Session.Instance.isServer && Session.Instance.isDedicated) return;

                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    Utils.PlayParticle(encasedData.stringData, encasedData.location, encasedData.settings, encasedData.installation, encasedData.floatingNum);
                    return;
                }

                // To Server
                if (package.Type == DataType.UpdateBlockText)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings mySettings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out mySettings)) return;

                    if (!mySettings.IsClaimed)
                        GPS.UpdateBlockText(mySettings, $"Unclaimed Territory: {encasedData.settings.UnclaimName}", encasedData.playerId);
                    else
                        GPS.UpdateBlockText(mySettings, $"Claimed Territory: {encasedData.settings.ClaimZoneName} ({encasedData.settings.ClaimedFaction})", encasedData.playerId);

                    return;
                }

                // To Client
                if (package.Type == DataType.SendAudio)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<AudioPackage>(package.Data);
                    if (encasedData == null) return;

                    Audio.audioQueue.Enqueue(encasedData);
                    return;
                }

                // To Server
                if (package.Type == DataType.CreateTrigger)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyEntity entity = null;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.claimBlockId, out entity)) return;
                    Events.CheckOwnership(entity as IMyTerminalBlock);
                    Triggers.CreateNewTriggers(entity as IMyBeacon);
                    return;
                }

                // To Server
                if (package.Type == DataType.RemoveTrigger)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    Triggers.RemoveTriggerData(encasedData.claimBlockId);
                    return;
                }

                // To Server
                if (package.Type == DataType.SendGps)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    Utils.CheckGridsToTag(encasedData.playerId);
                    Utils.CheckPlayersToTag(encasedData.playerId);
                    return;
                }

                // To Server
                if (package.Type == DataType.AddTerritory)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings mySettings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out mySettings)) return;

                    GPS.AddBlockLocation(mySettings.BlockPos, encasedData.playerId, mySettings);
                    return;
                }

                // To Everyone
                if (package.Type == DataType.UpdateEmissives)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings mySettings;
                    IMyEntity block;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out mySettings)) return;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.claimBlockId, out block)) return;

                    Utils.SetEmissive(mySettings.BlockEmissive, block as IMyBeacon);
                    return;
                }

                // To Clients
                if (package.Type == DataType.SyncProduction)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyEntity block;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out block)) return;

                    Utils.SetUpgradeValue(block as MyCubeBlock, encasedData.stringData, encasedData.floatingNum);
                    return;
                }

                // To Client/Everyone
                if (package.Type == DataType.SendClientSettings)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    if (Session.Instance.claimBlocks.ContainsKey(encasedData.settings.EntityId)) return;


                    IMyEntity entity;
                    if (MyAPIGateway.Entities.TryGetEntityById(encasedData.settings.EntityId, out entity))
                        encasedData.settings.Block = entity as IMyTerminalBlock;

                    Session.Instance.claimBlocks.Add(encasedData.settings.EntityId, encasedData.settings);
                    return;
                }

                // To Server
                if (package.Type == DataType.RequestSettings)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    foreach (var item in Session.Instance.claimBlocks.Values)
                    {
                        SendClientsSettings(item, encasedData.steamId);
                    }

                    return;
                }

                // To Client/Everyone
                if (package.Type == DataType.SendConfig)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    for (int i = 0; i < Session.Instance.territoryConfigs.Count; i++)
                    {
                        if (Session.Instance.territoryConfigs[i]._territoryName == encasedData.territoryConfig._territoryName)
                        {
                            Session.Instance.territoryConfigs[i] = encasedData.territoryConfig;
                            return;
                        }
                    }

                    Session.Instance.territoryConfigs.Add(encasedData.territoryConfig);
                    return;
                }

                // To Server
                if (package.Type == DataType.RequestConfig)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;
                    
                    foreach (var item in Session.Instance.territoryConfigs)
                    {
                        SendClientConfig(item, encasedData.steamId);
                    }

                    return;
                }

                // To Server
                if (package.Type == DataType.ResetTerritory)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings serverSettings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out serverSettings)) return;

                    if (encasedData.settings.IsClaimed)
                        Utils.RemoveSafeZone(serverSettings);

                    Utils.ResetClaim(serverSettings);
                    return;
                }

                // To Server
                /*if (package.Type == DataType.UpdateProduction)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings)) return;

                    Utils.UpdateProductionMultipliers(settings);
                    return;
                }*/

                // To Server
                /*if (package.Type == DataType.AddProduction)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                    Utils.AddAllProductionMultipliers(settings, null, true);
                    return;
                }*/

                // To Server
                /*if (package.Type == DataType.RemoveProduction)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                    Utils.RemoveProductionMultipliers(settings, null, true);
                    return;
                }*/

                // To Server
                /*if (package.Type == DataType.ConsumeDelayTokens)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                    Utils.DelaySiegeTokenConsumption(settings, true);
                }*/

                // To Server
                if (package.Type == DataType.UpdateSafeZoneAllies)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                    Utils.RemoveSafeZone(settings);
                    if (encasedData.settings.IsSieging)
                        Utils.AddSafeZone(settings, false);
                    else
                        Utils.AddSafeZone(settings);
                }

                // To Server
                if (package.Type == DataType.DisableSafeZone)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                    Utils.RemoveSafeZone(settings);
                }

                // To Server
                if (package.Type == DataType.EnableSafeZone)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                    Utils.AddSafeZone(settings);
                }

                // To Client
                if (package.Type == DataType.ResetModData)
                {
                    if (Session.Instance.isServer) return;
                    if (!Session.Instance.init)
                    {
                        MyLog.Default.WriteLineAndConsole("Territories: Client Connected, Init False, Returning");
                        return;
                    }

                    MyLog.Default.WriteLineAndConsole("Territories: Client Connected, Init True, Clearing Data/Reloading");
                    Session.Instance.claimBlocks.Clear();
                    RequestSettings(MyAPIGateway.Multiplayer.MyId);
                    MyAPIGateway.Parallel.StartBackground(() =>
                    {
                        MyAPIGateway.Parallel.Sleep(1500);
                        Session.Instance.init = false;
                    });
                }

                // To Server
                if (package.Type == DataType.PBMonitor)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                    Utils.MonitorSafeZonePBs(settings, true);
                }

                // To Server
                if (package.Type == DataType.BuyAddInstallation)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.entityId, out settings)) return;

                    Utils.CreateInstallation(settings, encasedData.installationType, encasedData.boolean);
                }

                // To Everyone
                if (package.Type == DataType.SyncInstallations)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.installation.ParentId, out settings)) return;

                    Installations.SyncInstallations(settings, encasedData.installation);
                    return;
                }

                // To Server
                if (package.Type == DataType.UpdateRelationAllies)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                    Utils.SetAlliesRelationWithNpc(settings, null, encasedData.relation);
                    return;
                }

                // To Server
                if (package.Type == DataType.RefreshConfig)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyEntity entity;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.claimBlockId, out entity)) return;

                    ClaimBlockConfig.GetClaimBlockConfigs();
                    //Utils.RefreshConfig();
                    Utils.SetBlockConfigs(entity as IMyTerminalBlock);

                    foreach(var item in Session.Instance.territoryConfigs)
                    {
                        SendClientConfig(item);
                    }

                    return;
                }

                // To Everyone
                if (package.Type == DataType.UpdateInstallation)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    ClaimBlockSettings settings;
                    if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings)) return;

                    if (!encasedData.boolean)
                        settings.GetInstallations.RemoveAll(x => x.Type == encasedData.installation.Type);
                    else
                    {
                        foreach (var item in settings.GetInstallations)
                            if (item.Type == encasedData.installation.Type) return;

                        settings._installations.Add(encasedData.installation);
                    }

                    return;
                }

                // To Everyone
                if (package.Type == DataType.SetRep)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    Utils.SetReputationWithFaction(encasedData.playerId, encasedData.toFaction, encasedData.integer);
                    return;
                }

                // To Server
                /*if (package.Type == DataType.AddPlayerToList)
                {
                    var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    IMyPlayer player = Triggers.GetPlayerFromId(encasedData.playerId);
                    if (player == null) return;

                    if (!Session.Instance.connectedPlayers.ContainsKey(encasedData.playerId))
                        Session.Instance.connectedPlayers.Add(encasedData.playerId, player);
                }*/

            }
            catch (Exception ex)
            {
                //MyVisualScriptLogicProvider.ShowNotificationToAll($"ERror with network: {ex}", 8000);
            }
        }

    }
}
