using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace irishoak {
	public class TitleCharController : MonoBehaviour {


		public List <Transform> CharList       = new List <Transform> ();
		public List <Vector3>   CharOrgPosList = new List <Vector3> ();

		float _textAlpha = 0.0f;

		public Color TextColor;

		void Awake () {

			foreach (Transform t in transform) {
				if (t != transform) {
					t.GetComponent <TextMesh> ().color = new Color (1,1,1,0);
					CharList.Add (t);
					CharOrgPosList.Add (t.localPosition);
				}
			}

		}

		void Start () {
			
		}
		
		void Update () {
		
			if (Input.GetKeyUp ("1")) {
				Show ();
			}

			if (Input.GetKeyUp ("2")) {
				Scatter ();
			}

			if (Input.GetKeyUp ("3")) {
				Reset ();
			}


		}

		public void Show () {

			iTween.Stop (gameObject);
			iTween.ValueTo (
				gameObject,
				iTween.Hash (
					"from", _textAlpha,
					"to", 1.0f,
					"time", 3.0f,
					"onupdate", "__updateTextAlpha"
				)
			);
		
		}

		public void Reset () {

			_textAlpha = 0.0f;
			for (var i = 0; i < CharList.Count; i++) {
				CharList [i].localPosition    = CharOrgPosList [i];
				CharList [i].localEulerAngles = Vector3.zero;
				CharList [i].GetComponent <TextMesh> ().color = new Color (TextColor.r, TextColor.g, TextColor.b, _textAlpha);
			}


		}

		public void Scatter () {
			foreach (Transform c in CharList) {
				if (c.GetComponent <iTween> () != null) {
					iTween.Stop (c.gameObject);
				}

				var rndPos = Random.insideUnitSphere * 5.0f;
				rndPos.z += Random.Range (0.0f, 5.0f);
				var time   = Random.Range (5.0f, 8.0f);
				var delay  = 0.0f;// Random.Range (0.0f, 3.0f);
				iTween.MoveTo (
					c.gameObject,
					iTween.Hash (
						"x", c.localPosition.x + rndPos.x,
						"y", c.localPosition.y + rndPos.y,
						"z", c.localPosition.z + rndPos.z - 30.0f,
						"islocal", true,
						"time", time,
						"delay", delay,
						"easetype", iTween.EaseType.easeInQuad
					)
				);

	//			var rndRot = Random.insideUnitSphere * 360.0f;
	//			delay = Random.Range (0.0f, 2.0f);
	//			iTween.RotateTo (
	//				c.gameObject,
	//				iTween.Hash (
	//					"x", rndRot.x,
	//					"y", rndRot.y,
	//					"z", rndRot.z,
	//					"time", time,
	//					"delay", delay,
	//					"easetype", iTween.EaseType.easeInQuad
	//				)
	//			);
			}
		}

		void __updateTextAlpha (float value) {

			_textAlpha = value;

			foreach (Transform c in CharList) {
				c.GetComponent <TextMesh> ().color = new Color (TextColor.r, TextColor.g, TextColor.b, _textAlpha);
			}

		}
	}
}