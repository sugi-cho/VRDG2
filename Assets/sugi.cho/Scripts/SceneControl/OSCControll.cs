using UnityEngine;
using System.Collections;
using System.Linq;
//using OscJack;

namespace sugi.cc
{
    public class OSCControll : MonoBehaviour
    {
        //    int numDials = 8;
        //    int numSliders = 8;

        //    [SerializeField]
        //    int totalMessages;

        //    [SerializeField]
        //    float[] dials;
        //    [SerializeField]
        //    float[] sliders;

        //    ComputeBuffer dialBuffer;
        //    ComputeBuffer sliderBuffer;

        //    // Use this for initialization
        //    void Start()
        //    {
        //        dials = new float[8];
        //        sliders = new float[8];

        //        dialBuffer = Helper.CreateComputeBuffer(dials);
        //        sliderBuffer = Helper.CreateComputeBuffer(sliders);
        //    }

        //    void OnDestroy()
        //    {
        //        dialBuffer.Release();
        //        sliderBuffer.Release();
        //    }

        //    // Update is called once per frame
        //    void Update()
        //    {
        //        totalMessages = OscMaster.MasterDirectory.TotalMessageCount;
        //        for (var i = 1; i <= numDials; i++)
        //        {
        //            var address = "/osc/dial/" + i.ToString();
        //            if (OscMaster.HasData(address))
        //            {
        //                var val = (float)OscMaster.GetData(address).LastOrDefault();
        //                dials[i - 1] = val;
        //                OscMaster.ClearData(address);
        //            }
        //        }
        //        dialBuffer.SetData(dials);
        //        for (var i = 1; i <= numSliders; i++)
        //        {
        //            var address = "/osc/slider/" + i.ToString();
        //            if (OscMaster.HasData(address))
        //            {
        //                var val = (float)OscMaster.GetData(address).LastOrDefault();
        //                sliders[i - 1] = val;
        //                OscMaster.ClearData(address);
        //            }
        //        }
        //        sliderBuffer.SetData(sliders);
        //    }
    }
}