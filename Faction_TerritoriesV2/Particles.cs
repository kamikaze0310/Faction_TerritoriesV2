using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.ModAPI;
using VRageMath;

namespace Faction_TerritoriesV2
{
    /*public class Particles
    {
        public string _subtype;
        public bool _tiedToEntity;
        public IMyEntity _entity;
        public MyParticleEffect _particle;

        public Particles() { }

        public Particles(string subtype, bool tiedToEntity, IMyEntity entity, MyParticleEffect particle)
        {
            _subtype = subtype;
            _tiedToEntity = tiedToEntity;
            _entity = entity;
            _particle = particle;

            _particle.OnDelete += OnParticleDelete;
        }

        public void OnParticleDelete(MyParticleEffect effect)
        {
            MyVisualScriptLogicProvider.ShowNotification("On Particle Delete Event", 8000);
            effect.OnDelete -= OnParticleDelete;
            if (_tiedToEntity)
            {
                if (_entity != null && !_entity.MarkedForClose)
                {
                    MyParticleEffect _effect;
                    MatrixD matrix = effect.WorldMatrix;
                    Vector3D pos = _entity.GetPosition();
                    MyParticlesManager.TryCreateParticleEffect(effect.GetName(), ref matrix, ref pos, uint.MaxValue, out _effect);

                    _particle = _effect;
                    _particle.OnDelete += OnParticleDelete;
                    MyVisualScriptLogicProvider.ShowNotification("Started new particle", 1000);
                }
            }
        }
    }*/
}
