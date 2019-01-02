using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class used for lerping other clients' position
class UserInterpolation
{
    internal Vector3 start, dest;
    internal float speed, progress;
    internal GameObject go;
    internal bool isMoving;

    internal UserInterpolation(GameObject ins, Vector3 destination, float sp)
    {
        go = ins;
        start = ins.transform.position;
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
    [SerializeField] float speed = 10f;

    public bool isClientMoving {get; private set; }
    
    HashSet<UserInterpolation> otherUsers;
    Vector3 start;
    float progress = 0f;
    NetworkClient client; 

    void Start()
    {
        client = GetComponent<NetworkClient>();
        otherUsers = new HashSet<UserInterpolation>();
    }

    void Update()
    {
        if(isClientMoving)
        {
            if (progress < 1)
            {
                progress += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(start, client.desiredPosition, progress);
            } else
                isClientMoving = false;
        }

        if(otherUsers.Count > 0)
        {
            foreach(UserInterpolation user in otherUsers)
            {
                if(user.progress < 1f)
                {
                    user.progress += Time.deltaTime * user.speed;
                    user.go.transform.position = Vector3.Lerp(user.start, user.dest, user.progress);
                } else
                    user.isMoving = false;
                // TODO: delete the users finished moving
            }
        }
    }

    public void Move(GameObject go, Vector3 dest)
    {
        if(go == gameObject)
        {
            start = transform.position;
            client.desiredPosition = dest;
            isClientMoving = true;
            progress = 0f;
        }
        else
        {
            // if already contains the gameobject, move it to its destination and set a new destination
            foreach(UserInterpolation user in otherUsers)
                if(user.go == go)
                {
                    // TODO: do a unit test that will check the same gameobject is not being added again
                    user.go.transform.position = user.dest;
                    user.start = user.go.transform.position;
                    user.progress = 0f;
                    user.dest = dest;
                    return;
                }
            otherUsers.Add(new UserInterpolation(go, dest, speed));
        }
    }
}
