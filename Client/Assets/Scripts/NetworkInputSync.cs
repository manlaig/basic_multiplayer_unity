using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkClient))]
public class NetworkInputSync : MonoBehaviour
{
    [Tooltip("The distance to be moved in each move input")]
    [SerializeField] float moveDistance = 1f;
    [Tooltip("The step length while moving towards the desired position")]
    [SerializeField] float speed = 10f;
    
    NetworkClient client;
    Vector3 start;
    float fraction = 0f;
    bool isMoving;

    void Start()
    {
        isMoving = false;
        client = GetComponent<NetworkClient>();
    }

    void Update()
    {
        if(client.id != "")
        {
            string userInput = GetMoveInput();
            if(userInput != "")
            {
                Move(userInput);
                client.SendPacket(userInput);
            }
        }

        if(isMoving)
        {
            if (fraction < 1)
            {
                fraction += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(start, client.desiredPosition, fraction);
            } else
                isMoving = false;
        }
    }

    string GetMoveInput()
    {
        string input = "";
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            input = "a";
        else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            input = "d";
        
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            input = "w";
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            input = "s";
        return input;
    }

    public void Move(string userInput)
    {
        if(isMoving)
            transform.position = client.desiredPosition;
        Vector3 newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        if(userInput == "a")
            newPos.x -= moveDistance;
        else if(userInput == "d")
            newPos.x += moveDistance;
        else if(userInput == "w")
            newPos.y += moveDistance;
        else if(userInput == "s")
            newPos.y -= moveDistance;

        start = transform.position;
        client.desiredPosition = newPos;
        isMoving = true;
        fraction = 0f;
        //transform.position = newPos;
        //Debug.Log("Curr: " + transform.position);
        //Debug.Log("Dest: " + newPos);
    }
}
