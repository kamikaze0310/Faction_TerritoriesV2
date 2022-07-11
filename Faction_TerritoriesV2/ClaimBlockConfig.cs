using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRageMath;
using static Faction_TerritoriesV2.TerritoryInstallations;

namespace Faction_TerritoriesV2
{
    public enum InstallationType
    {
        Production = 1,
        SafeZone = 2,
        Radar = 3,
        Drone = 4,
        Resource = 5,
        Research = 6
    }

    [ProtoContract] [XmlRoot("TerritoryConfigs")]
    public class ClaimBlockConfig
    {
        [ProtoIgnore] [XmlIgnore] public string _version = "1.00";
        [ProtoMember(1)] [XmlElement("TerritoryConfig")] public TerritoryConfig[] _config;

        public static void GetClaimBlockConfigs()
        {
            if (MyAPIGateway.Utilities.FileExistsInLocalStorage("Territory_Settings.xml", typeof(ClaimBlockConfig)) == true)
            {
                try
                {
                    ClaimBlockConfig defaults = new ClaimBlockConfig();
                    var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage("Territory_Settings.xml", typeof(ClaimBlockConfig));
                    string content = reader.ReadToEnd();

                    reader.Close();
                    var settings = MyAPIGateway.Utilities.SerializeFromXML<ClaimBlockConfig>(content);
                    if (settings == null)
                    {
                        CreateNewFile();
                        return;
                    }

                    if (settings._version == defaults._version)
                    {
                        AddConfigs(settings);
                        return;
                    }

                    settings._version = defaults._version;
                    Session.Instance.config = settings;
                    SaveConfig(settings);
                    return;
                }
                catch (Exception ex)
                {
                    CreateNewFile();
                    return;
                }
            }

            CreateNewFile();
        }

        public static void AddConfigs(ClaimBlockConfig config)
        {
            Session.Instance.config = config;
            Session.Instance.territoryConfigs.Clear();
            foreach (var tConfig in config._config)
                Session.Instance.territoryConfigs.Add(tConfig);
        }

        public static void CreateNewFile()
        {
            List<TerritoryConfig> territoryConfigs = new List<TerritoryConfig>();
            List<TerritoryInstallations> installations = new List<TerritoryInstallations>();
            foreach (InstallationType installType in Enum.GetValues(typeof(InstallationType)))
            {
                installations.Add(new TerritoryInstallations()
                {   _installationType = installType,
                    _prefabName = "PrefabName"
                    //_installationDefense = new InstallationDefenses()
                });
            }

            List<ResourceItem> resourceItems = new List<ResourceItem>();
            List<ResourceItem> researchItems = new List<ResourceItem>();
            ResourceItem resourceItem = new ResourceItem()
            {
                _resourceId = "MyObjectBuilder_Ingot/Iron",
                _frequency = 1
            };
            ResourceItem researchItem = new ResourceItem()
            {
                _resourceId = "MyObjectBuilder_Component/Steelplate",
                _frequency = 1
            };
            resourceItems.Add(resourceItem);
            researchItems.Add(researchItem);

            ClaimBlockConfig defaults = new ClaimBlockConfig();
            TerritoryConfig territoryConfig = new TerritoryConfig()
            {
                _npcTag = "SPRT",
                _discordRoleId = 0,
                _token = "MyObjectBuilder_Component/ZoneChip",
                _territoryName = "Name",
                _safeZoneRadius = 500,
                _territoryRadius = 25000,
                _claimingConfig = new ClaimingConfig()
                {
                    _claimingTime = 300,
                    _distanceToClaim = 3000,
                    _tokensToClaim = 500
                },
                _siegingConfig = new SiegingConfig()
                {
                    _siegingTime = 300,
                    _distanceToSiege = 3000,
                    _tokensToSiege = 500
                },
                _territoryOptions = new TerritoryOptions(),
                _perkConfig = new PerksConfig()
                {
                    _productionConfig = new ProductionConfig(),
                    _safeZoneConfig = new SafeZoneConfig(),
                    _radarConfig = new RadarConfig(),
                    _droneConfig = new DroneConfig(),
                    _resourceConfig = new ResourceConfig()
                    {
                        _resourcePrefab = "PrefabSubtype",
                        _resourceItems = resourceItems.ToArray()
                    },
                    _researchConfig = new ResearchConfig()
                    {
                        _researchPrefab = "PrefabSubtype",
                        _resourceItems = researchItems.ToArray()
                    },
                },
                _territoryInstallations = installations.ToArray()
            };

            territoryConfigs.Add(territoryConfig);
            defaults._config = territoryConfigs.ToArray();
            AddConfigs(defaults);
            SaveConfig(defaults);
        }

