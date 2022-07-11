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
using VRageMath;

namespace Faction_TerritoriesV2
{
    /*public enum UIControls
    {
        Misc,
        Claiming,
        Sieging,
        Perks,
        TerritoryOptions
    }

    public enum PerkTypeList
    {
        Production
    }

    public enum PlayerPerks
    {
        Production,
        Drones,
        SafeZone,
        Radar
    }*/

    public enum TerritoryStatus
    {
        Offline,
        Neutral,
        Claimed,
        Claiming,
        Sieging,
        FailedSiegeCooldown,
        CooldownToNeutral
    }

    [ProtoContract(IgnoreListHandling = true)]
    public class ClaimBlockSettings
    {
        public static readonly string SettingsVersion = "1.00";

        [ProtoMember(1)]
        public long? _entityId = null;

        //[ProtoMember(2)]
        //public float _safeZoneSize;

        //[ProtoMember(3)]
        //public float _claimRadius;

        //[ProtoMember(4)]
        //public bool Sync;

        [ProtoMember(5)]
        public Vector3D? _blockPos = null;

        [ProtoMember(6)]
        public string _claimedFaction = null;

        [ProtoMember(7)]
        public long? _safeZoneEntity = null;

        [ProtoMember(8)]
        public bool? _enabled = null;

        [ProtoMember(9)]
        public bool? _isClaimed = null;

        //[ProtoMember(10)]
        //public int _toClaimTimer;

        //[ProtoIgnore]
        //public Dictionary<long, PlayerData> _playersInside;

        [ProtoMember(11)]
        public string _unclaimName = null;

        [ProtoMember(12)]
        public string _claimZoneName = null;

        [ProtoMember(13)]
        public List<long> _safeZones = null;

        [ProtoMember(14)]
        public int? _timer = null;

        [ProtoMember(15)]
        public bool? _isClaiming = null;

        //[ProtoMember(16)]
        //public double _distanceToClaim;

        [ProtoMember(17)]
        public string _detailInfo = null;

        [ProtoMember(18)]
        public long? _jdClaimingId = null;

        [ProtoMember(19)]
        public long? _playerClaimingId = null;

        [ProtoMember(20)]
        public int? _recoverTimer = null;

        [ProtoMember(21)]
        public List<ZonesDelayRemove> _zonesDelay = null;

        //[ProtoMember(22)]
        //public int _consumeTokenTimer;

        //[ProtoMember(23)]
        //public int _toSiegeTimer;

        //[ProtoMember(24)]
        //public int _tokensToClaim;

        //[ProtoMember(25)]
        //public int _tokensToSiege;

        [ProtoMember(26)]
        public bool? _isSieging = null;

        [ProtoMember(27)]
        public bool? _isSieged = null;

        //[ProtoMember(28)]
        //public int _zoneDeactivationTimer;

        //[ProtoMember(29)]
        //public int _gpsUpdateDelay;

        //[ProtoMember(30)]
        //public double _distanceToSiege;

        [ProtoMember(31)]
        public bool? _triggerInit = null;

        [ProtoMember(32)]
        public long? _playerSiegingId = null;

        [ProtoMember(33)]
        public long? _jdSiegingId = null;

        [ProtoMember(34)]
        public int? _siegeTimer = null;

        [ProtoMember(35)]
        public long? _discordRoleId = null;

        [ProtoMember(36)]
        public string _blockOwner = null;

        //[ProtoMember(37)]
        //public int _siegeNoficationFreq;

        [ProtoMember(38)]
        public long? _factionId = null;

        [ProtoMember(39)]
        public EmissiveState? _emissiveState = null;

        //[ProtoMember(40)]
        //public Dictionary<PerkType, PerkBase> _perks;

        //[ProtoMember(41)]
        //public UIControls _uiControls;

        //[ProtoMember(42)]
        //public PerkTypeList _uiPerkList;

        //[ProtoMember(43)]
        //public PlayerPerks _uiPlayerPerkList;

        //[ProtoMember(44)]
        //public bool _isSiegingFinal;

        //[ProtoMember(45)]
        //public bool _isSiegedFinal;

        [ProtoMember(46)]
        public string _version = null;

        [ProtoMember(47)]
        public bool? _isCooling = null;

        //[ProtoMember(48)]
        //public int _toSiegeFinalTimer;

        //[ProtoMember(49)]
        //public int _tokensToSiegeFinal;

        //[ProtoMember(50)]
        //public int _tokensToDelaySiege;

        //[ProtoMember(51)]
        //public int _siegeDelayed;

        //[ProtoMember(52)]
        //public int _timeToDelay;

        //[ProtoMember(53)]
        //public int _siegeDelayAllowed;

        //[ProtoMember(54)]
        //public int _cooldownTime;

        //[ProtoMember(55)]
        //public string _siegedBy;

        //[ProtoMember(56)]
        //public bool _readyToSiege;

        //[ProtoMember(57)]
        //public int _timeframeToSiege;

        //[ProtoMember(58)]
        //public bool _centerToPlanet;

        [ProtoMember(59)]
        public Vector3D? _planetCenter = null;

        //[ProtoMember(60)]
        //public bool _adminAllowSafeZoneAllies;

        //[ProtoMember(61)]
        //public bool _adminAllowTerritoryAllies;

        [ProtoMember(62)]
        public bool? _allowSafeZoneAllies = null;

        [ProtoMember(63)]
        public bool? _allowTerritoryAllies = null;

        //[ProtoMember(64)]
        //public bool _allowTools;

        //[ProtoMember(65)]
        //public bool _allowDrilling;

        //[ProtoMember(66)]
        //public bool _allowWelding;

        //[ProtoMember(67)]
        //public bool _allowGrinding;

        [ProtoMember(68)]
        public string _planetName = null;

        //[ProtoMember(69)]
        //public string _consumptionItem;

        //[ProtoMember(70)]
        //public int _consumptionAmt;

        [ProtoMember(71)]
        public bool? _isSiegeCooling = null;

        //[ProtoMember(72)]
        //public int _siegeCoolingTime;

        [ProtoMember(73)]
        public TerritoryStatus? _territoryStatus = null;

        [ProtoMember(74)]
        public TerritoryConfig? _territoryConfig = null;

        [ProtoMember(75)]
        public List<Installations> _installations = null;

        [ProtoIgnore]
        public ServerData _server;

        public ClaimBlockSettings() { _server = new ServerData(); }

