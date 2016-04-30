using UnityEngine;
using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;

using UnityOSC;

namespace mattatz {

    public interface IOSCUnitResponsible {
        void OnTrigger(OSCUnit unit);
        void OnControl(OSCUnit unit);
    }

    [System.Serializable]
    public class OSCUnit {
        public int index;
        public bool[] buttons;
        public float[] sliders;
        public OSCUnit (int index, int buttonCount, int sliderCount) {
            this.index = index;
            buttons = new bool[buttonCount];
            sliders = new float[sliderCount];
        }
    }

    [System.Serializable] public class OSCUnitTriggerEvent : UnityEvent<OSCUnit> { }
    [System.Serializable] public class OSCUnitControlEvent : UnityEvent<OSCUnit> { }
    [System.Serializable] public class OSCCoreColorEvent : UnityEvent<Color> { }
    [System.Serializable] public class OSCCoreDistanceEvent : UnityEvent<float> { }

    public class OSCController : MonoBehaviour {

        const string _ServerID = "_OSC_mattatz_";
        const int _ButtonCount = 4;
        const int _SliderCount = 4;

        [SerializeField] int port = 8888;

        [SerializeField] UnityEvent onBegin;
        [SerializeField] UnityEvent onReplay;
        [SerializeField] UnityEvent onStart;
        [SerializeField] OSCUnitTriggerEvent onTrigger;
        [SerializeField] OSCUnitControlEvent onControl;
        [SerializeField] OSCCoreColorEvent onCoreColor;
        [SerializeField] OSCCoreDistanceEvent onCoreDistance;

        Queue queue;

        Dictionary<int, OSCUnit> units;

        void Start() {
            units = new Dictionary<int, OSCUnit>();

            queue = new Queue();
            queue = Queue.Synchronized(queue);
            OSCHandler.Instance.CreateServer(_ServerID, port);
            OSCHandler.Instance.PacketReceivedEvent += OnPacketReceived;
        }

        void Update() {
            while (0 < queue.Count) {
                OSCPacket packet = queue.Dequeue() as OSCPacket;
                if (packet.IsBundle()) {
                    OSCBundle bundle = packet as OSCBundle;
                    foreach (OSCMessage msg in bundle.Data) {
                        Receive(msg);
                    }
                } else {
                    OSCMessage msg = packet as OSCMessage;
                    Receive(msg);
                }
            }
        }

        void Receive(OSCMessage msg) {
            var address = msg.Address.Split('/');
            if (address[1] == "core") {
                if(address[2] == "color") {
                    float r = float.Parse(msg.Data[0].ToString());
                    float g = float.Parse(msg.Data[1].ToString());
                    float b = float.Parse(msg.Data[2].ToString());
                    onCoreColor.Invoke(new Color(r, g, b));
                } else if(address[2] == "distance") {
                    float distance = float.Parse(msg.Data[0].ToString());
                    onCoreDistance.Invoke(distance);
                }
            } else if (address[1] == "mattatz") {

                if (address[2] == "begin") {
                    onBegin.Invoke();
                    return;
                } else if(address[2] == "replay") {
                    for(int i = 0, n = onReplay.GetPersistentEventCount(); i < n; i++) {
                        var behaviour = onReplay.GetPersistentTarget(i) as MonoBehaviour;
                        behaviour.gameObject.SetActive(true);
                    }
                    onReplay.Invoke();
                    return;
                } else if(address[2] == "start") {
                    onStart.Invoke();
                    return;
                }

                int index = int.Parse(msg.Data[0].ToString());
                if (!units.ContainsKey(index)) {
                    units[index] = new OSCUnit(index, _ButtonCount, _SliderCount);
                }
                var unit = units[index];
                if(address[2] == "trigger") {
                    for(int i = 0; i < _ButtonCount; i++) {
                        unit.buttons[i] = int.Parse(msg.Data[i + 1].ToString()) == 1;
                    }
                    onTrigger.Invoke(unit);
                } else if(address[2] == "control") {
                    for(int i = 0; i < _SliderCount; i++) {
                        unit.sliders[i] = float.Parse(msg.Data[i + 1].ToString());
                    }
                    onControl.Invoke(unit);
                }
            }

        }

        void OnPacketReceived(OSCServer server, OSCPacket packet) {
            queue.Enqueue(packet);
        }

    }

}


