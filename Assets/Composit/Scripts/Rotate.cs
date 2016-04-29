using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour
{
    Vector3 axis;
    // Use this for initialization
    void Start()
    {
        axis = Random.onUnitSphere;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(axis * Time.deltaTime * 90f);
    }
}
