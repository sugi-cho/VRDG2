using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using sugi.cc;

public class MidiTrackPlayer : MonoBehaviour
{
    MidiControl midiControl { get { return MidiControl.Instance; } }

    string trackDataFilePath;
    public MidiTrack track;

    float startTime;
    float prevTime;
    float playLength;

    public void RecordTrack()
    {
        StopPlaying();
        StopRecording();
        midiControl.midiRecorders.Add(this);
        startTime = Time.timeSinceLevelLoad;
    }

    public void PlayTrack()
    {
        StopPlaying();
        StopRecording();
        midiControl.midiPlayers.Add(this);
        playLength = track.length;
        Debug.Log(playLength);
        startTime = Time.timeSinceLevelLoad;
    }

    public void StopPlaying()
    {
        if (midiControl.midiPlayers.Contains(this))
            midiControl.midiPlayers.Remove(this);
    }
    public void StopRecording()
    {
        if (midiControl.midiRecorders.Contains(this))
            midiControl.midiRecorders.Remove(this);
    }

    public void onRecord(MidiMessage message)
    {
        var recordTime = Time.timeSinceLevelLoad - startTime;
        track.Record(recordTime, message.originalData);
    }

    public void SaveTrack()
    {
        Helper.SaveJsonFile(track, trackDataFilePath);
    }

    public MidiMessage[] GetMessages()
    {
        var playTime = Time.timeSinceLevelLoad - startTime;
        if (playLength < playTime)
            StopPlaying();

        var messages = track.messageList.Where(b => prevTime < b.Key && b.Key < playTime)
                .OrderBy(b => b.Key).Select(b => new MidiMessage(b.Value)).ToArray();
        prevTime = playTime;
        return messages;
    }

    // Use this for initialization
    void Start()
    {
        trackDataFilePath = "MidiTrack/" + name + ".json";
        Helper.LoadJsonFile(track, trackDataFilePath);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            RecordTrack();
        if (Input.GetKeyDown(KeyCode.P))
            PlayTrack();
        if (Input.GetKeyDown(KeyCode.S))
            SaveTrack();
    }


    [System.Serializable]
    public class MidiTrack : ISerializationCallbackReceiver
    {
        public Dictionary<float, ulong> messageList;

        [SerializeField]
        private List<float> _keyList;
        [SerializeField]
        private List<ulong> _valueList;

        public float length { get { return messageList.Keys.OrderBy(b => b).LastOrDefault(); } }

        public void Record(float time, ulong data)
        {
            while (messageList.ContainsKey(time))
                time += 0.001f;
            messageList.Add(time, data);
        }

        #region http://qiita.com/toRisouP/items/53be639f267da8845a42
        public void OnBeforeSerialize()
        {
            _keyList = messageList.Keys.ToList();
            _valueList = messageList.Values.ToList();
        }

        public void OnAfterDeserialize()
        {
            messageList = _keyList.Select((id, index) =>
            {
                var value = _valueList[index];
                return new { id, value };
            }).ToDictionary(x => x.id, x => x.value);

            _keyList.Clear();
            _valueList.Clear();

        }
        #endregion
    }
}