        public static void SaveConfig(ClaimBlockConfig config)
        {
            if (config == null) return;
            try
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage("Territory_Settings.xml", typeof(ClaimBlockConfig)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                VRage.Utils.MyLog.Default.WriteLineAndConsole($"TerritoriesV2: Error trying to save config!\n {ex.ToString()}");
            }
        }
    }

    [ProtoContract]
    public struct TerritoryConfig
    {
        [ProtoMember(1)] [XmlElement("DiscordRoleId")] public long _discordRoleId;
        [ProtoMember(2)] [XmlElement("NpcTag")] public string _npcTag;
        [ProtoMember(3)] [XmlElement("Token")] public string _token;
        [ProtoMember(4)] [XmlElement("TerritoryName")] public string _territoryName;
        [ProtoMember(5)] [XmlElement("SafeZoneRadius")] public float _safeZoneRadius;
        [ProtoMember(6)] [XmlElement("TerritoryRadius")] public float _territoryRadius;
        [ProtoMember(7)] [XmlElement("TerritoryMaintenace")] public TerritoryMaintenance _territoryMaintenance;
        [ProtoMember(8)] [XmlElement("ClaimingConfig")] public ClaimingConfig _claimingConfig;
        [ProtoMember(9)] [XmlElement("SiegingConfig")] public SiegingConfig _siegingConfig;
        [ProtoMember(10)] [XmlElement("TerritoryOptions")] public TerritoryOptions _territoryOptions;
        [ProtoMember(11)] [XmlElement("PerksConfig")] public PerksConfig _perkConfig;
        [ProtoMember(12)] [XmlElement("TerritoryInstallations")] public TerritoryInstallations[] _territoryInstallations;
    }

    [ProtoContract]
    public struct TerritoryMaintenance
    {
        [ProtoMember(1)] [XmlElement("ConsumptionAmt")] public int _consumptionAmt;
        [ProtoMember(2)] [XmlElement("ConsumptionTime")] public int _consumptionTime;
    }

    [ProtoContract]
    public struct ClaimingConfig
    {
        [ProtoMember(1)] [XmlElement("ClaimingTime")] public int _claimingTime;
        [ProtoMember(2)] [XmlElement("DistanceToClaim")] public float _distanceToClaim;
        [ProtoMember(3)] [XmlElement("TokensToClaim")] public int _tokensToClaim;
    }

    [ProtoContract]
    public struct SiegingConfig
    {
        [ProtoMember(1)] [XmlElement("SiegingTime")] public int _siegingTime;
        [ProtoMember(2)] [XmlElement("DistanceToSiege")] public float _distanceToSiege;
        [ProtoMember(3)] [XmlElement("TokensToSiege")] public int _tokensToSiege;
        [ProtoMember(4)] [XmlElement("SiegeCooldown")] public int _siegeCooldown;
    }

    [ProtoContract]
    public struct TerritoryOptions
    {
        [ProtoMember(1)] [XmlElement("AllowAllTools")] public bool _allowTools;
        [ProtoMember(2)] [XmlElement("AllowWelding")] public bool _allowWelding;
        [ProtoMember(3)] [XmlElement("AllowGrinding")] public bool _allowGrinding;
        [ProtoMember(4)] [XmlElement("AllowDrilling")] public bool _allowDrilling;
        [ProtoMember(5)] [XmlElement("CenterOnPlanet")] public bool _centerOnPlanet;
        [ProtoMember(6)] [XmlElement("CooldownToClaim")] public int _cooldown;
    }

    [ProtoContract]
    public struct PerksConfig
    {
        [ProtoMember(1)] [XmlElement("ProductionConfig")] public ProductionConfig _productionConfig;
        [ProtoMember(2)] [XmlElement("SafeZoneConfig")] public SafeZoneConfig _safeZoneConfig;
        [ProtoMember(3)] [XmlElement("RadarConfig")] public RadarConfig _radarConfig;
        [ProtoMember(4)] [XmlElement("DroneConfig")] public DroneConfig _droneConfig;
        [ProtoMember(5)] [XmlElement("ResourceConfig")] public ResourceConfig _resourceConfig;
        [ProtoMember(6)] [XmlElement("ResearchConfig")] public ResearchConfig _researchConfig;
    }

