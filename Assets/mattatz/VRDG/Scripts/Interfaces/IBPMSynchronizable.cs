using UnityEngine;
using System.Collections;

namespace mattatz {

    public interface IBPMSynchronizable {
        void OnClick(int bpm, int samples);
    }

}


