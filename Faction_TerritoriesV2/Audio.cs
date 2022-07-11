using ProtoBuf;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Faction_TerritoriesV2
{
    public enum AudioClips
    {
        SafeZoneConstructed,
        RadarConstructed,
        ProductionConstructed,
        DroneConstructed,
        UnderAttack,
        WarningIntegrity,
        RealHudUnable,
        EnemyDetected
    }

    public enum AudioType
    {
        Character,
        Block
    }

    [ProtoContract]
    public class AudioPackage
    {
        [ProtoMember(1)] public AudioClips clip;
        [ProtoMember(2)] public InstallationType installationType;
        [ProtoMember(3)] public long playerId;
        [ProtoMember(4)] public AudioType audioType;
        [ProtoIgnore] public IMyPlayer player;

        public AudioPackage() { }

        public AudioPackage(AudioClips Clip, InstallationType Installation, long PlayerId, AudioType Audio, IMyPlayer Player)
        {
            clip = Clip;
            installationType = Installation;
            playerId = PlayerId;
            audioType = Audio;
            player = Player;

            Comms.SendAudioToClient(this);
        }
    }

    public static class Audio
    {
        private static IMyEntity currentEmitter;
        private static MyEntity3DSoundEmitter emitter;
        public static int delay = 0;
        public static Queue<AudioPackage> audioQueue = new Queue<AudioPackage>();

        public static void PlayClip()
        {
            if (emitter != null && emitter.IsPlaying) return;
            if (audioQueue.Count == 0) return;
            AudioPackage audio = audioQueue.Dequeue();
            if (audio == null) return;

            if (audio.clip == AudioClips.RealHudUnable || audio.clip == AudioClips.EnemyDetected)
                if (delay > 0) return;

            MySoundPair soundPair = null;
            if (audio.audioType == AudioType.Character)
            {
                IMyEntity controlledEntity = MyAPIGateway.Session.LocalHumanPlayer?.Controller?.ControlledEntity?.Entity;
                if (controlledEntity == null) return;

                if (currentEmitter == null || currentEmitter != controlledEntity || emitter == null)
                {
                    currentEmitter = controlledEntity;
                    emitter = new MyEntity3DSoundEmitter(controlledEntity as MyEntity);
                }

                if (emitter == null) return;
                soundPair = new MySoundPair(audio.clip.ToString());
            }

            if (soundPair == null) return;
            emitter.PlaySound(soundPair);

            if (audio.clip == AudioClips.RealHudUnable)
            {
                delay = 5;
                MyVisualScriptLogicProvider.ShowNotification("No Tools Allowed Inside Claimed Territory...", 3000, "Red");
            }

            if (audio.clip == AudioClips.EnemyDetected)
                delay = 10;
        }
    }
}
