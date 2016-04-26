using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using mattatz.Utils;

namespace mattatz {

    public class Core : MonoBehaviour, IBPMSynchronizable {

        [SerializeField] GameObject prefab;
        [SerializeField] MetaballRenderer mRenderer;
        [SerializeField] List<MetaballEntity> entities;

        public int count = 4;
        public Vector2 mScaleRange = new Vector2(-0.1f, 0.1f);
        public Vector2 mRadiusRange = new Vector2(0.1f, 0.2f);

        public Vector2 speedRange = new Vector2(0.1f, 3f);
        public float speed = 1f;
        float originSpeed;

        public float distance = 1f;

        public Vector2 radiusRange = new Vector2(0.8f, 1.5f);
        public float radius = 1f;
        float originRadius;

        void Start () {
            for(int i = 0; i < count; i++) {
                var go = Instantiate(prefab);
                go.transform.parent = transform;
                go.transform.localPosition = Random.insideUnitSphere * Random.Range(mRadiusRange.x, mRadiusRange.y);
                var entity = go.GetComponent<MetaballEntity>();
                entity.radius = Random.Range(mScaleRange.x, mScaleRange.y);
                entity.m_renderer = mRenderer;
                entities.Add(entity);
            }

            originRadius = radius;
            originSpeed = speed;
        }
        
        void Update () {

            for(int i = 0, n = entities.Count; i < n; i++) {
                var entity = entities[i];
                float noise = Mathf.Lerp(0.9f, 1.1f, Mathf.PerlinNoise(i + Time.timeSinceLevelLoad, 0f));
                entity.SetDistance(Mathf.Max(distance, 0.01f));
                entity.SetRadius(radius * noise);
            }

        }

        public void OnClick(int bpm, int samples) {
            float next = 60f / bpm / samples;
            float hn = next * 0.5f;

            StartCoroutine(Easing.Ease(hn, Easing.Exponential.Out, (float t) => {
                radius = originRadius * Mathf.Lerp(radiusRange.x, radiusRange.y, t);
            }, 0f, 1f, () => {
                StartCoroutine(Easing.Ease(hn, Easing.Exponential.Out, (float t) => {
                    radius = originRadius * Mathf.Lerp(radiusRange.x, radiusRange.y, t);
                }, 1f, 0f));
            }));

            entities.ForEach(planet => {
                planet.AddForce(speed);
            });
        }

    }

}