        /*public ClaimBlockSettings()
        {
            _entityId = 0;
            //_safeZoneSize = 0;
            //_claimRadius = 0;
            //Sync = false;
            _blockPos = new Vector3D();
            _claimedFaction = "";
            _safeZoneEntity = 0;
            _enabled = false;
            _isClaimed = false;
            //_toClaimTimer = 300;
            //_playersInside = new Dictionary<long, PlayerData>();
            _claimZoneName = _unclaimName;
            _unclaimName = "";
            _safeZones = new List<long>();
            _timer = 0;
            _isClaiming = false;
            //_distanceToClaim = 0;
            _detailInfo = "";
            _jdClaimingId = 0;
            _playerClaimingId = 0;
            _recoverTimer = 0;
            _zonesDelay = new List<ZonesDelayRemove>();
            //_consumeTokenTimer = 3600;
            //_toSiegeTimer = 3600;
           // _tokensToClaim = 1000;
           // _tokensToSiege = 1000;
            _isSieging = false;
            _isSieged = false;
            //_zoneDeactivationTimer = 86400;
            //_gpsUpdateDelay = 30;
            //_distanceToSiege = 3000;
            _triggerInit = false;
            _playerSiegingId = 0;
            _jdSiegingId = 0;
            _siegeTimer = 0;
            _discordRoleId = 0;
            _blockOwner = "";
            //_siegeNoficationFreq = 10;
            _factionId = 0;
            _emissiveState = EmissiveState.Offline;
            //_perks = new Dictionary<PerkType, PerkBase>();
            //_uiControls = UIControls.Claiming;
            //_uiPerkList = PerkTypeList.Production;
            //_uiPlayerPerkList = PlayerPerks.Production;
           // _isSiegingFinal = false;
            //_isSiegedFinal = false;
            _version = SettingsVersion;
            _isCooling = false;
            //_toSiegeFinalTimer = 3600;
            //_tokensToSiegeFinal = 500;
            //_tokensToDelaySiege = 500;
            //_siegeDelayed = 0;
            //_timeToDelay = 6;
           //_siegeDelayAllowed = 3;
            //_cooldownTime = 1800;
            //_siegedBy = "";
            //_readyToSiege = false;
            //_timeframeToSiege = 1800;
            //_centerToPlanet = false;
            _planetCenter = new Vector3D();
            //_adminAllowSafeZoneAllies = false;
            //_adminAllowTerritoryAllies = false;
            _allowSafeZoneAllies = false;
            _allowTerritoryAllies = false;
            //_allowTools = false;
            //_allowDrilling = false;
            //_allowWelding = false;
            //_allowGrinding = false;
            _planetName = "";
            //_consumptionItem = "MyObjectBuilder_Component/ZoneChip";
            //_consumptionAmt = 1;
            _isSiegeCooling = false;
            //_siegeCoolingTime = 259200;
            _territoryStatus = TerritoryStatus.Neutral;
            _territoryConfig = new TerritoryConfig();
            _installations = new List<Installations>();
            _server = new ServerData();
            //_jdSieging = null;
            //_jdClaiming = null;
            //_playerClaiming = null;
            //_block = null;
            //_gpsData = new List<GpsData>();
            //_gridsInside = new Dictionary<long, GridData>();
        }*/

        public ClaimBlockSettings(long blockId, Vector3D pos, IMyTerminalBlock block)
        {
            _entityId = blockId;
            //_safeZoneSize = 1000f;
            //_claimRadius = IsOnPlanet(pos);
            //Sync = false;
            _blockPos = pos;
            _claimedFaction = "";
            _safeZoneEntity = 0;
            _enabled = false;
            _isClaimed = false;
            //_toClaimTimer = 300;
            //_playersInside = new Dictionary<long, PlayerData>();
            _claimZoneName = _unclaimName;
            _unclaimName = "";
            _safeZones = new List<long>();
            _timer = 0;
            _isClaiming = false;
            //_distanceToClaim = 3000;
            _detailInfo = "";
            _jdClaimingId = 0;
            _playerClaimingId = 0;
            _recoverTimer = 0;
            _zonesDelay = new List<ZonesDelayRemove>();
            //_consumeTokenTimer = 3600;
            //_toSiegeTimer = 3600;
           // _tokensToClaim = 1000;
            //_tokensToSiege = 1000;
            _isSieging = false;
            _isSieged = false;
            //_zoneDeactivationTimer = 86400;
            //_gpsUpdateDelay = 30;
            //_distanceToSiege = 3000;
            _triggerInit = false;
            _playerSiegingId = 0;
            _jdSiegingId = 0;
            _siegeTimer = 0;
            _discordRoleId = 0;
            _blockOwner = "";
            //_siegeNoficationFreq = 10;
            _factionId = 0;
            _emissiveState = EmissiveState.Offline;
            //_perks = new Dictionary<PerkType, PerkBase>();
            //_uiControls = UIControls.Claiming;
            //_uiPerkList = PerkTypeList.Production;
            //_uiPlayerPerkList = PlayerPerks.Production;
            //_isSiegingFinal = false;
            //_isSiegedFinal = false;
            _version = SettingsVersion;
            _isCooling = false;
            //_toSiegeFinalTimer = 3600;
            //_tokensToSiegeFinal = 500;
            //_tokensToDelaySiege = 500;
            //_siegeDelayed = 0;
            //_timeToDelay = 6;
            //_siegeDelayAllowed = 3;
            //_cooldownTime = 1800;
            //_siegedBy = "";
            //_readyToSiege = false;
            //_timeframeToSiege = 1800;
            //_centerToPlanet = false;
            _planetCenter = new Vector3D();
            //_adminAllowSafeZoneAllies = false;
            //_adminAllowTerritoryAllies = false;
            _allowSafeZoneAllies = false;
            _allowTerritoryAllies = false;
            //_allowTools = false;
            //_allowDrilling = false;
            //_allowWelding = false;
            //_allowGrinding = false;
            _planetName = "";
            //_consumptionItem = "MyObjectBuilder_Component/ZoneChip";
            //_consumptionAmt = 1;
            _isSiegeCooling = false;
            //_siegeCoolingTime = 259200;
            _territoryStatus = TerritoryStatus.Neutral;
            _territoryConfig = new TerritoryConfig();
            _installations = new List<Installations>();
            _server = new ServerData(block);
            //_jdSieging = null;
            //_jdClaiming = null;
            //_playerClaiming = null;
            //_block = block;
            //_gpsData = new List<GpsData>();
            //_gridsInside = new Dictionary<long, GridData>();
        }

        public void InstallationAdd(Installations addItem)
        {
            foreach(var item in _installations)
                if (item.Type == addItem.Type) return;

            _installations.Add(addItem);

            /*ClaimBlockSettings settings = new ClaimBlockSettings()
            {
                _installations = new List<Installations>(this._installations)
            };

            Comms.SyncSettingsToOthers(settings, _entityId ?? 0);*/
            Comms.UpdateInstallations(this, addItem, true);

            if (MyAPIGateway.Session.IsServer)
                Session.Instance.delayLCD = true;
        }

        public void InstallationRemove(Installations removeItem)
        {
            _installations.RemoveAll(x => x.Type == removeItem.Type);
            /*ClaimBlockSettings settings = new ClaimBlockSettings()
            {
                _installations = new List<Installations>(this._installations)
            };

            Comms.SyncSettingsToOthers(settings, _entityId ?? 0);*/
            Comms.UpdateInstallations(this, removeItem, false);

            if (MyAPIGateway.Session.IsServer)
                Session.Instance.delayLCD = true;
        }
        public List<Installations> GetInstallations
        {
            get
            {
                if (_installations == null)
                    _installations = new List<Installations>();

                return _installations;
            }
        }

        public Installations GetInstallationByType(InstallationType type)
        {
            return _installations.Find(x => x.Type == type) ?? null;
        }

        public Installations GetInstallationByType(string type)
        {
            return _installations.Find(x => type.Contains(x.Type.ToString())) ?? null;
        }

        public Installations GetInstallationByGridId(long gridId)
        {
            return _installations.Find(x => x.GridEntityId == gridId);
        }

