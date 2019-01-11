using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [SerializeField] Text leftScoreText, rightScoreText;
    int leftScore = 0, rightScore = 0;
    bool gameStarted = false;
    //NetworkClient client;
    void Start()
    {
        //client = GetComponent<NetworkClient>();
        //StartBallMovement(); // change later
    }

    void Update()
    {
        //if(!gameStarted && client.otherClients.Count >= 1)
            //StartBallMovement();
    }
    
    void StartBallMovement()
    {
        transform.position = new Vector2(0, 0);
        GetComponent<Rigidbody2D>().AddForce(new Vector2(300, 150));
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "ScoreWall")
        {
            if(col.transform.position.x > 0)
                leftScoreText.text = (++leftScore).ToString();
            else
                rightScoreText.text = (++rightScore).ToString();
            //StartBallMovement();
        }
    }
}
