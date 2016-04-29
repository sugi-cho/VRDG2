using UnityEngine;
using System;
using System.Collections;
using OscJack;

public class CameraTransformOscReceiver2 : MonoBehaviour
{

    Vector3 pos;
    Quaternion rot;

    public bool InvertSignPosX = false;
    public bool InvertSignPosY = false;
    public bool InvertSignPosZ = true;
    public bool InvertSignRotX = false;
    public bool InvertSignRotY = false;
    public bool InvertSignRotZ = true;
    public bool InvertSignRotW = true;

    public float Scale = 0.1f;

    String mess = "";

    void Update()
    {

        var position = OscMaster.GetData("/camera/position");
        if (position != null)
        {
            if (position.Length > 2)
            {
                pos = new Vector3(
                    (float)position[0] * (InvertSignPosX ? -1.0f : 1.0f),
                    (float)position[1] * (InvertSignPosY ? -1.0f : 1.0f),
                    (float)position[2] * (InvertSignPosZ ? -1.0f : 1.0f)
                ) * Scale;
            }
            OscMaster.ClearData("/camera/position");
        }

        var rotation = OscMaster.GetData("/camera/quaternion");
        if (rotation != null)
        {
            if (rotation.Length > 3)
            {
                rot = new Quaternion(
                    (float)rotation[0] * (InvertSignRotX ? -1.0f : 1.0f),
                    (float)rotation[1] * (InvertSignRotY ? -1.0f : 1.0f),
                    (float)rotation[2] * (InvertSignRotZ ? -1.0f : 1.0f),
                    (float)rotation[3] * (InvertSignRotW ? -1.0f : 1.0f)
                );
            }
            OscMaster.ClearData("/camera/quaternion");
        }

        transform.localPosition = pos;
        transform.localRotation = rot;

        //OscMessageDebugger.oscMessage = mess;
    }
}
