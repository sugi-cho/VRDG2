using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;

namespace irishoak {
	
	public class IrisSceneControllerOscReceiver : MonoBehaviour {

		public int listenPort = 12000;
		UdpClient  udpClient;
		IPEndPoint endPoint;
		Osc.Parser osc = new Osc.Parser ();

		public IrisSceneController    IrisSceneControllerScript;
		public TitleCharController    TitleCharControllerScript;
		public GroundCircleController GroundCircleControllerScript;

		public String mess;

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

				if (msg.path == "/pulseField/pulseRadius") {
					IrisSceneControllerScript.PulseRadius = (float) msg.data [0];
				}

				if (msg.path == "/pulseField/pulseFieldHeight") {
					IrisSceneControllerScript.PulseFieldHeight = (float) msg.data [0];
				}

				if (msg.path == "/pulseField/pulseFieldScale") {
					IrisSceneControllerScript.PulseFieldScale = (float) msg.data [0];
				}

				if (msg.path == "/pulseField/pulseColor") {
					if (msg.data.Length > 3) {
						IrisSceneControllerScript.PulseColor = new Color (
							(float) msg.data [0],
							(float) msg.data [1],
							(float) msg.data [2],
							(float) msg.data [3]
						);
					}
				}

				if (msg.path == "/title/show") {
					TitleCharControllerScript.Show ();
				}

				if (msg.path == "/title/scatter") {
					TitleCharControllerScript.Scatter ();
				}

				if (msg.path == "/title/reset") {
					TitleCharControllerScript.Reset ();
				}

				if (msg.path == "/title/enable") {
					IrisSceneControllerScript.EnableTitle ();
				}

				if (msg.path == "/title/disable") {
					IrisSceneControllerScript.DisableTitle ();
				}

				if (msg.path == "/groundCircle/show") {
					GroundCircleControllerScript.Show ();
				}

				if (msg.path == "/groundCircle/hide") {
					GroundCircleControllerScript.Hide ();
				}

				if (msg.path == "/groundCircle/emissionRingPingPong") {
					GroundCircleControllerScript.RotationPingPong ^= true;
				}

			
				mess = msg.ToString ();
			}

		}
	}
}