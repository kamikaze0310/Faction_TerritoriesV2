using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace Faction_TerritoriesV2
{
    [ProtoContract]
    public class Installations
    {
        [ProtoMember(1)] public InstallationType? _type = null;
        [ProtoMember(2)] public long? _gridEntityId = null;
        [ProtoMember(3)] public long? _blockEntityId = null;
        [ProtoMember(4)] public TerritoryInstallations? _territoryInstallation = null;
        [ProtoMember(5)] public int? _rebuildCooldown = null;
        [ProtoMember(6)] public bool? _enabled = null;
        [ProtoMember(7)] public long? _parentId = null;
        [ProtoMember(8)] public Vector3D? _coords = null;
        [ProtoMember(9)] public int? _damageNotificationCooldown = null;
        [ProtoMember(10)] public bool? _integrityWarning = null;
        [ProtoMember(11)] public float? _integrity = null;
        [ProtoMember(12)] public long? _resourceGridId = null;
        [ProtoMember(13)] public long? _researchGridId = null;
        [ProtoMember(14)] public int? _resourceTimer = null;
        [ProtoMember(15)] public int? _researchTimer = null;
        [ProtoIgnore] public List<IMyTerminalBlock> _turrets;
        [ProtoIgnore] public MyParticleEffect _installationParticle;
        [ProtoIgnore] public volatile bool _failedToSpawn;
        [ProtoIgnore] public volatile bool _searchingTerrain;

        public Installations() { }

        public Installations(long? entityId)
        {
            _parentId = entityId;
            _turrets = new List<IMyTerminalBlock>();
        }

        public Installations(InstallationType? type, long? parentId)
        {
            _type = type;
            _parentId = parentId;
        }

        public InstallationType Type
        {
            get { return _type ?? InstallationType.SafeZone; }
            set
            {
                _type = value;
                Installations settings = new Installations()
                {
                    _type = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public long GridEntityId
        {
            get { return _gridEntityId ?? 0; }
            set
            {
                _gridEntityId = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _gridEntityId = value
                };

                MyVisualScriptLogicProvider.ShowNotificationToAll($"Installation to Sync = {this.Type}", 15000);

                Comms.SyncInstallations(settings);
            }
        }

        public long BlockEntityId
        {
            get { return _blockEntityId ?? 0; }
            set
            {
                _blockEntityId = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _blockEntityId = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public TerritoryInstallations TerritoryInstallations
        {
            get { return _territoryInstallation ?? new TerritoryInstallations(); }
            set
            {
                _territoryInstallation = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _territoryInstallation = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public int RebuildCooldown
        {
            get { return _rebuildCooldown ?? 0; }
            set
            {
                _rebuildCooldown = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _rebuildCooldown = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public bool Enabled
        {
            get { return _enabled ?? false; }
            set
            {
                _enabled = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _enabled = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public long ParentId
        {
            get { return _parentId ?? 0; }
            set
            {
                _parentId = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _parentId = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public Vector3D Coords
        {
            get { return _coords ?? new Vector3D(); }
            set
            {
                _coords = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _coords = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public int DamageNotificationCooldown
        {
            get { return _damageNotificationCooldown ?? 0; }
            set
            {
                _damageNotificationCooldown = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _damageNotificationCooldown = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public bool IntegrityWarning
        {
            get { return _integrityWarning ?? false; }  
            set
            {
                _integrityWarning = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _integrityWarning = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public float Integrity
        {
            get { return _integrity ?? 0; }
            set
            {
                _integrity = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _integrity = value
                };

                Comms.SyncInstallations(settings);

                if (MyAPIGateway.Session.IsServer)
                    Session.Instance.delayLCD = true;
            }
        }

        public long ResourceGridId
        {
            get { return _resourceGridId ?? 0; }
            set
            {
                _resourceGridId = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _resourceGridId = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public long ResearchGridId
        {
            get { return _researchGridId ?? 0; }    
            set
            {
                _researchGridId = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _researchGridId = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public int ResourceTimer
        {
            get { return _resourceTimer ?? 0; }
            set
            {
                _resourceTimer = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _resourceTimer = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public int ResearchTimer
        {
            get { return _researchTimer ?? 0; }
            set
            {
                _researchTimer = value;
                Installations settings = new Installations(Type, ParentId)
                {
                    _researchTimer = value
                };

                Comms.SyncInstallations(settings);
            }
        }

        public void AddTurret(IMyTerminalBlock gun)
        {
            if (_turrets == null)
                _turrets = new List<IMyTerminalBlock>();

            _turrets.Add(gun);
        }

        public List<IMyTerminalBlock> GetTurrets()
        {
            if (_turrets == null)
                _turrets = new List<IMyTerminalBlock>();

            return _turrets;
        }

        public static void SyncInstallations(ClaimBlockSettings settings, Installations installation)
        {
            if (settings.GetInstallationByType(installation.Type) == null) return;
            if (installation._type.HasValue)
                settings.GetInstallationByType(installation.Type)._type = installation.Type;
            if (installation._gridEntityId.HasValue)
                settings.GetInstallationByType(installation.Type)._gridEntityId = installation.GridEntityId;
            if (installation._blockEntityId.HasValue)
                settings.GetInstallationByType(installation.Type)._blockEntityId = installation.BlockEntityId;
            if (installation._territoryInstallation.HasValue)
                settings.GetInstallationByType(installation.Type)._territoryInstallation = installation.TerritoryInstallations;
            if (installation._rebuildCooldown.HasValue)
                settings.GetInstallationByType(installation.Type)._rebuildCooldown = installation.RebuildCooldown;
            if (installation._enabled.HasValue)
                settings.GetInstallationByType(installation.Type)._enabled = installation.Enabled;
            if (installation._parentId.HasValue)
                settings.GetInstallationByType(installation.Type)._parentId = installation.ParentId;
            if (installation._coords.HasValue)
                settings.GetInstallationByType(installation.Type)._coords = installation.Coords;
            if (installation._damageNotificationCooldown.HasValue)
                settings.GetInstallationByType(installation.Type)._damageNotificationCooldown = installation.DamageNotificationCooldown;
            if (installation._integrity.HasValue)
                settings.GetInstallationByType(installation.Type)._integrity = installation.Integrity;
            if (installation._resourceGridId.HasValue)
                settings.GetInstallationByType(installation.Type)._resourceGridId = installation.ResourceGridId;
            if (installation._researchGridId.HasValue)
                settings.GetInstallationByType(installation.Type)._researchGridId = installation._researchGridId;
            if (installation._resourceTimer.HasValue)
                settings.GetInstallationByType(installation.Type)._resourceTimer = installation._resourceTimer;
            if (installation._researchTimer.HasValue)
                settings.GetInstallationByType(installation.Type)._researchTimer = installation._researchTimer;
        }
    }

    public class InstallationExplosion
    {
        public IMyCubeGrid grid;
        public List<IMySlimBlock> blocks;
        public int explosions;
        public int explosionMax;
    }
}
