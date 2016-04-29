using UnityEngine;
using UnityEditor;

using System.Collections;

namespace mattatz {

    [CustomEditor(typeof(BoxController))]
    public class BoxControllerEditor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(GUILayout.Button("Begin")) {
                var controller = target as BoxController;
                controller.Begin();
            }

        }

    }

}



