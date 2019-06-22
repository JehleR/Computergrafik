using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyControl : MonoBehaviour
{
    float speed;
    float alpha;
    // Start is called before the first frame update
    void Start()
    {
        speed = 0.3f;
        alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed * 200);

        alpha = alpha + 0.001f;
        if (alpha > 0.585)
        {
            alpha = 0;
        }
        transform.Rotate(0, -alpha, 0);
    }
}
