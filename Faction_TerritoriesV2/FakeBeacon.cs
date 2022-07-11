using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Faction_TerritoriesV2
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false, "InstallationBeacon")]
    public class FakeBeacon : MyGameLogicComponent
    {
        public IMyBeacon beacon;
        private bool isServer;
        private bool isDedicated;
        private MyResourceSinkComponent sink;
        private MyDefinitionId ElectricityId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            isServer = MyAPIGateway.Session.IsServer;
            isDedicated = MyAPIGateway.Utilities.IsDedicated;
            beacon = Entity as IMyBeacon;
            if (beacon == null) return;

            sink = beacon.ResourceSink as MyResourceSinkComponent;
            sink.SetRequiredInputByType(ElectricityId, 0);

            foreach(var claimBlock in Session.Instance.claimBlocks.Values)
            {
                Installations installation = claimBlock.GetInstallationByGridId(beacon.CubeGrid.EntityId);
                if (installation == null) continue;

                if (installation.Integrity < 25 && !isDedicated)
                {
                    if (installation._installationParticle != null)
                    {
                        installation._installationParticle.Close();
                        installation._installationParticle = null;
                    }

                    Utils.PlayParticle("ExhaustFire", beacon.GetPosition(), claimBlock, installation, 6f);
                }
            }
        }

        public override void OnRemovedFromScene()
        {

        }
    }
}
