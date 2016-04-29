using UnityEngine;
using UnityEditor;

using System.Collections;

namespace mattatz {

    [CustomEditor(typeof(CPExplodeUpdater))]
    public class CPExplodeUpdaterEditor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(GUILayout.Button("Explode")) {
                var controller = target as CPExplodeUpdater;
                controller.Explode();
            }

        }

    }

}



