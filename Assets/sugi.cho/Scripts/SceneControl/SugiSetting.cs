using UnityEngine;
using System.Collections;
namespace sugi.cc
{
    public class SugiSetting : MonoBehaviour
    {
        static SugiSetting Instance { get { if (_Instance == null) _Instance = FindObjectOfType<SugiSetting>(); return _Instance; } }
        static SugiSetting _Instance;

        //Jsonファイル保存の時に、ComputeBufferのデータが消えるから、設定したら、再起動
        static string FilePath = "sugi.cho/setting.json";
        [SerializeField]
        Setting setting;
        public ComputeShader compute;
        // Use this for initialization
        void Start()
        {
            SettingManager.AddSettingMenu(setting, FilePath);
        }

        [System.Serializable]
        public class Setting : SettingManager.Setting
        {
            public Vector3 worldOffset;
            public float worldScale = 1.0f;
            public float triangleSize = 1.0f;
            public float objectSize = 1.0f;
            public float toriSize = 1.0f;


            public override void OnGUIFunc()
            {
                base.OnGUIFunc();
                Shader.SetGlobalVector("_WOffset", worldOffset);
                Shader.SetGlobalFloat("_WScale", worldScale);
                Instance.compute.SetFloat("_TriSize", triangleSize);
                Instance.compute.SetFloat("_ObjSize", objectSize);
                Instance.compute.SetFloat("_ToriSize", toriSize);
            }
            protected override void OnLoad()
            {
                base.OnLoad();
                Shader.SetGlobalVector("_WOffset", worldOffset);
                Shader.SetGlobalFloat("_WScale", worldScale);
                Instance.compute.SetFloat("_TriSize", triangleSize);
                Instance.compute.SetFloat("_ObjSize", objectSize);
                Instance.compute.SetFloat("_ToriSize", toriSize);
            }
        }
    }
}