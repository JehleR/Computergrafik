using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public float sunSpeed = 10f;

    private Vector3 rotationVector = Vector3.up;
    // Update is called once per frame
    void Update()
    {
        this.transform.RotateAround(Vector3.zero, rotationVector, sunSpeed * Time.deltaTime);
    }
}
