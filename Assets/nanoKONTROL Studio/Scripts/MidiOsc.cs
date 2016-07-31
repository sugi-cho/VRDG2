using UnityEngine;
using sugi.cc;
using System.Net;

public class MidiOsc : MonoBehaviour
{
    public int midiSceneIdx = 0;
    public string remoteHost = "localhost";
    public int remotePort;

    IPEndPoint remote { get { if (_remote == null) _remote = new IPEndPoint(OscController.Instance.FindFromHostName(remoteHost), remotePort); return _remote; } }
    IPEndPoint _remote;

    const string sliderOscPrefix = "/osc/slider/";
    const string dialOscPrefix = "/osc/dial/";

    void Start()
    {
        var midi = MidiControl.Instance;
        if (midi == null) return;
        var controllEvents = midi.midiEvents[midiSceneIdx];
        for (var i = 0; i < 8; i++)
        {
            var onKnob = new MidiControllAction();
            onKnob.address = dialOscPrefix + (i + 1).ToString();
            onKnob.remote = remote;
            controllEvents.onKnobUpdate[i].AddListener(onKnob.OnUpdate);
            var onSlider = new MidiControllAction();
            onSlider.address = sliderOscPrefix + (i + 1).ToString();
            onSlider.remote = remote;
            controllEvents.onSliderUpdate[i].AddListener(onSlider.OnUpdate);
        }
    }
    void Test(float t)
    {
        print(t);
    }
    [SerializeField]
    public class MidiControllAction
    {
        public string address;
        public IPEndPoint remote;
        public void OnUpdate(float val)
        {
            var osc = new MessageEncoder(address);
            osc.Add(val);
            OscController.Instance.Send(osc, remote);
        }
    }
}