    [ProtoContract]
    public struct ProductionConfig
    {
        [ProtoMember(1)] [XmlElement("ProductionSpeed")] public float _speed;
        [ProtoMember(2)] [XmlElement("ProductionYield")] public float _yield;
        [ProtoMember(3)] [XmlElement("ProductionEnergy")] public float _energy;
    }

    [ProtoContract]
    public struct SafeZoneConfig
    {
        [ProtoMember(1)] [XmlElement("SafeZoneExtensionMultiplier")] public float _safeZoneExtensionMultiplier;
    }

    [ProtoContract]
    public struct RadarConfig
    {
        [ProtoMember(1)] [XmlElement("UpdateFrequency")] public int _updateFreq;
    }

    [ProtoContract]
    public struct DroneConfig
    {
        [ProtoMember(1)] [XmlElement("Radius")] public double _radius;
        [ProtoMember(2)] [XmlElement("MaxSpawns")] public int _maxSpawns;
        [ProtoMember(3)] [XmlElement("MinThreat")] public int _minThreat;
    }

    [ProtoContract]
    public struct ResourceConfig
    {
        [ProtoMember(1)] [XmlElement("ResourcePrefab")] public string _resourcePrefab;
        [ProtoMember(2)] [XmlElement("SecondsToSpawn")] public int _secondsToSpawn;
        [ProtoMember(3)] [XmlElement("ResourceItem")] public ResourceItem[] _resourceItems;
    }

    [ProtoContract]
    public struct ResourceItem
    {
        [ProtoMember(1)] [XmlElement("ResourceId")] public string _resourceId;
        [ProtoMember(2)] [XmlElement("AmountMin")] public int _amountMin;
        [ProtoMember(3)] [XmlElement("AmountMax")] public int _amountMax;
        [ProtoMember(4)] [XmlElement("Frequency")] public int _frequency;
    }

    [ProtoContract]
    public struct ResearchConfig
    {
        [ProtoMember(1)] [XmlElement("ResearchPrefab")] public string _researchPrefab;
        [ProtoMember(2)] [XmlElement("SecondsToSpawn")] public int _secondsToSpawn;
        [ProtoMember(3)] [XmlElement("ResearchItem")] public ResourceItem[] _resourceItems;
    }

    /*[ProtoContract]
    public struct ResearchItem
    {
        [ProtoMember(1)] [XmlElement("ResearchId")] public string _researchId;
        [ProtoMember(2)] [XmlElement("AmountMin")] public int _amountMin;
        [ProtoMember(3)] [XmlElement("AmountMax")] public int _amountMax;
        [ProtoMember(4)] [XmlElement("Frequency")] public int _frequency;
    }*/

    [ProtoContract]
    public struct TerritoryInstallations
    {
        [ProtoMember(1)] [XmlElement("InstallationType")] public InstallationType _installationType;
        [ProtoMember(2)] [XmlElement("PrefabName")] public string _prefabName;
        //[ProtoMember(3)] [XmlElement("SpawnOnSurface")] public bool _spawnOnSurface;
        [ProtoMember(4)] [XmlElement("InstallationCost")] public int _installationCost;
        [ProtoMember(5)] [XmlElement("AddSiegeTimeAmt")] public int _addSiegeTimeAmt;
        [ProtoMember(6)] [XmlElement("RebuildCooldown")] public int _rebuildCooldown;
        //[ProtoMember(7)] [XmlElement("EnemyCheckRebuild")] public bool _enemyCheckRebuild;
        [ProtoMember(8)] [XmlElement("DamageModifier")] public float _damageModifier;
        //[ProtoMember(9)] [XmlElement("InstallationDefense")] public InstallationDefenses _installationDefense;
    }

    /*[ProtoContract]
    public struct InstallationDefenses
    {
        [ProtoMember(1)] [XmlElement("EnableDroneDefenses")] public bool _enableDroneDefenses;
        [ProtoMember(2)] [XmlElement("Radius")] public double _defenseRadius;
        [ProtoMember(3)] [XmlElement("MaxSpawns")] public int _maxDefenseSpawns;
        [ProtoMember(4)] [XmlElement("ExpirationTime")] public int _defenseExpireTime;
        [ProtoMember(5)] [XmlElement("MinThreat")] public int _minDefenseThreat;
    }*/
}