        public TerritoryConfig TerritoryConfig
        {
            get { return _territoryConfig ?? new TerritoryConfig(); }
            set
            {
                _territoryConfig = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _territoryConfig = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public TerritoryStatus GetTerritoryStatus
        {
            get { return _territoryStatus ?? TerritoryStatus.Neutral; }
            set
            {
                _territoryStatus = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _territoryStatus = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);

                if (MyAPIGateway.Session.IsServer)
                    Session.Instance.delayLCD = true;
            }
        }

        public bool IsSiegeCooling
        {
            get { return _isSiegeCooling ?? false; }
            set
            {
                _isSiegeCooling = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _isSiegeCooling = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public int SiegeCoolingTime
        {
            get { return _siegeCoolingTime; }
            set
            {
                _siegeCoolingTime = value;
                _server.Sync = true;
            }
        }*/

        /*public int ConsumptinAmt
        {
            get { return _consumptionAmt; }
            set
            {
                _consumptionAmt = value;
                _server.Sync = true;
            }*
        }*/

        /*public string ConsumptionItem
        {
            get { return _consumptionItem; }
            set
            {
                _consumptionItem = value;
                _server.Sync = true;
            }
        }*/

        public string PlanetName
        {
            get { return _planetName ?? null; }
            set
            {
                _planetName = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _planetName = value
                };
                
                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public bool AllowGrinding
        {
            get { return _allowGrinding; }
            set
            {
                _allowGrinding = value;
                _server.Sync = true;
            }
        }*/

        /*public bool AllowWelding
        {
            get { return _allowWelding; }
            set
            {
                _allowWelding = value;
                _server.Sync = true;
            }
        }*/

        /*public bool AllowDrilling
        {
            get { return _allowDrilling; }
            set
            {
                _allowDrilling = value;
                _server.Sync = true;
            }
        }*/

        /*public bool AllowTools
        {
            get { return _allowTools; }
            set
            {
                _allowTools = value;
                _server.Sync = true;
            }
        }*/

        public bool AllowTerritoryAllies
        {
            get { return _allowTerritoryAllies ?? false; }
            set
            {
                if (!_allowSafeZoneAllies ?? false && value)
                    Comms.UpdateAlliesRelation(this, VRage.Game.MyRelationsBetweenFactions.Friends);
                else if (!_allowSafeZoneAllies ?? false && !value)
                    Comms.UpdateAlliesRelation(this, VRage.Game.MyRelationsBetweenFactions.Enemies);

                _allowTerritoryAllies = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _allowTerritoryAllies = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public bool AllowSafeZoneAllies
        {
            get { return _allowSafeZoneAllies ?? false; }
            set
            {
                if (!_allowTerritoryAllies ?? false && value)
                    Comms.UpdateAlliesRelation(this, VRage.Game.MyRelationsBetweenFactions.Friends);
                else if (!_allowTerritoryAllies ?? false && !value)
                    Comms.UpdateAlliesRelation(this, VRage.Game.MyRelationsBetweenFactions.Enemies);

                _allowSafeZoneAllies = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _allowSafeZoneAllies = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);

                if (!IsClaimed) return;
                if (Session.Instance.isServer)
                {
                    Utils.RemoveSafeZone(this);
                    if (IsSieging)
                        Utils.AddSafeZone(this, false);
                    else
                        Utils.AddSafeZone(this);
                }
                else
                {
                    Comms.UpdateSafeZoneAllies(this);
                }
            }
        }

        /*public bool AdminAllowTerritoryAllies
        {
            get { return _adminAllowTerritoryAllies; }
            set
            {
                _adminAllowTerritoryAllies = value;
                _server.Sync = true;
            }
        }*/

        /*public bool AdminAllowSafeZoneAllies
        {
            get { return _adminAllowSafeZoneAllies; }
            set
            {
                _adminAllowSafeZoneAllies = value;
                _server.Sync = true;
            }
        }*/

        public Vector3D PlanetCenter
        {
            get { return _planetCenter ?? new Vector3D(); }
            set
            {
                _planetCenter = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _planetCenter = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public bool CenterToPlanet
        {
            get { return _centerToPlanet; }
            set
            {
                _centerToPlanet = value;
                _server.Sync = true;
            }
        }*/

        /*public bool ReadyToSiege
        {
            get { return _readyToSiege; }
            set
            {
                _readyToSiege = value;
                _server.Sync = true;
            }
        }*/

        /*public int TimeframeToSiege
        {
            get { return _timeframeToSiege; }
            set
            {
                _timeframeToSiege = value;
                _server.Sync = true;
            }
        }*/

        /*public bool IsSiegingFinal
        {
            get { return _isSiegingFinal; }
            set
            {
                _isSiegingFinal = value;
                _server.Sync = true;
            }
        }*/

        /*public bool IsSiegeFinal
        {
            get { return _isSiegedFinal; }
            set
            {
                _isSiegedFinal = value;
                _server.Sync = true;
            }
        }*/

        public bool IsCooling
        {
            get { return _isCooling ?? false; }
            set
            {
                _isCooling = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _isCooling = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public int SiegeFinalTimer
        {
            get { return _toSiegeFinalTimer; }
            set
            {
                _toSiegeFinalTimer = value;
                _server.Sync = true;
            }
        }*/

        /*public int TokensSiegeFinal
        {
            get { return _tokensToSiegeFinal; }
            set
            {
                _tokensToSiegeFinal = value;
                _server.Sync = true;
            }
        }*/

        /*public int SiegeDelayAllow
        {
            get { return _siegeDelayAllowed; }
            set
            {
                _siegeDelayAllowed = value;
                _server.Sync = true;
            }
        }*/

        /*public int TokensSiegeDelay
        {
            get { return _tokensToDelaySiege; }
            set
            {
                _tokensToDelaySiege = value;
                _server.Sync = true;
            }
        }*/

        /*public int SiegedDelayedHit
        {
            get { return _siegeDelayed; }
            set
            {
                _siegeDelayed = value;
                _server.Sync = true;
            }
        }*/

        /*public int HoursToDelay
        {
            get { return _timeToDelay; }
            set
            {
                _timeToDelay = value;
                _server.Sync = true;
            }
        }*/

        /*public PlayerPerks UIPlayerPerks
        {
            get { return _uiPlayerPerkList; }
            set
            {
                _uiPlayerPerkList = value;
                _server.Sync = true;
            }
        }*/

        /*public int CooldownTimer
        {
            get { return _cooldownTime; }
            set
            {
                _cooldownTime = value;
                _server.Sync = true;
            }
        }*/

        /*public string SiegedBy
        {
            get { return _siegedBy; }
            set
            {
                _siegedBy = value;
                _server.Sync = true;
            }
        }*/

        /*public PerkTypeList UIPerkList
        {
            get { return _uiPerkList; }
            set
            {
                _uiPerkList = value;
                _server.Sync = true;
            }
        }*/

        /*public UIControls UI
        {
            get { return _uiControls; }
            set
            {
                _uiControls = value;
                _server.Sync = true;
            }
        }*/

        /*public Dictionary<PerkType, PerkBase> GetPerks
        {
            get { return _perks; }
        }*/

        /*public void UpdatePerks(PerkType perkType, bool add)
        {
            if (_perks == null)
                _perks = new Dictionary<PerkType, PerkBase>();

            if (add)
            {
                if (_perks.ContainsKey(perkType)) return;
                _perks.Add(perkType, new PerkBase(perkType, true));
                Comms.SyncSettingType(this, MyAPIGateway.Session.LocalHumanPlayer, SyncType.AddProductionPerk);
            }
            else
            {
                if (!_perks.ContainsKey(perkType)) return;
                if (IsClaimed)
                {
                    if (perkType == PerkType.Production)
                    {
                        Utils.RemovePerkType(this, PerkType.Production);
                        //Comms.SendRemoveProductionPerkToServer(this);
                    };
                }
            }
        }*/

        public ServerData Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public EmissiveState BlockEmissive
        {
            get { return _emissiveState ?? EmissiveState.Offline; }
            set
            {
                _emissiveState = value;

                IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
                if ((Session.Instance.isServer && !Session.Instance.isDedicated) || !Session.Instance.isServer)
                    Utils.SetEmissive(value, Block as IMyBeacon);

                Comms.SyncSettingType(this, player, SyncType.Emissive);
            }
        }

        public long FactionId
        {
            get { return _factionId ?? 0; }
            set
            {
                _factionId = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _factionId = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public int SiegeNoficationFreq
        {
            get { return _siegeNoficationFreq; }
            set
            {
                _siegeNoficationFreq = value;
                _server.Sync = true;
            }
        }*/

        public string BlockOwner
        {
            get { return _blockOwner ?? null; }
            set
            {
                _blockOwner = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _blockOwner = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public long DiscordRoleId
        {
            get { return _discordRoleId ?? 0; }
            set
            {
                _discordRoleId = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _discordRoleId = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public int SiegeTimer
        {
            get { return _siegeTimer ?? 0; }
            set
            {
                _siegeTimer = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _siegeTimer = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);

                if (MyAPIGateway.Session.IsServer)
                    Session.Instance.delayLCD = true;
            }
        }

        public long PlayerSiegingId
        {
            get { return _playerSiegingId ?? 0; }
            set
            {
                _playerSiegingId = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _playerSiegingId = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public long JDSiegingId
        {
            get { return _jdSiegingId ?? 0; }
            set
            {
                _jdSiegingId = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _jdSiegingId = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public bool TriggerInit
        {
            get { return _triggerInit ?? false; }
            set
            {
                _triggerInit = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _triggerInit = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public double DistanceToSiege
        {
            get { return _distanceToSiege; }
            set
            {
                _distanceToSiege = value;
                _server.Sync = true;
            }
        }*/

        /*public int GpsUpdateDelay
        {
            get { return _gpsUpdateDelay; }
            set
            {
                _gpsUpdateDelay = value;
                _server.Sync = true;
            }
        }*/

        /*public int ToClaimTimer
        {
            get { return _toClaimTimer; }
            set
            {
                _toClaimTimer = value;
                _server.Sync = true;
            }
        }*/

        /*public int ConsumeTokenTimer
        {
            get { return _consumeTokenTimer; }
            set
            {
                _consumeTokenTimer = value;
                _server.Sync = true;
            }
        }*/

        /*public int ToSiegeTimer
        {
            get { return _toSiegeTimer; }
            set
            {
                _toSiegeTimer = value;
                _server.Sync = true;
            }
        }*/

        /*public int TokensToClaim
        {
            get { return _tokensToClaim; }
            set
            {
                _tokensToClaim = value;
                _server.Sync = true;
            }
        }*/

        /*public int TokensToSiege
        {
            get { return _tokensToSiege; }
            set
            {
                _tokensToSiege = value;
                _server.Sync = true;
            }
        }*/

        public bool IsSieging
        {
            get { return _isSieging ?? false; }
            set
            {
                _isSieging = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _isSieging = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public bool IsSieged
        {
            get { return _isSieged ?? false; }
            set
            {
                _isSieged = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _isSieged = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public int ZoneDeactivationTimer
        {
            get { return _zoneDeactivationTimer; }
            set
            {
                _zoneDeactivationTimer = value;
                _server.Sync = true;
            }
        }*/

        public long EntityId
        {
            get { return _entityId ?? 0; }
            set
            {
                _entityId = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _entityId = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public float SafeZoneSize
        {
            get { return _safeZoneSize; }
            set
            {
                _safeZoneSize = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._safeZoneSize = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }*/

        public Vector3D BlockPos
        {
            get { return _blockPos ?? new Vector3D(); }
            set
            {
                _blockPos = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _blockPos = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public string ClaimedFaction
        {
            get { return _claimedFaction ?? null; }
            set
            {
                _claimedFaction = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _claimedFaction = value
                };
                
                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public float ClaimRadius
        {
            get { return _claimRadius; }
            set
            {
                _claimRadius = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._claimRadius = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }*/

        public long SafeZoneEntity
        {
            get { return _safeZoneEntity ?? 0; }
            set
            {
                _safeZoneEntity = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _safeZoneEntity = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public bool Enabled
        {
            get { return _enabled ?? false; }
            set
            {
                _enabled = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _enabled = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public bool IsClaimed
        {
            get { return _isClaimed ?? false; }
            set
            {
                _isClaimed = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _isClaimed = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public Dictionary<long, PlayerData> GetPlayersInside
        {
            get { return _server._playersInside; }
        }

        public void UpdatePlayerInside(long player, bool add)
        {
            if (add)
            {
                if (_server._playersInside.ContainsKey(player)) return;
                _server._playersInside.Add(player, new PlayerData(player));
            }

            else
            {
                if (player == 0)
                {
                    GPS.RemoveCachedGps(0, GpsType.Player, this);
                    _server._playersInside.Clear();
                    return;
                }


                if (_server._playersInside.ContainsKey(player))
                {
                    GPS.RemoveCachedGps(0, GpsType.Player, this, 0, player);
                    _server._playersInside.Remove(player);
                }
            }


            //_server.Sync = true;
        }

        public string ClaimZoneName
        {
            get { return _claimZoneName ?? null; }
            set
            {
                _claimZoneName = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _claimZoneName = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public string UnclaimName
        {
            get { return _unclaimName ?? null; }
            set
            {
                _unclaimName = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _unclaimName = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public List<long> GetSafeZones
        {
            get
            {
                if (_safeZones == null)
                    _safeZones = new List<long>();

                return _safeZones;
            }
        }

        public void UpdateSafeZones(long blockId, bool add)
        {
            if (_safeZones == null)
                _safeZones = new List<long>();

            if (add)
            {
                if (_safeZones.Contains(blockId)) return;
                _safeZones.Add(blockId);
            }
            else
                if (_safeZones.Contains(blockId)) _safeZones.Remove(blockId);

            ClaimBlockSettings settings = new ClaimBlockSettings()
            {
                _safeZones = new List<long>(this._safeZones)
            };

            Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
        }

        public int Timer
        {
            get { return _timer ?? 0; }
            set
            {
                _timer = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _timer = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
                
                if (!IsClaimed && IsClaiming || IsCooling)
                    if (MyAPIGateway.Session.IsServer)
                        Session.Instance.delayLCD = true;
            }
        }

        public bool IsClaiming
        {
            get { return _isClaiming ?? false; }
            set
            {
                _isClaiming = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _isClaiming = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        /*public double DistanceToClaim
        {
            get { return _distanceToClaim; }
            set
            {
                _distanceToClaim = value;
                _server.Sync = true;
                //Session.Instance.claimBlocks[_entityId]._distanceToClaim = value;
                //Session.Instance.claimBlocks[_entityId]._server.Sync = true;
            }
        }*/

        public string DetailInfo
        {
            get { return _detailInfo ?? null; }
            set
            {
                _detailInfo = value;
                IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;

                if (Session.Instance.claimBlocks[_entityId ?? 0].Block != null && !Session.Instance.isDedicated)
                {
                    IMyTerminalBlock block = Session.Instance.claimBlocks[_entityId ?? 0].Block;
                    block.RefreshCustomInfo();
                    ActionControls.RefreshControls(block, false);
                }

                Comms.SyncSettingType(this, player, SyncType.DetailInfo);
            }
        }

        public long JDClaimingId
        {
            get { return _jdClaimingId ?? 0; }
            set
            {
                _jdClaimingId = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _jdClaimingId = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public long PlayerClaimingId
        {
            get { return _playerClaimingId ?? 0; }
            set
            {
                _playerClaimingId = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _playerClaimingId = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public IMyEntity JDSieging
        {
            get { return _server._jdSieging; }
            set { _server._jdSieging = value; }
        }

        public IMyEntity JDClaiming
        {
            get { return _server._jdClaiming; }
            set { _server._jdClaiming = value; }
        }

        public IMyPlayer PlayerClaiming
        {
            get { return _server._playerClaiming; }
            set { _server._playerClaiming = value; }
        }

        public int RecoveryTimer
        {
            get { return _recoverTimer ?? 0; }
            set
            {
                _recoverTimer = value;
                ClaimBlockSettings settings = new ClaimBlockSettings()
                {
                    _recoverTimer = value
                };

                Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
            }
        }

        public IMyTerminalBlock Block
        {
            get
            {
                if (_server == null)
                    _server = new ServerData();

                return _server._block;
            }
            set
            {
                if (_server == null)
                    _server = new ServerData();

                _server._block = value;
            }
        }

        public List<ZonesDelayRemove> GetZonesDelayRemove
        {
            get
            {
                if (_zonesDelay == null)
                    _zonesDelay = new List<ZonesDelayRemove>();

                return _zonesDelay;
            }
        }

        public void UpdateZonesDelayRemove(long id, DateTime time, bool add)
        {
            if (add)
            {
                if (_zonesDelay == null)
                    _zonesDelay = new List<ZonesDelayRemove>();

                _zonesDelay.Add(new ZonesDelayRemove(id, time));
            }
            else
            {
                _zonesDelay.RemoveAll(x => x.zoneId == id);
            }

            ClaimBlockSettings settings = new ClaimBlockSettings()
            {
                _zonesDelay = new List<ZonesDelayRemove>(this._zonesDelay)
            };

            Comms.SyncSettingsToOthers(settings, _entityId ?? 0);
        }

        public Dictionary<long, GridData> GetGridsInside
        {
            get { return _server._gridsInside; }
        }

        public void UpdateGridsInside(long gridId, MyCubeGrid grid, bool add)
        {
            if (_server._gridsInside == null)
                _server._gridsInside = new Dictionary<long, GridData>();

            if (add)
            {
                if (_server._gridsInside.ContainsKey(gridId)) return;
                _server._gridsInside.Add(gridId, new GridData(gridId, grid));

            }
            else
            {
                if (!_server._gridsInside.ContainsKey(gridId)) return;
                if (_server._gridsInside[gridId].gpsData.Count != 0)
                    GPS.RemoveCachedGps(0, GpsType.Tag, this, gridId);

                if (grid != null)
                    UpdatesBlocksMonitored(grid, null, false, true);

                _server._gridsInside.Remove(gridId);
            }
        }

        public void UpdateGridData(long gridId, GridChangeType type, bool value)
        {
            if (!_server._gridsInside.ContainsKey(gridId)) return;

            if (type == GridChangeType.Controller)
                _server._gridsInside[gridId].hasController = value;

            if (type == GridChangeType.Power)
                _server._gridsInside[gridId].hasPower = value;
        }

        public List<GpsData> GetGpsData(long gridId)
        {
            if (_server._gridsInside.ContainsKey(gridId))
                return _server._gridsInside[gridId].gpsData;

            return new List<GpsData>();
        }

        public void UpdateGpsData(GpsData data, bool add)
        {
            if (data.gpsType == GpsType.Tag || data.gpsType == GpsType.Block)
            {
                if (_server._gridsInside == null)
                    _server._gridsInside = new Dictionary<long, GridData>();

                if (!_server._gridsInside.ContainsKey(data.entity.EntityId))
                    UpdateGridsInside(data.entity.EntityId, data.entity as MyCubeGrid, true);

                if (add)
                {
                    if (!_server._gridsInside.ContainsKey(data.entity.EntityId)) return;
                    _server._gridsInside[data.entity.EntityId].gpsData.Add(data);
                }
                else
                {
                    if (!_server._gridsInside.ContainsKey(data.entity.EntityId)) return;
                    _server._gridsInside[data.entity.EntityId].gpsData.Remove(data);
                }
            }

            if (data.gpsType == GpsType.Player)
            {
                if (_server._playersInside == null)
                    _server._playersInside = new Dictionary<long, PlayerData>();

                if (!_server._playersInside.ContainsKey(data.playerGps.IdentityId))
                    UpdatePlayerInside(data.playerGps.IdentityId, true);

                if (add)
                {
                    if (!_server._playersInside.ContainsKey(data.playerGps.IdentityId)) return;
                    _server._playersInside[data.playerGps.IdentityId].gpsData.Add(data);
                }
                else
                {
                    if (!_server._playersInside.ContainsKey(data.playerGps.IdentityId)) return;
                    _server._playersInside[data.playerGps.IdentityId].gpsData.Remove(data);
                }
            }



            //MyVisualScriptLogicProvider.ShowNotification($"Added gps data = {_gridsInside[data.entity.EntityId].gpsData.Count}", 8000);
            //Session.Instance.claimBlocks[_entityId]._gridsInside[data.entity.EntityId].gpsData = _gridsInside[data.entity.EntityId].gpsData;
        }

        public void UpdatesBlocksMonitored(MyCubeGrid grid, MyCubeBlock block, bool add, bool clearAll = false)
        {
            try
            {
                if (!_server._gridsInside.ContainsKey(grid.EntityId)) return;

                if (_server._gridsInside[grid.EntityId].blocksMonitored == null)
                    _server._gridsInside[grid.EntityId].blocksMonitored = new BlocksMonitored();

                if (clearAll)
                {
                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.controllers)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                    }

                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.powers)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                    }

                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.tools)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                    }

                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.drills)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                    }

                    foreach (var blocks in _server._gridsInside[grid.EntityId].blocksMonitored.production)
                    {
                        blocks.IsWorkingChanged -= Events.IsWorkingChanged;
                        blocks.OwnershipChanged -= Events.ProductionOwnershipChanged;

                        Utils.RemoveProductionPerk(this, block);
                    }

                    //Utils.RemovePerksFromGrid(this, grid);

                    _server._gridsInside[grid.EntityId].blocksMonitored.controllers.Clear();
                    _server._gridsInside[grid.EntityId].blocksMonitored.powers.Clear();
                    _server._gridsInside[grid.EntityId].blocksMonitored.tools.Clear();
                    _server._gridsInside[grid.EntityId].blocksMonitored.drills.Clear();
                    _server._gridsInside[grid.EntityId].blocksMonitored.production.Clear();

                    return;
                }

                if (add)
                {
                    if (block as IMyShipController != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.controllers.Contains(block as IMyShipController)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.controllers.Add(block as IMyShipController);
                    }

                    if (block as IMyPowerProducer != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.powers.Contains(block as IMyPowerProducer)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.powers.Add(block as IMyPowerProducer);
                    }

                    if (block as IMyShipToolBase != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.tools.Contains(block as IMyShipToolBase)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.tools.Add(block as IMyShipToolBase);
                    }

                    if (block as IMyShipDrill != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.drills.Contains(block as IMyShipDrill)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.drills.Add(block as IMyShipDrill);
                    }

                    if (block as IMyProductionBlock != null)
                    {
                        if (_server._gridsInside[grid.EntityId].blocksMonitored.production.Contains(block as IMyProductionBlock)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.production.Add(block as IMyProductionBlock);

                        var production = block as IMyProductionBlock;
                        production.OwnershipChanged += Events.ProductionOwnershipChanged;

                        //if (IsClaimed)
                        //Utils.AddAllProductionMultipliers(this, block, true);

                        if (IsClaimed)
                        {
                            IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                            if (blockFaction != null)
                            {
                                if (ClaimedFaction == blockFaction.Tag)
                                    Utils.AddProductionPerk(this, block);
                                else
                                {
                                    if (AllowTerritoryAllies && !Utils.IsFactionEnemy(this, blockFaction))
                                        Utils.AddProductionPerk(this, block);
                                }
                            }
                        }
                    }

                    block.IsWorkingChanged += Events.IsWorkingChanged;
                }
                else
                {
                    if (block as IMyShipController != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.controllers.Contains(block as IMyShipController)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.controllers.Remove(block as IMyShipController);
                    }

                    if (block as IMyPowerProducer != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.powers.Contains(block as IMyPowerProducer)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.powers.Remove(block as IMyPowerProducer);
                    }

                    if (block as IMyShipToolBase != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.tools.Contains(block as IMyShipToolBase)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.tools.Remove(block as IMyShipToolBase);
                    }

                    if (block as IMyShipDrill != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.drills.Contains(block as IMyShipDrill)) return;
                        _server._gridsInside[grid.EntityId].blocksMonitored.drills.Remove(block as IMyShipDrill);
                    }

                    if (block as IMyProductionBlock != null)
                    {
                        if (!_server._gridsInside[grid.EntityId].blocksMonitored.production.Contains(block as IMyProductionBlock)) return;

                        /*if (GetPerks.ContainsKey(PerkType.Production))
                        {
                            Utils.RemoveProductionMultipliers(this, block as IMyProductionBlock, true);
                        }*/

                        _server._gridsInside[grid.EntityId].blocksMonitored.production.Remove(block as IMyProductionBlock);

                        var production = block as IMyProductionBlock;
                        production.OwnershipChanged -= Events.ProductionOwnershipChanged;

                        if (IsClaimed)
                        {
                            IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                            if (blockFaction != null)
                            {
                                if (ClaimedFaction == blockFaction.Tag)
                                    Utils.RemoveProductionPerk(this, block);
                                else
                                {
                                    if (AllowTerritoryAllies && !Utils.IsFactionEnemy(this, blockFaction))
                                        Utils.RemoveProductionPerk(this, block);
                                }
                            }
                        }
                    }

                    block.IsWorkingChanged -= Events.IsWorkingChanged;
                }

            }
            catch (Exception ex)
            {

            }

        }

        private float IsOnPlanet(Vector3D pos)
        {
            if (MyVisualScriptLogicProvider.IsPlanetNearby(pos))
                return 25000f;
            else
                return 50000f;
        }

        public enum GridChangeType
        {
            Power,
            Controller,
            Both
        }

        public static void SyncSettings(ClaimBlockSettings newSettings, long claimBlockId)
        {
            ClaimBlockSettings settings = null;
            if (!Session.Instance.claimBlocks.TryGetValue(claimBlockId, out settings)) return;

            if (newSettings._entityId.HasValue)
                settings._entityId = newSettings.EntityId;
            if (newSettings._blockPos.HasValue)
                settings._blockPos = newSettings.BlockPos;
            if (!string.IsNullOrEmpty(newSettings._claimedFaction))
                settings._claimedFaction = newSettings.ClaimedFaction;
            if (newSettings._safeZoneEntity.HasValue)
                settings._safeZoneEntity = newSettings.SafeZoneEntity;
            if (newSettings._enabled.HasValue)
                settings._enabled = newSettings.Enabled;
            if (newSettings._isClaimed.HasValue)
                settings._isClaimed = newSettings.IsClaimed;
            if (!string.IsNullOrEmpty(newSettings._unclaimName))
                settings._unclaimName = newSettings.UnclaimName;
            if (!string.IsNullOrEmpty(newSettings._claimZoneName))
                settings._claimZoneName = newSettings.ClaimZoneName;
            if (newSettings._safeZones != null)
                settings._safeZones = newSettings.GetSafeZones;
            if (newSettings._timer.HasValue)
                settings._timer = newSettings.Timer;
            if (newSettings._isClaiming.HasValue)
                settings._isClaiming = newSettings.IsClaiming;
            if (newSettings._jdClaimingId.HasValue)
                settings._jdClaimingId = newSettings.JDClaimingId;
            if (newSettings._playerClaimingId.HasValue)
                settings._playerClaimingId = newSettings.PlayerClaimingId;
            if (newSettings._recoverTimer.HasValue)
                settings._recoverTimer = newSettings.RecoveryTimer;
            if (newSettings._zonesDelay != null)
                settings._zonesDelay = newSettings.GetZonesDelayRemove;
            if (newSettings._isSieging.HasValue)
                settings._isSieging = newSettings.IsSieging;
            if (newSettings._isSieged.HasValue)
                settings._isSieged = newSettings.IsSieged;
            if (newSettings._triggerInit.HasValue)
                settings._triggerInit = newSettings.TriggerInit;
            if (newSettings._playerSiegingId.HasValue)
                settings._playerSiegingId = newSettings.PlayerSiegingId;
            if (newSettings._jdSiegingId.HasValue)
                settings._jdSiegingId = newSettings.JDSiegingId;
            if (newSettings._siegeTimer.HasValue)
                settings._siegeTimer = newSettings.SiegeTimer;
            if (newSettings._discordRoleId.HasValue)
                settings._discordRoleId = newSettings.DiscordRoleId;
            if (!string.IsNullOrEmpty(newSettings._blockOwner))
                settings._blockOwner = newSettings.BlockOwner;
            if (newSettings._factionId.HasValue)
                settings._factionId = newSettings.FactionId;
            if (newSettings._isCooling.HasValue)
                settings._isCooling = newSettings.IsCooling;
            if (newSettings._planetCenter.HasValue)
                settings._planetCenter = newSettings.PlanetCenter;
            if (newSettings._allowSafeZoneAllies.HasValue)
                settings._allowSafeZoneAllies = newSettings.AllowSafeZoneAllies;
            if (newSettings._allowTerritoryAllies.HasValue)
                settings._allowTerritoryAllies = newSettings.AllowTerritoryAllies;
            if (!string.IsNullOrEmpty(newSettings._planetName))
                settings._planetName = newSettings.PlanetName;
            if (newSettings._isSiegeCooling.HasValue)
                settings._isSiegeCooling = newSettings.IsSiegeCooling;
            if (newSettings._territoryStatus.HasValue)
                settings._territoryStatus = newSettings.GetTerritoryStatus;
            if (newSettings._territoryConfig.HasValue)
                settings._territoryConfig = newSettings.TerritoryConfig;
            if (newSettings._installations != null)
            {
                List<Installations> temps = new List<Installations>();
                foreach(var installation in settings._installations)
                    temps.Add(installation);

                settings._installations.Clear();
                settings._installations = newSettings.GetInstallations;

                foreach(var installation in settings._installations)
                {
                    foreach(var temp in temps)
                    {
                        if (temp.Type == installation.Type)
                        {
                            installation._turrets = new List<IMyTerminalBlock>(temp._turrets);
                            installation._installationParticle = temp._installationParticle;
                        }
                    }
                }



                /*for (int i = 0; i < settings._installations.Count; i++)
                {
                    var turrets = settings._installations[i]._turrets;
                    var particle = settings._installations[i]._installationParticle;

                    foreach(var newInstallation in newSettings.GetInstallations)
                    {
                        if (newInstallation.Type == settings._installations[i].Type)
                        {
                            settings._installations[i] = newInstallation;
                            settings._installations[i]._turrets = new List<IMyTerminalBlock>(turrets);
                            settings._installations[i]._installationParticle = particle;
                            break;
                        }
                    }


                }*/
            }
                //settings._installations = newSettings.GetInstallations;
        }
    }

    public class MyCallbacks
    {
        public ClaimBlockSettings _settings;
        public Installations _installation;
        public Vector3D _spawnCoords;

        public MyCallbacks() { }

        public MyCallbacks(ClaimBlockSettings settings, Installations installation, Vector3D spawn)
        {
            _settings = settings;
            _installation = installation;
            _spawnCoords = spawn;
        }

        public void InstallationSpawnCallback(IMyEntity ent)
        {
            ClaimBlockSettings mySettings;
            Installations myInstallation;
            if (!Session.Instance.claimBlocks.TryGetValue(_settings.EntityId, out mySettings)) return;
            myInstallation = mySettings.GetInstallationByType(_installation.Type);
            if (myInstallation == null) return;

            IMyCubeGrid cubeGrid = ent as IMyCubeGrid;
            MyCubeGrid grid = ent as MyCubeGrid;
            if (cubeGrid == null || grid == null) return;
            cubeGrid.IsStatic = true;
            grid.Editable = false;
            cubeGrid.CustomName = ($"{mySettings.UnclaimName} Territory - {myInstallation.Type}(Installation) [NPC-IGNORE]");
            cubeGrid.OnGridSplit += Events.OnInstallationSplit;

            myInstallation.GridEntityId = ent.EntityId;
            //myInstallation.ParentId = mySettings.EntityId;

            Utils.blocks.Clear();
            cubeGrid.GetBlocks(Utils.blocks);
            //MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(beacons);
            foreach (var block in Utils.blocks)
            {
                IMyCubeBlock fat = block.FatBlock;
                if (fat == null) continue;

                IMyBeacon beacon = fat as IMyBeacon;
                if (beacon == null) continue;

                if (beacon.BlockDefinition.SubtypeName == "InstallationBeacon")
                {
                    myInstallation.BlockEntityId = beacon.EntityId;
                    beacon.HudText = $"{myInstallation.Type} Installation Integrity: {Math.Round((((beacon.SlimBlock.BuildIntegrity - beacon.SlimBlock.CurrentDamage)) / beacon.SlimBlock.MaxIntegrity) * 100),2}%";
                    beacon.Radius = 10000;
                    break;
                }
            }

            myInstallation.Enabled = true;
            myInstallation.IntegrityWarning = false;
            myInstallation.Coords = _spawnCoords;
            myInstallation.Integrity = 100f;
            myInstallation.ResourceTimer = 10;
            myInstallation.ResearchTimer = 10;
            Utils.GetInstallationTurrets(myInstallation, cubeGrid);
            //_settings.InstallationAdd(myInstallation);
            //MyAPIGateway.Parallel.Sleep(1000);
            Utils.AddInstallationPerk(mySettings, myInstallation);
        }
    }

    public class ServerData
    {
        public IMyEntity _jdSieging;
        public IMyEntity _jdClaiming;
        public IMyPlayer _playerClaiming;
        public IMyTerminalBlock _block;
        public bool Sync;
        public Dictionary<long, GridData> _gridsInside;
        public Dictionary<long, PlayerData> _playersInside;
        public bool _perkWarning;
        public List<IMyProgrammableBlock> _pbList;
        public bool _save;

        public ServerData()
        {
            _jdSieging = null;
            _jdClaiming = null;
            _playerClaiming = null;
            _block = null;
            Sync = false;
            _gridsInside = new Dictionary<long, GridData>();
            _playersInside = new Dictionary<long, PlayerData>();
            _perkWarning = false;
            _pbList = new List<IMyProgrammableBlock>();
            _save = false;
        }

        public ServerData(IMyTerminalBlock block)
        {
            _jdSieging = null;
            _jdClaiming = null;
            _playerClaiming = null;
            _block = block;
            Sync = false;
            _gridsInside = new Dictionary<long, GridData>();
            _playersInside = new Dictionary<long, PlayerData>();
            _perkWarning = false;
            _pbList = new List<IMyProgrammableBlock>();
            _save = false;
        }
    }

    /*public enum PerkType
    {
        Production,
        Drones,
        SafeZone,
        Radar
    }*/

    /*[ProtoContract]
    public class PerkBase
    {
        [ProtoMember(50)] public bool enabled;
        [ProtoMember(51)] public PerkType type;
        [ProtoMember(52)] public Perk perk;
        [ProtoMember(53)] public PlayerPerks playerPerks;

        public PerkBase()
        {
            enabled = false;
            type = PerkType.Production;
            perk = new Perk();
            playerPerks = PlayerPerks.Production;
        }

        public PerkBase(PerkType perkType, bool enable)
        {
            enabled = enable;
            type = perkType;
            perk = new Perk(perkType);
            playerPerks = PlayerPerks.Production;

            //_server.Sync = true;
        }

        public PlayerPerks PlayerPerksUI
        {
            get { return playerPerks; }
            set { playerPerks = value; }
        }

        public bool Enable
        {
            get { return enabled; }
            set
            {
                enabled = value;
                //perkSync = true;
            }
        }

        public int TotalPerkCost
        {
            get
            {
                int cost = 0;
                if (type == PerkType.Production)
                {
                    if (perk.productionPerk.allowClientControlSpeed && perk.productionPerk.enableClientControlSpeed)
                        cost += perk.productionPerk.speedTokens;

                    if (perk.productionPerk.allowClientControlYield && perk.productionPerk.enableClientControlYield)
                        cost += perk.productionPerk.yieldTokens;

                    if (perk.productionPerk.allowClientControlEnergy && perk.productionPerk.enableClientControlEnergy)
                        cost += perk.productionPerk.energyTokens;
                }


                return cost;
            }
        }

        public int ActivePerkCost
        {
            get
            {
                int cost = 0;
                if (type == PerkType.Production)
                {
                    if (perk.productionPerk.GetActiveUpgrades.Count == 0) return 0;
                    foreach (var upgrade in perk.productionPerk.GetActiveUpgrades)
                    {
                        if (upgrade == "Productivity")
                            cost += perk.productionPerk.speedTokens;

                        if (upgrade == "Effectiveness")
                            cost += perk.productionPerk.yieldTokens;

                        if (upgrade == "PowerEfficiency")
                            cost += perk.productionPerk.energyTokens;
                    }
                }

                return cost;
            }
        }

        public int PendingPerkCost
        {
            get
            {
                int cost = ActivePerkCost;
                if (type == PerkType.Production)
                {
                    if (perk.productionPerk.GetPendingAddUpgrades.Count != 0)
                    {
                        foreach (var upgrade in perk.productionPerk.GetPendingAddUpgrades)
                        {
                            if (upgrade == "Productivity")
                                cost += perk.productionPerk.speedTokens;

                            if (upgrade == "Effectiveness")
                                cost += perk.productionPerk.yieldTokens;

                            if (upgrade == "PowerEfficiency")
                                cost += perk.productionPerk.energyTokens;
                        }
                    }

                    if (perk.productionPerk.GetPendingRemoveUpgrades.Count != 0)
                    {
                        foreach (var upgrade in perk.productionPerk.GetPendingRemoveUpgrades)
                        {
                            if (upgrade == "Productivity")
                                cost -= perk.productionPerk.speedTokens;

                            if (upgrade == "Effectiveness")
                                cost -= perk.productionPerk.yieldTokens;

                            if (upgrade == "PowerEfficiency")
                                cost -= perk.productionPerk.energyTokens;
                        }
                    }
                }

                return cost;
            }
        }
    }*/

    /*[ProtoContract]
    public class Perk
    {
        [ProtoMember(53)] public ProductionPerk productionPerk;

        public Perk()
        {
            productionPerk = new ProductionPerk();
        }

        public Perk(PerkType perkType)
        {
            if (perkType == PerkType.Production)
            {
                productionPerk = new ProductionPerk();
            }
        }
    }*/

    /*[ProtoContract]
    public class ProductionPerk
    {
        [ProtoMember(54)] public float speed;
        [ProtoMember(55)] public float yield;
        [ProtoMember(56)] public float energy;
        [ProtoMember(57)] public List<long> attachedEntities;
        [ProtoMember(58)] public bool allowClientControlSpeed;
        [ProtoMember(59)] public bool allowClientControlYield;
        [ProtoMember(60)] public bool allowClientControlEnergy;
        [ProtoMember(61)] public bool allowStandAlone;
        [ProtoMember(62)] public int speedTokens;
        [ProtoMember(63)] public int yieldTokens;
        [ProtoMember(64)] public int energyTokens;
        [ProtoMember(65)] public bool enableClientControlSpeed;
        [ProtoMember(66)] public bool enableClientControlYield;
        [ProtoMember(67)] public bool enableClientControlEnergy;
        [ProtoMember(68)] public bool productionRunning;
        [ProtoMember(69)] public List<string> activeUpgrades;
        [ProtoMember(70)] public List<string> pendingAddUpgrades;
        [ProtoMember(71)] public List<string> pendingRemoveUpgrades;


        public ProductionPerk()
        {
            speed = 0f;
            yield = 0f;
            energy = 0f;
            attachedEntities = new List<long>();
            allowStandAlone = false;
            allowClientControlSpeed = false;
            allowClientControlYield = false;
            allowClientControlEnergy = false;
            speedTokens = 0;
            yieldTokens = 0;
            energyTokens = 0;
            enableClientControlSpeed = false;
            enableClientControlYield = false;
            enableClientControlEnergy = false;
            productionRunning = false;
            activeUpgrades = new List<string>();
            pendingAddUpgrades = new List<string>();
            pendingRemoveUpgrades = new List<string>();
        }

        public void PendingAddUpgrades(string upgradeName, bool add)
        {
            if (pendingAddUpgrades == null)
                pendingAddUpgrades = new List<string>();

            if (add)
            {
                if (pendingAddUpgrades.Contains(upgradeName)) return;
                pendingAddUpgrades.Add(upgradeName);
            }
            else
            {
                if (!pendingAddUpgrades.Contains(upgradeName)) return;
                pendingAddUpgrades.Remove(upgradeName);
            }
        }

        public void PendingRemoveUpgrades(string upgradeName, bool add)
        {
            if (pendingRemoveUpgrades == null)
                pendingRemoveUpgrades = new List<string>();

            if (add)
            {
                if (pendingRemoveUpgrades.Contains(upgradeName)) return;
                pendingRemoveUpgrades.Add(upgradeName);
            }
            else
            {
                if (!pendingRemoveUpgrades.Contains(upgradeName)) return;
                pendingRemoveUpgrades.Remove(upgradeName);
            }
        }

        public List<string> GetPendingAddUpgrades
        {
            get { return pendingAddUpgrades; }
        }

        public List<string> GetPendingRemoveUpgrades
        {
            get { return pendingRemoveUpgrades; }
        }

        public void ActiveUprades(string upgradeName, bool add)
        {
            if (activeUpgrades == null)
                activeUpgrades = new List<string>();

            if (add)
            {
                if (activeUpgrades.Contains(upgradeName)) return;
                activeUpgrades.Add(upgradeName);
            }
            else
            {
                if (!activeUpgrades.Contains(upgradeName)) return;
                activeUpgrades.Remove(upgradeName);
            }
        }

        public List<string> GetActiveUpgrades
        {
            get { return activeUpgrades; }
        }

        public bool ProductionRunning
        {
            get { return productionRunning; }
            set { productionRunning = value; }
        }

        public List<long> GetAttachedEntities
        {
            get { return attachedEntities; }
        }

        public void UpdateAttachedEntities(long entityId, bool add)
        {
            //MyVisualScriptLogicProvider.ShowNotificationToAll($"Added Attached Production EntityId = {entityId} | IsServer = {MyAPIGateway.Multiplayer.IsServer}", 20000, "Red");
            if (attachedEntities == null)
                attachedEntities = new List<long>();

            if (add)
            {
                if (attachedEntities.Contains(entityId)) return;
                attachedEntities.Add(entityId);
            }
            else
            {
                if (!attachedEntities.Contains(entityId)) return;
                attachedEntities.Remove(entityId);
            }

            //perkSync = true;
        }

        public float Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                //_server.Sync = true;
            }
        }

        public float Yield
        {
            get { return yield; }
            set
            {
                yield = value;
                //_server.Sync = true;
            }
        }

        public float Energy
        {
            get { return energy; }
            set
            {
                energy = value;
                //_server.Sync = true;
            }
        }
    }*/

    public class GridData
    {
        public long gridId;
        public MyCubeGrid cubeGrid;
        public bool hasController;
        public bool hasPower;
        public bool hasGps;
        public List<GpsData> gpsData;
        public BlocksMonitored blocksMonitored;

        public GridData()
        {

        }

        public GridData(long Id, MyCubeGrid grid)
        {
            gridId = Id;
            cubeGrid = grid;
            hasController = false;
            hasPower = false;
            hasGps = false;
            gpsData = new List<GpsData>();
            blocksMonitored = new BlocksMonitored();

            //data.blocksMonitored.controllers = new List<IMyShipController>();
            //data.blocksMonitored.powers = new List<IMyPowerProducer>();
            //data.blocksMonitored.tools = new List<IMyShipToolBase>();
        }
    }

    public class PlayerData
    {
        public long playerId;
        public List<GpsData> gpsData;

        public PlayerData()
        {

        }

        public PlayerData(long id)
        {
            playerId = id;
            gpsData = new List<GpsData>();
        }
    }

    public class BlocksMonitored
    {
        public List<IMyShipController> controllers = new List<IMyShipController>();
        public List<IMyPowerProducer> powers = new List<IMyPowerProducer>();
        public List<IMyShipToolBase> tools = new List<IMyShipToolBase>();
        public List<IMyShipDrill> drills = new List<IMyShipDrill>();
        public List<IMyProductionBlock> production = new List<IMyProductionBlock>();

        public BlocksMonitored()
        {
            controllers = new List<IMyShipController>();
            powers = new List<IMyPowerProducer>();
            tools = new List<IMyShipToolBase>();
            drills = new List<IMyShipDrill>();
            production = new List<IMyProductionBlock>();
        }
    }

    [ProtoContract]
    public class ZonesDelayRemove
    {
        [ProtoMember(100)]
        public long zoneId;

        [ProtoMember(101)]
        public DateTime time;

        public ZonesDelayRemove()
        {
            zoneId = 0;
            time = DateTime.Now;
        }

        public ZonesDelayRemove(long id, DateTime delay)
        {
            zoneId = id;
            time = delay;
        }
    }
}
