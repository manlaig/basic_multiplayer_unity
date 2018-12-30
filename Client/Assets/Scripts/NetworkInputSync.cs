using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkClient))]
public class NetworkInputSync : MonoBehaviour
{
    NetworkClient client;

    void Start()
    {
        client = GetComponent<NetworkClient>();
    }

    void Update()
    {
        if(client.id != "")
            SendMoveInput();
    }

    void SendMoveInput()
    {
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            client.SendPacket("a");
        else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            client.SendPacket("d");
        
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            client.SendPacket("w");
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            client.SendPacket("s");
    }
}
