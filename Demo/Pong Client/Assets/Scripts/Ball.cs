using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(new Vector2(320, 100));
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "ScoreWall")
        {
            if(col.transform.position.x > 0)
                Debug.Log("Left scored");
            else
                Debug.Log("Right scored");
        }
    }
}
