using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public float sunSpeed = 10f;

    private Vector3 rotationVecotr = new Vector3(0, 1, 0);
    // Update is called once per frame
    void Update()
    {
        this.transform.RotateAround(Vector3.zero, rotationVecotr, sunSpeed * Time.deltaTime);
    }
}
