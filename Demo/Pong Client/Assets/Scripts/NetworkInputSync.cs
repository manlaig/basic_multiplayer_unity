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
            if(userInput != "" && !(clientMover.usersToInterpolate.ContainsKey(gameObject) && clientMover.usersToInterpolate[gameObject].isMoving))
            {
                Move(userInput);
                client.SendPacket(userInput);
            }
        }
    }

    string GetMoveInput()
    {
        string input = "";
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            input = "w";
        else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            input = "s";
        return input;
    }

    public void Move(string userInput)
    {
        Vector3 newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        if(userInput == "w")
            newPos.y += moveDistance;
        else if(userInput == "s")
            newPos.y -= moveDistance;
        
        client.desiredPosition = newPos;
        /* interpolate client to newPos */
        clientMover.Move(gameObject, newPos);
    }
}
