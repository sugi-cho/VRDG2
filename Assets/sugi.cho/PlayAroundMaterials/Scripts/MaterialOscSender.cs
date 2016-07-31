using UnityEngine;
using System.Collections;
using Osc;

namespace sugi.cc
{
    public class MaterialOscSender : MonoBehaviour
    {
        public int sceneIdx = 0;
        public MidiAction[] knobActions = new MidiAction[8];
        public MidiAction[] sliderActions = new MidiAction[8];

        void Start()
        {
            var controllEvents = MidiControl.Instance.midiEvents[sceneIdx];
            for (var i = 0; i < knobActions.Length && i < controllEvents.onKnobUpdate.Length; i++)
            {
                var knobAction = knobActions[i];
                controllEvents.onKnobUpdate[i].AddListener(knobAction.SendOsc);
                knobAction.SendOsc(0);
            }
            for (var i = 0; i < sliderActions.Length && i < controllEvents.onSliderUpdate.Length; i++)
            {
                var sliderAction = sliderActions[i];
                controllEvents.onSliderUpdate[i].AddListener(sliderAction.SendOsc);
                sliderAction.SendOsc(0);
            }
        }

        public enum propType
        {
            floatProp = 0,
            colorProp = 1,
            vectorProp = 2,
        }

        [System.Serializable]
        public class MidiAction
        {
            public Material targetMat;
            public string propName = "_Color";
            public propType type = propType.floatProp;
            public float minVal = 0f;
            public float maxVal = 1f;
            public int index = 0;

            public void SendOsc(float val)
            {
                val = Mathf.Lerp(minVal, maxVal, val);
                var address = "/material";
                if (type == propType.floatProp)
                {
                    address += "/float";
                    var osc = new MessageEncoder(address);
                    osc.Add(targetMat.name);
                    osc.Add(propName);
                    osc.Add(val);
                    OscController.Instance.Send(osc);
                    targetMat.SetFloat(propName, val);
                }
                else if (type == propType.colorProp)
                {
                    address += "/color";
                    var osc = new MessageEncoder(address);
                    var color = targetMat.GetColor(propName);
                    color[index] = val;
                    osc.Add(targetMat.name);
                    osc.Add(propName);
                    osc.Add(color[0]);
                    osc.Add(color[1]);
                    osc.Add(color[2]);
                    osc.Add(color[3]);
                    OscController.Instance.Send(osc);
                    targetMat.SetColor(propName, color);
                }
                else
                {
                    address += "/vector";
                    var osc = new MessageEncoder(address);
                    var vec4 = targetMat.GetVector(propName);
                    vec4[index] = val;
                    osc.Add(targetMat.name);
                    osc.Add(propName);
                    osc.Add(vec4[0]);
                    osc.Add(vec4[1]);
                    osc.Add(vec4[2]);
                    osc.Add(vec4[3]);
                    OscController.Instance.Send(osc);
                    targetMat.SetVector(propName, vec4);
                }
            }
        }
    }
}