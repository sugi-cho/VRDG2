using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;

namespace irishoak {
	
	public class CameraTransformOscReceiver : MonoBehaviour {

		public int listenPort = 12000;
		UdpClient  udpClient;
		IPEndPoint endPoint;
		Osc.Parser osc = new Osc.Parser ();

		Vector3    pos;
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

		void Start () {

			endPoint = new IPEndPoint (IPAddress.Any, listenPort);
			udpClient = new UdpClient (endPoint);

		}

		void Update () {

			while (udpClient.Available > 0) {
				osc.FeedData (udpClient.Receive (ref endPoint));
			}

			while (osc.MessageCount > 0) {
				var msg = osc.PopMessage ();

				if (msg.path == "/camera/position") {
					if (msg.data.Length > 2) {
						pos = new Vector3 (
							(float) msg.data [0] * (InvertSignPosX ? -1.0f : 1.0f),
							(float) msg.data [1] * (InvertSignPosY ? -1.0f : 1.0f),
							(float) msg.data [2] * (InvertSignPosZ ? -1.0f : 1.0f)
						) * Scale;
					}
				}

				if (msg.path == "/camera/quaternion") {
					if (msg.data.Length > 3) {
						rot = new Quaternion (
							(float) msg.data [0] * (InvertSignRotX ? -1.0f : 1.0f),
							(float) msg.data [1] * (InvertSignRotY ? -1.0f : 1.0f),
							(float) msg.data [2] * (InvertSignRotZ ? -1.0f : 1.0f),
							(float) msg.data [3] * (InvertSignRotW ? -1.0f : 1.0f)
						);
					}
				}

				mess = msg.ToString ();
			}

			transform.localPosition = pos;
			transform.localRotation = rot;
		}
	}
}