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

[RequireComponent(typeof(NetworkClientDisplay))]
public class NetworkClient : MonoBehaviour
{
    // set it to your server address
    [SerializeField] string serverIP = "127.0.0.1";
    [SerializeField] int port = 8080;

    #region "Public Members"
    public string id { get; private set; }
    public int packetNumber { get; private set; }
    public Dictionary<int, StateHistory> history;
    [HideInInspector] public Vector3 desiredPosition;
    #endregion

    #region "Private Members"
    Dictionary<string, GameObject> otherClients;
    NetworkClientDisplay otherClientMover;
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
        desiredPosition = transform.position;
        otherClientMover = GetComponent<NetworkClientDisplay>();
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
        string p = "n " + transform.position.x + " " + transform.position.y
                + " " + transform.position.z;

        byte[] packet = Encoding.ASCII.GetBytes(p);
        udp.SendTo(packet, endPoint);
    }

    public void SendPacket(string str)
    {
        if(id == null || id == "")
        {
            Debug.LogError("NOT Connected to server! (hint: start the server and then play the scene again");
            SendInitialReqToServer();
            return;
        }
        UpdateStateHistory();
        byte[] arr = Encoding.ASCII.GetBytes(packetNumber + " " + id + " " + str);
        udp.SendTo(arr, endPoint);
    }

    public void UpdateStateHistory()
    {
        /* using desiredPosition because transform.position is used for lerping to desired position */
        history.Add(++packetNumber, new StateHistory(desiredPosition));
        bool suc = history.Remove(packetNumber - 6);
    }

    void OnApplicationQuit()
    {
        byte[] arr = Encoding.ASCII.GetBytes("e " + id);
        udp.SendTo(arr, endPoint);
        udp.Close();
    }

    void Update()
    {
        if(udp.Available != 0)
        {
            byte[] buffer = new byte[64];
            udp.Receive(buffer);

            string data = Encoding.Default.GetString(buffer);
            string parsedID = Regex.Match(data, @"c\d+t").Value;
            Debug.Log("Received: " + data);

            // server sending the unique id of the client
            if(data[0] == 'a')
            {
                id = parsedID;
                Debug.Log("client ID: " + id);
                return;
            } else if(data[0] == 'e' && otherClients.ContainsKey(parsedID))
            {
                // means parsedID has disconnected
                otherClientMover.usersToInterpolate.Remove(otherClients[parsedID]);
                Destroy(otherClients[parsedID]);
                otherClients.Remove(parsedID);
                return;
            }
            
            int seqNumber = ParseSequenceNumber(data);
            if(parsedID == "" || seqNumber == -1)  return;

            Vector3 posInPacket = ParsePosition(data);
            if(parsedID.Equals(id) && history.ContainsKey(seqNumber) && !history[seqNumber].position.Equals(posInPacket))
            {
                Debug.Log("Server-Client position mismatch, you're at " + posInPacket);
                /* the authorative server will control the user's position if they mismatch */
                otherClientMover.usersToInterpolate.Remove(gameObject);
                transform.position = posInPacket;
            }
            else if(otherClients.ContainsKey(parsedID))
                otherClientMover.Move(otherClients[parsedID], posInPacket);
            else if(!parsedID.Equals(id))
                AddOtherClient(parsedID, posInPacket);
        }
    }

    int ParseSequenceNumber(string data)
    {
        Match match = Regex.Match(data, @"(?<seqNumber>\d+) c\d+t");
        int seqNumber;
        return int.TryParse(match.Groups["seqNumber"].Value, out seqNumber) ? seqNumber : -1;
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
