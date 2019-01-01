using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkClient))]
public class NetworkInputSync : MonoBehaviour
{
    [Tooltip("The distance to be moved in each move input")]
    [SerializeField] float moveDistance = 1f;
    [Tooltip("The step length while moving towards the desired position")]
    [SerializeField] float stepDistance = 1f;
    
    NetworkClient client;

    void Start()
    {
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
        Vector3 newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        if(userInput == "a")
            newPos.x -= moveDistance;
        else if(userInput == "d")
            newPos.x += moveDistance;
        else if(userInput == "w")
            newPos.y += moveDistance;
        else if(userInput == "s")
            newPos.y -= moveDistance;
        //StartCoroutine(MoveTo(newPos));
        transform.position = newPos;
    }

    public IEnumerator MoveTo(Vector3 end)
    {
        while (Vector3.Distance(transform.position,end) > stepDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, end, stepDistance);
            yield return 0;
        }
        transform.position = end;
    }
}
