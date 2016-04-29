using UnityEngine;
using System.Collections;

public class KeyToActive : MonoBehaviour
{
    public GameObject[] gos;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            foreach (var g in gos)
                g.SetActive(true);
    }
}
