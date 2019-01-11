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
        foreach(KeyValuePair<EndPoint, Client> client in clients)
        {
            if(client.Value.position.x < 0 && client.Value.opponent != null)
            {
                client.Value.ballPosition = client.Value.opponent.ballPosition;
                Vector3 ballPos = client.Value.ballPosition;
                SendPacket("b " + ballPos.x + " " + ballPos.y + " " + ballPos.z, client.Key);
                SendPacket("b " + ballPos.x + " " + ballPos.y + " " + ballPos.z, client.Value.opponent.address);
            }
        }

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
            {
                string id = Parser.ParseID(info);
                int seqNumber = Parser.ParseSequenceNumber(info);
                if(id == "" || seqNumber == -1)    return;

                string userInput = Parser.ParseInput(info);
                if(userInput != "")
                    HandleUserMoveInput(sender, userInput, seqNumber);

                if(Parser.isValidBallPosition(info))
                {
                    Vector3 ballPos = Parser.ParseBallPosition(info);
                    /* if the player is on left side, position of their ball will mirror their enemy's */
                    if(clients[sender].position.x > 0)
                        clients[sender].ballPosition = ballPos;
                }
            }
        }
    }

    void HandleNewClient(EndPoint addr, string data)
    {
        if(clients.ContainsKey(addr))
            return;
        string id = "c" + idAssignIndex++ + "t";
        Debug.Log("Handling new client with id " + id);

        Vector3 pos = Parser.ParseInitialPosition(data);
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
        if(!clients.ContainsKey(sender))    return;
        /* letting the opponent know the client has disconnected */
        if(clients[sender].opponent != null)
            SendPacket(data, clients[sender].opponent.address);
        clients.Remove(sender);
    }

    void HandleUserMoveInput(EndPoint client, string userInput, int seqNumber)
    {
        if(!clients.ContainsKey(client))    return;
        
        if(clients[client].lastSeqNumber <= seqNumber)
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

    void SendPacket(string str, EndPoint addr)
    {
        byte[] arr = Encoding.ASCII.GetBytes(str);
        udp.SendTo(arr, addr);
    }
}
