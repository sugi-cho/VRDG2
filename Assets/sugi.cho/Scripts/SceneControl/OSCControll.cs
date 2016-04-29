using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Linq;
using OscJack;
using mattatz;

namespace sugi.cc
{
    public class OSCControll : MonoBehaviour
    {
        public static OSCControll Instance { get { if (_Instance == null) _Instance = FindObjectOfType<OSCControll>(); return _Instance; } }
        private static OSCControll _Instance;

        public Material visualizer;
        public ComputeShader compute;
        int numDials = 8;
        int numSliders = 8;

        [SerializeField]
        int totalMessages;

        float[] dials;
        float[] sliders;

        ComputeBuffer dialBuffer;
        ComputeBuffer sliderBuffer;

        public UnityEvent onStartTrack2;
        public FloatEvent[] onUpdateDial;
        public FloatEvent[] onUpdateSlider;
        public ColorEvent onCoreColor;

        public CPExtrudeUpdater cpUpdater;

        //    // Use this for initialization
        void Start()
        {
            dials = new float[8];
            sliders = new float[8];

            dialBuffer = Helper.CreateComputeBuffer(dials);
            sliderBuffer = Helper.CreateComputeBuffer(sliders);

            visualizer.SetBuffer("_Dial", dialBuffer);
            visualizer.SetBuffer("_Slider", sliderBuffer);
        }

        void OnDestroy()
        {
            dialBuffer.Release();
            sliderBuffer.Release();
        }

        //    // Update is called once per frame
        void Update()
        {
            totalMessages = OscMaster.MasterDirectory.TotalMessageCount;

            if (OscMaster.HasData("/track2") || Input.GetKeyDown(KeyCode.Space))
            {
                onStartTrack2.Invoke();
                OscMaster.ClearData("/track2");
            }

            for (var i = 1; i <= numDials; i++)
            {
                var address = "/osc/dial/" + i.ToString();
                if (OscMaster.HasData(address))
                {
                    var val = (float)OscMaster.GetData(address).LastOrDefault();
                    dials[i - 1] = val;
                    onUpdateDial[i - 1].Invoke(val);
                    OscMaster.ClearData(address);
                }
            }
            dialBuffer.SetData(dials);
            for (var i = 1; i <= numSliders; i++)
            {
                var address = "/osc/slider/" + i.ToString();
                if (OscMaster.HasData(address))
                {
                    var val = (float)OscMaster.GetData(address).LastOrDefault();
                    sliders[i - 1] = val;
                    onUpdateSlider[i - 1].Invoke(val);
                    OscMaster.ClearData(address);
                }
            }
            compute.SetFloats("_Dial", dials);
            compute.SetFloats("_Slider", sliders);

            if (OscMaster.HasData("/light/color"))
            {
                var data = OscMaster.GetData("/light/color");
                if (data.Length > 3)
                {
                    var color = new Color((float)data[0], (float)data[1], (float)data[2], (float)data[3]);
                    Shader.SetGlobalColor("_LightColor", color);
                }
                OscMaster.ClearData("/light/color");
            }
            if (OscMaster.HasData("/light/intensity"))
            {
                var val = (float)OscMaster.GetData("/light/intensity").FirstOrDefault();
                Shader.SetGlobalFloat("_LightIntensity", val);
                OscMaster.ClearData("/light/intensity");
            }
            if (OscMaster.HasData("/core/color"))
            {
                var data = OscMaster.GetData("/core/color");
                var color = new Color((float)data[0], (float)data[1], (float)data[2], (float)data[3]);
                onCoreColor.Invoke(color);
                OscMaster.ClearData("/core/color");
            }
            sliderBuffer.SetData(sliders);

            if (OscMaster.HasData("/beat"))
            {
                if (cpUpdater.gameObject.activeSelf)
                    cpUpdater.OnClick();
                OscMaster.ClearData("/beat");
            }
        }
    }
}