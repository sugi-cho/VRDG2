using UnityEngine;
using System.Collections;

namespace mattatz
{

    public class BuoyancyUpdater : ParticleUpdater {

        public float buoyancy = 4f;

        protected override void Update()
        {
            base.Update();
            material.SetFloat("_Buoyancy", buoyancy);
        }

    }

}


