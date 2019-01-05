using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkClient))]
[RequireComponent(typeof(NetworkClientDisplay))]
public class NetworkInputSync : MonoBehaviour
{
    [Tooltip("The distance to be moved in each move input")]
    [SerializeField] float moveDistance = 1f;

    NetworkClient client;
    NetworkClientDisplay clientMover;

    void Start()
    {
        client = GetComponent<NetworkClient>();
        clientMover = GetComponent<NetworkClientDisplay>();
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
        // to prevent mismatch between server and client
        if(clientMover.usersToInterpolate.ContainsKey(gameObject) && clientMover.usersToInterpolate[gameObject].isMoving)
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
        
        client.desiredPosition = newPos;
        //interpolate client to newPos
        clientMover.Move(gameObject, newPos);
    }
}
