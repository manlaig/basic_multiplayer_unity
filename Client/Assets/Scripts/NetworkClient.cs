using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

public class StateHistory
{
    public Vector3 position;

    public StateHistory(Vector3 pos)
    {
        position = pos;
    }
}

public class NetworkClient : MonoBehaviour
{
    // set it to your server address
    [SerializeField] string serverIP = "127.0.0.1";
    [SerializeField] int port = 8080;

    #region "Public Members"
    public string id { get; private set; }
    public int packetNumber { get; private set; }
    public Dictionary<int, StateHistory> history;
    #endregion

    #region "Private Members"
    Dictionary<string, GameObject> otherClients;
    Socket udp;
    IPEndPoint endPoint;
    #endregion

    void Awake()
    {
        if(serverIP == "")
            Debug.LogError("Server IP Address not set");
        if(port == -1)
            Debug.LogError("Port not set");

        packetNumber = 0;
        otherClients = new Dictionary<string, GameObject>();
        history = new Dictionary<int, StateHistory>();
        endPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
        udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udp.Blocking = false;

        // n stands for new user
        // server will reply with a unique id for this user
        SendInitialReqToServer();
        history.Add(0, new StateHistory(transform.position));
    }

    void SendInitialReqToServer()
    {   
        // TODO: send rotation also
        string p = "n " + transform.position.x + " " + transform.position.y
                + " " + transform.position.z;

        byte[] packet = Encoding.ASCII.GetBytes(p);
        udp.SendTo(packet, endPoint);
    }

    public void SendPacket(string str)
    {
        history.Add(++packetNumber, new StateHistory(transform.position));
        byte[] arr = Encoding.ASCII.GetBytes(packetNumber + " " + id + " " + str);
        udp.SendTo(arr, endPoint);
    }

    void Update()
    {
        if(udp.Available != 0)
        {
            byte[] buffer = new byte[64];
            udp.Receive(buffer);

            string data = Encoding.Default.GetString(buffer);

            string parsedID = Regex.Match(data, @"c\d+t").Value;
            if(parsedID == "")  return;
            int seqNumber = 0;
            int.TryParse(Regex.Match(data, @"(?<seqNum>\d+) c\d+t").Value, out seqNumber);
            
            // means the server sending the unique id of the client
            if(data[0] == 'a')
            {
                id = parsedID;
                Debug.Log("client ID: " + id);
                return;
            }

            Vector3 posInPacket = ParsePosition(data);
            if(parsedID.Equals(id) && history.ContainsKey(seqNumber) && history[seqNumber].position != posInPacket)
            {
                transform.position = posInPacket;
                Debug.Log("You're at " + posInPacket);
            }
            else if(otherClients.ContainsKey(parsedID))
                otherClients[parsedID].transform.position = posInPacket;
            else if(!parsedID.Equals(id))
                AddOtherClient(parsedID, posInPacket);
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
