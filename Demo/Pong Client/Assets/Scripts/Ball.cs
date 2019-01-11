using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [SerializeField] Text leftScoreText, rightScoreText;

    public static Ball instance { get; private set; }
    Rigidbody2D rb;
    int leftScore = 0, rightScore = 0;
    bool gameStarted = false;
    
    void Start()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void StartBallMovement()
    {
        rb.position = Vector2.zero;
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(100, 150));
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "ScoreWall")
        {
            if(col.transform.position.x > 0)
                leftScoreText.text = (++leftScore).ToString();
            else
                rightScoreText.text = (++rightScore).ToString();
            StartBallMovement();
        }
    }
}
