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

    public class OSCController : MonoBehaviour {

        const string _ServerID = "_OSC_mattatz_";
        const int _ButtonCount = 4;
        const int _SliderCount = 4;

        [SerializeField] int port = 8888;

        [SerializeField] OSCUnitTriggerEvent onTrigger;
        [SerializeField] OSCUnitControlEvent onControl;

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
            if (address[1] == "mattatz") {

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


