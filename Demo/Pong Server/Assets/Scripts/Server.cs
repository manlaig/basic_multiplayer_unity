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
    [Tooltip("Distance to move at each move input (must match with client)")]
    [SerializeField] float moveDistance = 1f;
    [Tooltip("Number of frames to wait until next processing")]
    [SerializeField] int frameWait = 2;
    [SerializeField] int maxClients = 2;
    #endregion

    #region "Private Members"
    Socket udp;
    int port = 8080;
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
        if(Time.frameCount % frameWait == 0 && udp.Available > 0)
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
                HandleUserMoveInput(sender, info);
        }
    }

    void HandleNewClient(EndPoint addr, string data)
    {
        string id = "c" + idAssignIndex++ + "t";
        Debug.Log("Handling new client with id " + id);

        Match match = Regex.Match(data,
            @"n (?<x>-?([0-9]*[.])?[0-9]+) (?<y>-?([0-9]*[.])?[0-9]+) (?<z>-?([0-9]*[.])?[0-9]+)");
        Vector3 pos = ParsePosition(match);
        Client newClient = new Client(id, pos, addr);

        foreach(KeyValuePair<EndPoint, Client> player in clients)
            if(player.Value.opponent == null)
            {
                /* connecting the two clients */
                player.Value.opponent = newClient;
                newClient.opponent = player.Value;

                /* if the opponent is on left side, move this player to the right side */
                newClient.position = player.Value.position.x < 0f ? new Vector3(5.7f, 0f, 0f) : new Vector3(-5.7f, 0f, 0f);

                /* sending their initial data to both players */
                SendPacket(player.Value.ToString(), addr);
                SendPacket(newClient.ToString(), player.Key);
                /* telling both clients to start the game (start the ball moving) */
                SendPacket("s", addr);
                SendPacket("s", player.Key);
                break;
            }
        SendPacket("a " + id + " " + newClient.position.x + " " + newClient.position.y + " " + newClient.position.z, addr);
        clients.Add(addr, newClient);
    }

    void DisconnectClient(EndPoint sender, string data)
    {
        /* letting the opponent know the client has disconnected */
        if(clients[sender].opponent != null)
            SendPacket(data, clients[sender].opponent.address);
        if(clients.ContainsKey(sender))
            clients.Remove(sender);
    }

    void HandleUserMoveInput(EndPoint client, string info)
    {
        if(!clients.ContainsKey(client))
            return;
        Regex pattern = new Regex(@"(?<seqNumber>\d+) (?<id>c\d+t) (?<input>[ws])");
        Match match = pattern.Match(info);
        if(match.Value == "")   return;

        int seqNumber = 0;
        bool res = int.TryParse(match.Groups["seqNumber"].Value, out seqNumber);
        if(!res)    return;

        string id = match.Groups["id"].Value;
        string userInput = match.Groups["input"].Value;
        
        if(id != "" && userInput != "" && clients[client].lastSeqNumber <= seqNumber)
        {
            if(!clients[client].history.ContainsKey(seqNumber))
            {
                clients[client].UpdateStateHistory(seqNumber);
                clients[client].lastSeqNumber = seqNumber;
            }
            UpdatePosition(client, userInput);
            /* sending the new position of the client to them and their opponent */
            SendPacket(clients[client].ToString(), client);
            if(clients[client].opponent != null)
                SendPacket(clients[client].ToString(), clients[client].opponent.address);
        }
    }

    void UpdatePosition(EndPoint addr, string input)
    {
        if(input.Equals("w"))
            clients[addr].position.y += moveDistance;
        else if(input.Equals("s"))
            clients[addr].position.y -= moveDistance;
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
