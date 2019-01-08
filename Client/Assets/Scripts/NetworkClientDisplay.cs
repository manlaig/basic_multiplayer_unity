using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class used for lerping clients' position
public class UserInterpolation
{
    internal Vector3 start, dest;
    internal float speed, progress;
    internal bool isMoving;

    internal UserInterpolation(Vector3 startV, Vector3 destination, float sp)
    {
        start = startV;
        dest = destination;
        speed = sp;
        progress = 0f;
        isMoving = true;
    }
}

[RequireComponent(typeof(NetworkClient))]
public class NetworkClientDisplay : MonoBehaviour
{
    [Tooltip("The step length while moving towards the desired position")]
    [SerializeField] float speed = 8f;
    
    public Dictionary<GameObject, UserInterpolation> usersToInterpolate;
    NetworkClient client; 

    void Start()
    {
        client = GetComponent<NetworkClient>();
        usersToInterpolate = new Dictionary<GameObject, UserInterpolation>();
    }

    void Update()
    {
        if(usersToInterpolate.Count > 0)
        {
            foreach(KeyValuePair<GameObject, UserInterpolation> user in usersToInterpolate)
            {
                if(user.Value.progress < 1f)
                {
                    user.Value.progress += Time.deltaTime * user.Value.speed;
                    user.Key.transform.position = Vector3.Lerp(user.Value.start, user.Value.dest, user.Value.progress);
                }
                else    user.Value.isMoving = false;
            }
        }
    }

    public void Move(GameObject go, Vector3 dest)
    {
        usersToInterpolate[go] = new UserInterpolation(go.transform.position, dest, speed);
    }
}
