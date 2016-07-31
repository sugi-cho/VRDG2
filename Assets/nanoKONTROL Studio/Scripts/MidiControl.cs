using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using sugi.cc;

public class MidiControl : MonoBehaviour
{
    public static MidiControl Instance { get { if (_Instance == null) _Instance = FindObjectOfType<MidiControl>(); return _Instance; } }
    static MidiControl _Instance;

    const string settingFilePath = "MIDI Controller/setting.json";

    public MidiEvent[] midiEvents = new MidiEvent[5];
    Dictionary<int, MidiEvent> statusEventsMap = new Dictionary<int, MidiEvent>();

    public FloatArrayEvent onKnobsUpdate;
    public FloatArrayEvent onSlidersUpdate;

    public BoolEvent onCycleButton;
    public BoolEvent onMarkerSetButton;
    public BoolEvent onMarkerRewButton;
    public BoolEvent onMarkerFfButton;

    public BoolEvent onRewindButton;
    public BoolEvent onForwardButton;
    public BoolEvent onTrackRewButton;
    public BoolEvent onTrackFfButton;

    public BoolEvent onReturnButton;
    public BoolEvent onStopButton;
    public BoolEvent onPlayButton;
    public BoolEvent onRecordButton;

    public FloatEvent onJogWheelUpdate;

    public KeyCode showMidiActionKey = KeyCode.M;

    public List<MidiTrackPlayer> midiPlayers;
    public List<MidiTrackPlayer> midiRecorders;

    [SerializeField]
    MidiConSetting setting;
    [SerializeField]
    float[] knob = new float[8];
    [SerializeField]
    float[] slider = new float[8];
    [SerializeField]
    float jogWheel;

    bool showMidiActions;

