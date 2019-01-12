using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;

public class Server : MonoBehaviour
{
    #region "Inspector Members"
    [SerializeField] int port = 8080;
    [Tooltip("Distance to move at each move input (must match with client)")]
    [SerializeField] float moveDistance = 1f;
    [Tooltip("Number of frames to wait until next processing")]
    [SerializeField] int frameWait = 2;
    [SerializeField] int maxClients = 2;
    #endregion

    #region "Private Members"
    Socket udp;
    int idAssignIndex = 0;
    Dictionary<EndPoint, Client> clients;
    #endregion

    void Start()
    {
        clients = new Dictionary<EndPoint, Client>();

        IPHostEntry host = Dns.Resolve(Dns.GetHostName());
        IPAddress ip = host.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ip, port);

        Debug.Log("Server IP Address: " + ip);
        Debug.Log("Port: " + port);
        udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udp.Bind(endPoint);
        udp.Blocking = false;
    }

    void Update()
    {
        if(Time.frameCount % frameWait == 0 && udp.Available != 0)
        {
            byte[] packet = new byte[64];
            EndPoint sender = new IPEndPoint(IPAddress.Any, port);

            int rec = udp.ReceiveFrom(packet, ref sender);
            string info = Encoding.Default.GetString(packet);

            Debug.Log("Server received: " + info);

            if(info[0] == 'n' && clients.Count < maxClients)
                HandleNewClient(sender, info);
            else if(info[0] == 'e')
                DisconnectClient(sender, info);
            else if(rec > 0)
            {
                string id = Parser.ParseID(info);
                int seqNumber = Parser.ParseSequenceNumber(info);
                if(id == "" || seqNumber == -1)    return;

                string userInput = Parser.ParseInput(info);
                if(userInput != "")
                    HandleUserMoveInput(sender, userInput, seqNumber);
            }
        }
    }

    void HandleNewClient(EndPoint addr, string data)
    {
        string id = "c" + idAssignIndex++ + "t";
        Debug.Log("Handling new client with id " + id);
        SendPacket("a " + id, addr);

        Vector3 pos = Parser.ParseInitialPosition(data);
        clients.Add(addr, new Client(id, pos));
        SendPositionToAllClients();
    }

    void DisconnectClient(EndPoint sender, string data)
    {
        if(clients.ContainsKey(sender))
            clients.Remove(sender);
        Broadcast(data);
    }

    void Broadcast(string data)
    {
        foreach(KeyValuePair<EndPoint, Client> p in clients)
            SendPacket(data, p.Key);
    }

    void HandleUserMoveInput(EndPoint client, string userInput, int seqNumber)
    {
        if(!clients.ContainsKey(client) || clients[client].lastSeqNumber > seqNumber)
            return;

        if(!clients[client].history.ContainsKey(seqNumber))
        {
            clients[client].UpdateStateHistory(seqNumber);
            clients[client].lastSeqNumber = seqNumber;
        }
        UpdatePosition(client, userInput);
        /* so that clients see newly connected clients */
        SendPositionToAllClients();
    }

    void UpdatePosition(EndPoint addr, string input)
    {
        if(input.Equals("a"))
            clients[addr].position.x -= moveDistance;
        else if(input.Equals("d"))
            clients[addr].position.x += moveDistance;
        else if(input.Equals("w"))
            clients[addr].position.y += moveDistance;
        else if(input.Equals("s"))
            clients[addr].position.y -= moveDistance;
    }

    void SendPositionToAllClients()
    {
        foreach(KeyValuePair<EndPoint, Client> p in clients)
            foreach(KeyValuePair<EndPoint, Client> p2 in clients)
                SendPacket(p2.Value.ToString(), p.Key);
    }

    Vector3 ParsePosition(Match match)
    {
        float x, y, z;
        float.TryParse(match.Groups["x"].Value, out x);
        float.TryParse(match.Groups["y"].Value, out y);
        float.TryParse(match.Groups["z"].Value, out z);
        return new Vector3(x, y, z);
    }

    void SendPacket(string str, EndPoint addr)
    {
        byte[] arr = Encoding.ASCII.GetBytes(str);
        udp.SendTo(arr, addr);
    }
}
