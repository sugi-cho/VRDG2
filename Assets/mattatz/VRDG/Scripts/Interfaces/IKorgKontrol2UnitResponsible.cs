using UnityEngine;
using System.Collections;

namespace mattatz {

    public interface IKorgKontrol2UnitResponsible {
        void OnDial(int index, float value);
        void OnSlider(int index, float value);
        void OnButton(KorgKontrol2ButtonPosition p, int index, bool value);
        void OnUnit(KorgKontrol2Unit unit);
    }

}


