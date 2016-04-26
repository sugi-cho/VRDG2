using UnityEngine;
using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;

using UnityOSC;

namespace mattatz {

    [System.Serializable] public class KorgKontrol2FunctionEvent : UnityEvent<string> { }
    [System.Serializable] public class KorgKontrol2DialEvent : UnityEvent<int, float> { }
    [System.Serializable] public class KorgKontrol2SliderEvent : UnityEvent<int, float> { }

    [System.Serializable] public enum KorgKontrol2ButtonPosition { Top, Middle, Bottom }
    [System.Serializable] public class KorgKontrol2ButtonEvent : UnityEvent<KorgKontrol2ButtonPosition, int, bool> { }
    [System.Serializable] public class KorgKontrol2UnitEvent : UnityEvent<KorgKontrol2Unit> { }

    [System.Serializable]
    public class KorgKontrol2Unit {
        public int index;
        public bool topButton, middleButton, bottomButton;
        public float dial;
        public float slider;

        public KorgKontrol2Unit(int index) {
            this.index = index;
        }
    }

    public class KorgKontrol2Controller : MonoBehaviour {

        const string _ServerID = "_mattatz_OSC_Test";

        [SerializeField] int port = 8888;

        [SerializeField] KorgKontrol2FunctionEvent functionEvent;
        [SerializeField] KorgKontrol2DialEvent dialEvent;
        [SerializeField] KorgKontrol2SliderEvent sliderEvent;
        [SerializeField] KorgKontrol2ButtonEvent buttonEvent;
        [SerializeField] KorgKontrol2UnitEvent unitEvent;

        Queue queue;

        const int _KorgKontrol2_Unit_Count = 8;
        KorgKontrol2Unit[] units;

        void Start() {
            units = new KorgKontrol2Unit[_KorgKontrol2_Unit_Count];
            for(int i = 0; i < _KorgKontrol2_Unit_Count; i++) {
                units[i] = new KorgKontrol2Unit(i);
            }

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
            var len = address.Length;
            var part = address[2];

            // Debug.Log(msg.Address);

            if(part == "function") {
                var key = address[3];
                Function(key);
            } else {
                var index = int.Parse(address[len - 1]) - 1; // index start from 0
                if(len == 4) {
                    var value = float.Parse(msg.Data[0].ToString());
                    if (part == "dial") Dial(index, value);
                    else if (part == "slider") Slider(index, value);
                } else {
                    var place = address[3];
                    var position = (place == "top") ? KorgKontrol2ButtonPosition.Top : ((place == "middle") ? KorgKontrol2ButtonPosition.Middle : KorgKontrol2ButtonPosition.Bottom);
                    var value = int.Parse(msg.Data[0].ToString());
                    Button(position, index, value == 1);
                }
            }
        }

        void Function (string key) {
            functionEvent.Invoke(key);
        }

        void Dial (int index, float value) {
            if (index < units.Length) {
                units[index].dial = value;
                unitEvent.Invoke(units[index]);
            }

            dialEvent.Invoke(index, value);
        }

        void Slider (int index, float value) {
            if (index < units.Length) {
                units[index].slider = value;
                unitEvent.Invoke(units[index]);
            }

            sliderEvent.Invoke(index, value);
        }

        // place : 0, 1, 2
        void Button (KorgKontrol2ButtonPosition position, int index, bool value) {
            if(index < units.Length) {
                switch(position) {
                    case KorgKontrol2ButtonPosition.Top:
                        units[index].topButton = value;
                        break;
                    case KorgKontrol2ButtonPosition.Middle:
                        units[index].middleButton = value;
                        break;
                    case KorgKontrol2ButtonPosition.Bottom:
                        units[index].bottomButton = value;
                        break;
                }
                unitEvent.Invoke(units[index]);
            }

            buttonEvent.Invoke(position, index, value);
        }

        void OnPacketReceived(OSCServer server, OSCPacket packet) {
            queue.Enqueue(packet);
        }

    }

}


