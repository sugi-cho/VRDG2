using UnityEngine;
using System.Collections;

namespace irishoak {

	[ExecuteInEditMode]
	public class IrisSceneController : MonoBehaviour {

		public GameObject TitleRef;

		public GameObject PulseFieldTopRef;
		public GameObject PulseFieldBottomRef;

		public GameObject GroundCircleTopRef;
		public GameObject GroundCircleBottomRef;

		public Material PulseFieldMat;
		public Material EmissionRingMat;

		[Range (0, 1)]
		public float PulseRadius = 0.0f;

		public Color PulseColor = new Color (0.2f, 0.8f, 0.5f, 1.0f);

		public float PulseFieldHeight = 2.0f;
		public float PulseFieldScale  = 1.0f;

		void Start () {
		
		}
		
		void Update () {

			PulseFieldTopRef.transform.localPosition    = new Vector3 (0,  PulseFieldHeight, 0);
			PulseFieldBottomRef.transform.localPosition = new Vector3 (0, -PulseFieldHeight, 0);

			GroundCircleTopRef.transform.localPosition    = new Vector3 (0,  PulseFieldHeight - 0.05f, 0);
			GroundCircleBottomRef.transform.localPosition = new Vector3 (0, -PulseFieldHeight + 0.05f, 0);

			PulseFieldTopRef.transform.localScale    = Vector3.one * PulseFieldScale;
			PulseFieldBottomRef.transform.localScale = Vector3.one * PulseFieldScale;

			var pulseRadius = Map (PulseRadius, 0.0f, 1.0f, -0.05f, 0.65f);
			PulseFieldMat.SetFloat ("_PulseRadius", pulseRadius);
			PulseFieldMat.SetColor ("_Color", PulseColor);
			EmissionRingMat.SetColor ("_Color", PulseColor);
			EmissionRingMat.SetColor ("_EmissionColor", PulseColor);

		}

		public void EnableTitle () {
			TitleRef.SetActive (true);
		}

		public void DisableTitle () {
			TitleRef.SetActive (false);
		}

		public float Map (float x, float in_min, float in_max, float out_min, float out_max)
		{
			return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
		}
	}
}