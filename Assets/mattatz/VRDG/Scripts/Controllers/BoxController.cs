using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using UnityOSC;

namespace mattatz {

    public class BoxController : MonoBehaviour {

        [SerializeField] GPUComputeParticleSystem system;
        // [SerializeField] List<CPParticleUpdaterControl> controls;

        void Start () {
        }

        public void OnTrigger (OSCUnit unit) {
            if (unit.index >= system.updaters.Count) return;

            var updater = system.updaters[unit.index];

            if(unit.buttons[0] != updater.gameObject.activeSelf) {
                updater.gameObject.SetActive(unit.buttons[0]);
            }

            if(unit.buttons[0]) {
                updater.OnTrigger(unit);
            }
        }

        public void OnControl(OSCUnit unit) {
            if (unit.index >= system.updaters.Count) return;

            var updater = system.updaters[unit.index];
            updater.OnControl(unit);
        }

    }

}


