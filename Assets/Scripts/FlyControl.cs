using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyControl : MonoBehaviour
{
    float time = 0;
    float speed;
    float width;
    float height;
    bool RotateValue;
    float alpha;
    // Start is called before the first frame update
    void Start()
    {
        speed = 0.3f;
        width = 400;
        height = 400;
        RotateValue = true;
        alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {

        
        time += Time.deltaTime * speed;

        float x = Mathf.Cos(time) * height;
        float y = 220;
        float z = Mathf.Sin(time) * width;

        transform.position = new Vector3(x, y, z);

        alpha = alpha + 0.001f;
        if (alpha > 0.6)
        {
            alpha = 0;
        }
        transform.Rotate(0, -alpha, 0);
        
       
    }
}
