using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;

public class Client
{
    public class StateHistory
    {
        public Vector3 position;
        public StateHistory(Vector3 pos)
        {
            this.position = pos;
        }
    }
    
    public string id;
    public int lastSeqNumber;
    public Dictionary<int, StateHistory> history;
    public Vector3 position;
    public EndPoint address;
    public Client opponent; // opponent for Pong

    public Client(string i, Vector3 p, EndPoint addr)
    {
        id = i;
        position = p;
        address = addr;
        opponent = null;
        lastSeqNumber = 0;
        history = new Dictionary<int, StateHistory>();
        history.Add(0, new StateHistory(position));
    }

    public void UpdateStateHistory(int seqNumber)
    {
        history.Add(seqNumber, new Client.StateHistory(position));
        bool suc = history.Remove(lastSeqNumber - 50);
    }

    public override string ToString()
    {
        /* example: "25 c0t 1 2 3" */
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