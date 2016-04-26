using UnityEngine;
using System.Collections;

namespace Utils
{

    public class MultiDisplay : MonoBehaviour {

        void Start () {
            var displays = Display.displays;
            for(int i = 0, n = displays.Length; i < n; i++)
            {
                displays[i].Activate();
            }
        }
        
    }

}


