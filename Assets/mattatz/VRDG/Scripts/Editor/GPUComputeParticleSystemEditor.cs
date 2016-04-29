﻿using UnityEngine;
using UnityEditor;

using System.Collections;

namespace mattatz {

    [CustomEditor (typeof(GPUComputeParticleSystem))]
    public class GPUComputeParticleSystemEditor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            GPUComputeParticleSystem system = target as GPUComputeParticleSystem;
            // system.updaters;

            EditorGUILayout.LabelField("updaters");
            // EditorGUILayout.BeginHorizontal();

            EditorGUI.indentLevel++;

            int n = system.updaters.Count;
            for(int i = 0; i < n; i++) {
                var updater = system.updaters[i];
                if(updater == null) {
                    system.updaters.RemoveAt(i);
                    return;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(updater.name);
                if(i != 0 && GUILayout.Button("up", GUILayout.Width(50), GUILayout.Height(20))) {
                    var tmp = system.updaters[i];
                    system.updaters[i] = system.updaters[i - 1];
                    system.updaters[i - 1] = tmp;
                    break;
                } else if(i != n - 1 && GUILayout.Button("down", GUILayout.Width(50), GUILayout.Height(20))) {
                    var tmp = system.updaters[i];
                    system.updaters[i] = system.updaters[i + 1];
                    system.updaters[i + 1] = tmp;
                    break;
                } else if(GUILayout.Button("Remove", GUILayout.Width(50), GUILayout.Height(20))) {
                    system.updaters.RemoveAt(i);
                    break;
                }
                GUILayout.EndHorizontal();
            }

            // EditorGUILayout.EndHorizontal();

        }

    }

}


