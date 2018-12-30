using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

public class NetworkClient : MonoBehaviour
{
    // set it to your server address
    [SerializeField] string serverIP = "127.0.0.1";
    [SerializeField] int port = 8080; 

    public string id { get; private set; }
    Dictionary<string, GameObject> otherClients;
    Socket s;
    IPEndPoint endPoint;

    void Awake()
    {
        if(serverIP == "")
            Debug.LogError("Server IP Address not set");
        if(port == -1)
            Debug.LogError("Port not set");

        otherClients = new Dictionary<string, GameObject>();
        endPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);

        s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        s.Blocking = false;

        // n stands for new user
        SendInitialReqToServer("n");
        // server will reply with a unique id for this user
    }

    void SendInitialReqToServer(string header)
    {   
        // TODO: send rotation also
        string p = header + " " + transform.position.x + " " + transform.position.y
                + " " + transform.position.z;

        byte[] packet = Encoding.ASCII.GetBytes(p);
        s.SendTo(packet, endPoint);
    }

    public void SendPacket(string str)
    {
        byte[] arr = Encoding.ASCII.GetBytes(id + " " + str);
        s.SendTo(arr, endPoint);
    }

    void Update()
    {
        if(s.Available != 0)
        {
            byte[] buffer = new byte[64];
            s.Receive(buffer);

            string data = Encoding.Default.GetString(buffer);

            string parsedID = Regex.Match(data, @"c\d+t").Value;
            if(parsedID == "")  return;
            
            // means the server sending the unique id of the client
            if(data[0] == 'a')
            {
                id = parsedID;
                Debug.Log("client ID: " + id);
                return;
            }

            if(parsedID.Equals(id))
                transform.position = ParsePosition(data);
            else if(otherClients.ContainsKey(parsedID))
                otherClients[parsedID].transform.position = ParsePosition(data);
            else
                AddOtherClient(parsedID, ParsePosition(data));
        }
    }

    Vector3 ParsePosition(string data)
    {  
        Match match = Regex.Match(data,
            @"c\d+t (?<x>-?([0-9]*[.])?[0-9]+) (?<y>-?([0-9]*[.])?[0-9]+) (?<z>-?([0-9]*[.])?[0-9]+)");

        float x, y, z;
        float.TryParse(match.Groups["x"].Value, out x);
        float.TryParse(match.Groups["y"].Value, out y);
        float.TryParse(match.Groups["z"].Value, out z);

        return new Vector3(x, y, z);
    }

    void AddOtherClient(string parsedID, Vector3 pos)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = parsedID;
        go.transform.position = pos;
        otherClients.Add(parsedID, go);
    }
}
