using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;

class Client
{
    internal class StateHistory
    {
        public Vector3 position;
        public StateHistory(Vector3 pos)
        {
            this.position = pos;
        }
    }
    
    public string id;
    public Dictionary<int, StateHistory> history;
    public Vector3 position;
    public int lastSeqNumber;

    public Client(string i, Vector3 p)
    {
        id = i;
        position = p;
        lastSeqNumber = 0;
        history = new Dictionary<int, StateHistory>();
        history.Add(0, new StateHistory(position));
    }

    public override string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append(lastSeqNumber);
        str.Append(" ");
        str.Append(id);
        str.Append(" ");
        str.Append(position.x);
        str.Append(" ");
        str.Append(position.y);
        str.Append(" ");
        str.Append(position.z);
        return str.ToString();
    }
}

public class Server : MonoBehaviour
{
    Socket socket;
    int idAssignIndex = 0;
    Dictionary<EndPoint, Client> clients;

    void Start()
    {
        clients = new Dictionary<EndPoint, Client>();

        IPHostEntry host = Dns.Resolve(Dns.GetHostName());
        //IPHostEntry host = Dns.Resolve("127.0.0.1");
        IPAddress ip = host.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 8080);

        Debug.Log("Server IP Address: " + ip);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(endPoint);
        socket.Blocking = false;
    }

    void Update()
    {       
        if(socket.Available != 0)
        {
            byte[] packet = new byte[64];
            EndPoint temp = new IPEndPoint(IPAddress.Any, 1025);

            int rec = socket.ReceiveFrom(packet, ref temp);
            string info = Encoding.Default.GetString(packet);

            Debug.Log("Server received: " + info);

            if(info[0] == 'n')
                HandleNewClient(temp, info);
            else if(rec > 0)
            {
                HandleUserMoveInput(temp, info);
            }
        }
    }

    void HandleNewClient(EndPoint addr, string data)
    {
        string id = "c" + idAssignIndex++ + "t";
        Debug.Log("Handling new client with id " + id);
        
        SendPacket("a " + id, addr);

        Match match = Regex.Match(data,
            @"n (?<x>-?([0-9]*[.])?[0-9]+) (?<y>-?([0-9]*[.])?[0-9]+) (?<z>-?([0-9]*[.])?[0-9]+)");
        Vector3 pos = ParsePosition(match);

        clients.Add(addr, new Client(id, pos));

        SendPositionToAllClients();
    }

    void HandleUserMoveInput(EndPoint client, string info)
    {
        Regex pattern = new Regex(@"(?<seqNumber>\d+) (?<id>c\d+t) (?<input>[aswd])");
        Match match = pattern.Match(info);
        if(match.Value == "")   return;

        int seqNumber = 0;
        bool res = int.TryParse(match.Groups["seqNumber"].Value, out seqNumber);
        if(!res)    return;

        string id = match.Groups["id"].Value;
        string userInput = match.Groups["input"].Value;
        
        if(id != "" && userInput != "")
        {
            if(!clients[client].history.ContainsKey(seqNumber))
            {
                clients[client].history.Add(seqNumber, new Client.StateHistory(clients[client].position));
                clients[client].lastSeqNumber = seqNumber;
            }
            UpdatePosition(client, userInput);
            foreach(KeyValuePair<EndPoint, Client> p in clients)
                SendPacket(clients[client].ToString(), p.Key);
        }
    }

    void UpdatePosition(EndPoint addr, string input)
    {
        float updateRate = 1f;

        if(input.Equals("a"))
            clients[addr].position.x -= updateRate;
        else if(input.Equals("d"))
            clients[addr].position.x += updateRate;
        else if(input.Equals("w"))
            clients[addr].position.y += updateRate;
        else if(input.Equals("s"))
            clients[addr].position.y -= updateRate;
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
        socket.SendTo(arr, addr);
    }
}
