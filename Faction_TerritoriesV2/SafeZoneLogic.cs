﻿using ObjectBuilders.SafeZone;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Faction_TerritoriesV2
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_SafeZoneBlock), false, "SafeZoneBlock")]
    public class SafeZoneLogic : MyGameLogicComponent
    {
        public IMySafeZoneBlock zoneBlock;
        private bool isServer;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            isServer = MyAPIGateway.Session.IsServer;
            zoneBlock = Entity as IMySafeZoneBlock;
            if (zoneBlock == null) return;
            if (zoneBlock?.CubeGrid?.Physics == null) return;

            Events.SafeZoneBlockSetup(zoneBlock);
        }

        public override void OnRemovedFromScene()
        {
            if (isServer)
            {
                Session.Instance.safeZoneBlocks.Remove(zoneBlock.EntityId);
                //MyVisualScriptLogicProvider.ShowNotification($"SafeZone block Removed", 20000);
            }
        }
    }
}
