using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz {

    public class CPParticleUpdaterControl : MonoBehaviour, IKorgKontrol2UnitResponsible {

        [SerializeField] List<CPParticleUpdater> updaters;

        public void Init (GPUComputeParticleSystem system) {
            updaters.ForEach(updater => {
                if(updater.gameObject.activeSelf) updater.Init(system.ParticleBuffer);
            });
        }

        public void Dispatch (GPUComputeParticleSystem system) {
            updaters.ForEach(updater => {
                if(updater.gameObject.activeSelf) updater.Dispatch(system);
            });
        }

        public virtual void OnUnit (KorgKontrol2Unit unit) {
            if(unit.index < updaters.Count) {
                // updaters[unit.index].OnUnit(unit);
            }
        }

        public void OnDial(int index, float value)
        {
            throw new NotImplementedException();
        }

        public void OnSlider(int index, float value)
        {
            throw new NotImplementedException();
        }

        public void OnButton(KorgKontrol2ButtonPosition p, int index, bool value)
        {
            throw new NotImplementedException();
        }
    }

}