    MidiReceiver receiver;
    // Use this for initialization
    void Start()
    {
        SettingManager.AddSettingMenu(setting, settingFilePath);
        receiver = GetComponent<MidiReceiver>();
        for (var i = 0; i < setting.status.Length; i++)
            if (i < midiEvents.Length)
                statusEventsMap.Add(setting.status[i], midiEvents[i]);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(showMidiActionKey))
            showMidiActions = !showMidiActions;
        for (var i = 0; i < midiPlayers.Count; i++)
        {
            var player = midiPlayers[i];
            foreach (var message in player.GetMessages())
                OnGetMessage(message);
        }
        while (!receiver.IsEmpty)
            OnGetMessage(receiver.PopMessage());
        onKnobsUpdate.Invoke(knob);
        onSlidersUpdate.Invoke(slider);
    }

    void OnGetMessage(MidiMessage message)
    {
        foreach (var recorder in midiRecorders)
            recorder.onRecord(message);
        var midiEvent = statusEventsMap[message.status];
        var cc = (int)message.data1;
        var val = (int)message.data2;

        for (var i = 0; i < 8; i++)
        {
            if (cc == setting.knobCC[i])
            {
                knob[i] = val / 127f;
                midiEvent.onKnobUpdate[i].Invoke(knob[i]);
            }
            if (cc == setting.sliderCC[i])
            {
                slider[i] = val / 127f;
                midiEvent.onSliderUpdate[i].Invoke(slider[i]);
            }
            if (cc == setting.muteCC[i])
                midiEvent.onMuteButton[i].Invoke(0 < val);
            if (cc == setting.soloCC[i])
                midiEvent.onSoloButton[i].Invoke(0 < val);
            if (cc == setting.recCC[i])
                midiEvent.onRecButton[i].Invoke(0 < val);
            if (cc == setting.selectCC[i])
                midiEvent.onSelectButton[i].Invoke(0 < val);
        }

        if (cc == setting.cycleCC)
            onCycleButton.Invoke(0 < val);
        if (cc == setting.markerSetCC)
            onMarkerSetButton.Invoke(0 < val);
        if (cc == setting.markerRewCC)
            onMarkerRewButton.Invoke(0 < val);
        if (cc == setting.markerFfCC)
            onMarkerFfButton.Invoke(0 < val);

        if (cc == setting.rewind)
            onRewindButton.Invoke(0 < val);
        if (cc == setting.Forward)
            onForwardButton.Invoke(0 < val);
        if (cc == setting.trackRewCC)
            onTrackRewButton.Invoke(0 < val);
        if (cc == setting.trackFfCC)
            onTrackFfButton.Invoke(0 < val);

        if (cc == setting.returnCC)
            onReturnButton.Invoke(0 < val);
        if (cc == setting.stopCC)
            onStopButton.Invoke(0 < val);
        if (cc == setting.playCC)
            onPlayButton.Invoke(0 < val);
        if (cc == setting.recordCC)
            onRecordButton.Invoke(0 < val);

        if (cc == setting.jogWheelCC)
        {
            jogWheel = val / 127f;
            onJogWheelUpdate.Invoke(jogWheel);
        }
    }

    Rect windowRect = Rect.MinMaxRect(0, 0, Mathf.Min(Screen.width, 1920f), Mathf.Min(Screen.height, 1080f));
    Vector2 scroll;
    bool showTransport;
    void OnGUI()
    {
        if (!showMidiActions) return;
        windowRect = GUI.Window(GetInstanceID(), windowRect, OnWindow, "Midi Actions");
    }
    void OnWindow(int id)
    {
        scroll = GUILayout.BeginScrollView(scroll);
        showTransport = GUILayout.Toggle(showTransport, "Show Transport Control");
        if (showTransport)
            ShowTransportGUI();
        for (var i = 0; i < midiEvents.Length; i++)
        {
            var midiEvent = midiEvents[i];
            midiEvent.toggleGui = GUILayout.Toggle(midiEvent.toggleGui, midiEvent.controlSceneName);
            if (midiEvent.toggleGui)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8f);
                ShowMidiEventGUI(midiEvent);
                GUILayout.EndHorizontal();
            }
        }
        ShowValueArrayEvents();
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }
    void ShowTransportGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        MidiEventField("Cycle", setting.cycleCC, onCycleButton, 0.25f);
        MidiEventField("MarkerSet", setting.markerSetCC, onMarkerSetButton, 0.25f);
        MidiEventField("MarkerRew", setting.markerRewCC, onMarkerRewButton, 0.25f);
        MidiEventField("MarkerFF", setting.markerFfCC, onMarkerFfButton, 0.25f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        MidiEventField("Rewind", setting.rewind, onRewindButton, 0.25f);
        MidiEventField("Forward", setting.Forward, onForwardButton, 0.25f);
        MidiEventField("TrackRew", setting.trackRewCC, onTrackRewButton, 0.25f);
        MidiEventField("TrackFF", setting.trackFfCC, onTrackFfButton, 0.25f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        MidiEventField("ReturnZero", setting.returnCC, onReturnButton, 0.25f);
        MidiEventField("Stop", setting.stopCC, onStopButton, 0.25f);
        MidiEventField("Play", setting.playCC, onPlayButton, 0.25f);
        MidiEventField("Record", setting.recordCC, onRecordButton, 0.25f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        MidiEventField("JogWheel", setting.jogWheelCC, onJogWheelUpdate, 1f, jogWheel);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
    void ShowMidiEventGUI(MidiEvent midiEvent)
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        for (var i = 0; i < 8; i++)
            MidiEventField(string.Format("Mute[{0}]", i), setting.muteCC[i], midiEvent.onMuteButton[i]);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        for (var i = 0; i < 8; i++)
            MidiEventField(string.Format("Solo[{0}]", i), setting.soloCC[i], midiEvent.onSoloButton[i]);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        for (var i = 0; i < 8; i++)
            MidiEventField(string.Format("Rec[{0}]", i), setting.recCC[i], midiEvent.onRecButton[i]);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        for (var i = 0; i < 8; i++)
            MidiEventField(string.Format("Select[{0}]", i), setting.selectCC[i], midiEvent.onSelectButton[i]);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        for (var i = 0; i < 8; i++)
            MidiEventField(string.Format("Knob[{0}]", i), setting.knobCC[i], midiEvent.onKnobUpdate[i], 1f / 8f, knob[i]);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        for (var i = 0; i < 8; i++)
            MidiEventField(string.Format("Slider[{0}]", i), setting.sliderCC[i], midiEvent.onSliderUpdate[i], 1f / 8f, slider[i]);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
    void ShowValueArrayEvents()
    {
        GUILayout.BeginHorizontal();
        MidiEventField("Knobs", setting.knobCC[0], onKnobsUpdate, 0.5f);
        MidiEventField("Sliders", setting.sliderCC[0], onSlidersUpdate, 0.5f);
        GUILayout.EndHorizontal();
    }
    void MidiEventField<T>(string cName, int cc, UnityEvent<T> e, float rate = 1f / 8f, float sliderVal = -1f)
    {
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUILayout.Width((windowRect.width - 100f) * rate));
        GUILayout.Label(string.Format("{0},CC({1:000})", cName, cc));
        if (sliderVal != -1f)
            GUILayout.HorizontalSlider(sliderVal, 0f, 1f);
        ShowAllMethodsOfEvent(e);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
    }
    void ShowAllMethodsOfEvent<T>(UnityEvent<T> e)
    {
        var color = GUI.color;
        for (var i = 0; i < e.GetPersistentEventCount(); i++)
        {
            GUI.color = i % 2 == 0 ? Color.cyan : Color.yellow;
            GUILayout.Label(e.GetPersistentTarget(i).ToString() + "\n." + e.GetPersistentMethodName(i));
        }
        GUI.color = color;
    }


    [System.Serializable]
    public class MidiConSetting : SettingManager.Setting
    {
        public int[] status = new int[5] { 176, 177, 178, 179, 180 };
        public int[] knobCC = new int[8] { 13, 14, 15, 16, 17, 18, 19, 20 };
        public int[] sliderCC = new int[8] { 2, 3, 4, 5, 6, 8, 9, 12 };
        public int[] muteCC = new int[8] { 21, 22, 23, 24, 25, 26, 27, 28 };
        public int[] soloCC = new int[8] { 29, 30, 31, 33, 34, 35, 36, 37 };
        public int[] recCC = new int[8] { 38, 39, 40, 41, 42, 43, 44, 45 };
        public int[] selectCC = new int[8] { 46, 47, 48, 49, 50, 51, 52, 53 };

        public int cycleCC = 54;
        public int markerSetCC = 55;
        public int markerRewCC = 56;
        public int markerFfCC = 57;

        public int rewind = 58;
        public int Forward = 59;
        public int trackRewCC = 60;
        public int trackFfCC = 61;

        public int returnCC = 62;
        public int stopCC = 63;
        public int playCC = 80;
        public int recordCC = 81;

        public int jogWheelCC = 86;

        public override void OnGUIFunc()
        {
            GUILayout.BeginVertical();
            for (var i = 0; i < knobCC.Length; i++)
                knobCC[i] = IntField(string.Format("Knob[{0}] CC:", i), knobCC[i], 0, 127);
            GUILayout.Space(8f);
            for (var i = 0; i < sliderCC.Length; i++)
                sliderCC[i] = IntField(string.Format("Slider[{0}] CC:", i), sliderCC[i], 0, 127);
            GUILayout.EndVertical();
        }
    }

    #region IntField
    static float FloatField(string label, float val, float min = 0, float max = 1f)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        string s = GUILayout.TextField(val.ToString(), GUILayout.MaxWidth(100));
        float f;
        if (float.TryParse(s, out f))
            val = f;
        val = GUILayout.HorizontalSlider(val, min, max);
        GUILayout.EndHorizontal();
        val = Mathf.Clamp(val, min, max);
        return val;
    }
    static int IntField(string label, int val, int min = 0, int max = 10)
    {
        float f = FloatField(label, val, min, max);
        return Mathf.FloorToInt(f);
    }
    #endregion
    [System.Serializable]
    public class MidiEvent
    {
        public string controlSceneName = "controll events";
        public FloatEvent[] onKnobUpdate = new FloatEvent[8];
        public FloatEvent[] onSliderUpdate = new FloatEvent[8];

        public BoolEvent[] onMuteButton = new BoolEvent[8];
        public BoolEvent[] onSoloButton = new BoolEvent[8];
        public BoolEvent[] onRecButton = new BoolEvent[8];
        public BoolEvent[] onSelectButton = new BoolEvent[8];

        public bool toggleGui;
    }
}
