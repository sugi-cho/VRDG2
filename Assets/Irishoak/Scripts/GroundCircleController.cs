using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace irishoak {
	
	[ExecuteInEditMode]
	public class GroundCircleController : MonoBehaviour {

		public float RingARotation = 7.5f;

		public Transform RingBTopRef;
		public Transform RingBBottomRef;
		public Transform RingCTopRef;
		public Transform RingCBottomRef;

		public List <Transform> RingALList = new List <Transform> ();
		public List <Transform> RingARList = new List <Transform> ();

		public bool RotationPingPong = false;
		float rotation       = 0.0f;
		float targetRotation = 0.0f;

		void Start () {
		
		}
		
		void Update () {

			if (Input.GetKeyUp ("4")) {
				Show ();
			}
			if (Input.GetKeyUp ("5")) {
				Hide ();
			}

			targetRotation = RotationPingPong ? RingARotation : -RingARotation;
			rotation = Mathf.Lerp (rotation, targetRotation, Time.deltaTime * 12.0f);

			if (RingALList != null) {
				foreach (Transform t in RingALList) {
					t.localEulerAngles = new Vector3(0.0f, 0.0f,  rotation);
				}
			}

			if (RingARList != null) {
				foreach (Transform t in RingARList) {
					t.localEulerAngles = new Vector3(0.0f, 0.0f, -rotation);
				}
			}

		}

		public void Show () {

			iTween.Stop (RingCTopRef.gameObject);
			iTween.ScaleTo (
				RingCTopRef.gameObject,
				iTween.Hash (
					"x",       1.1f,
					"y",       1.1f,
					"islocal", true,
					"time",    1.25f,
					"delay",   0.0f,
					"easetype", iTween.EaseType.easeInOutSine
				)
			);

			iTween.Stop (RingCBottomRef.gameObject);
			iTween.ScaleTo (
				RingCBottomRef.gameObject,
				iTween.Hash (
					"x",       1.1f,
					"y",       1.1f,
					"islocal", true,
					"time",    1.25f,
					"delay",   0.0f,
					"easetype", iTween.EaseType.easeInOutSine
				)
			);

			iTween.Stop (RingBTopRef.gameObject);
			iTween.ScaleTo (
				RingBTopRef.gameObject,
				iTween.Hash (
					"x",       1.0f,
					"y",       1.0f,
					"islocal", true,
					"time",    1.25f,
					"delay",    0.25f,
					"easetype", iTween.EaseType.easeInOutSine
				)
			);

			iTween.Stop (RingBBottomRef.gameObject);
			iTween.ScaleTo (
				RingBBottomRef.gameObject,
				iTween.Hash (
					"x",       1.0f,
					"y",       1.0f,
					"islocal", true,
					"time",    1.25f,
					"delay",    0.25f,
					"easetype", iTween.EaseType.easeInOutSine
				)
			);

		}

		public void Hide () {

			iTween.Stop (RingCTopRef.gameObject);
			iTween.ScaleTo (
				RingCTopRef.gameObject,
				iTween.Hash (
					"x",       0.0f,
					"y",       0.0f,
					"islocal", true,
					"time",    1.25f,
					"delay",    0.25f,
					"easetype", iTween.EaseType.easeInOutSine
				)
			);

			iTween.Stop (RingCBottomRef.gameObject);
			iTween.ScaleTo (
				RingCBottomRef.gameObject,
				iTween.Hash (
					"x",       0.0f,
					"y",       0.0f,
					"islocal", true,
					"time",    1.25f,
					"delay",    0.25f,
					"easetype", iTween.EaseType.easeInOutSine
				)
			);

			iTween.Stop (RingBTopRef.gameObject);
			iTween.ScaleTo (
				RingBTopRef.gameObject,
				iTween.Hash (
					"x",       0.0f,
					"y",       0.0f,
					"islocal", true,
					"time",    1.25f,
					"delay",   0.0f,
					"easetype", iTween.EaseType.easeInOutSine
				)
			);

			iTween.Stop (RingBBottomRef.gameObject);
			iTween.ScaleTo (
				RingBBottomRef.gameObject,
				iTween.Hash (
					"x",       0.0f,
					"y",       0.0f,
					"islocal", true,
					"time",    1.25f,
					"delay",   0.0f,
					"easetype", iTween.EaseType.easeInOutSine
				)
			);

		}
	}
}
