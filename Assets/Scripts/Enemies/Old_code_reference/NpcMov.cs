using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcMov : MonoBehaviour
{
    NavMeshAgent Agent;
    Vector3 GoingPos;
    int Next = 0;

    public List<Transform> Destinations = new List<Transform>();
 
    void Start()
    {
       Agent = GetComponent<NavMeshAgent>();
       GoingPos = Destinations[Next].position;
       Agent.SetDestination(GoingPos);
       Next = Next + 1;


    }

 
    void Update()
    {
            if ((transform.position - GoingPos).sqrMagnitude <0.2)
            {
                GoingPos = Destinations[Next].position;
                Agent.SetDestination(GoingPos);
                Next = Next + 1;
                Debug.Log(Next);

                if (Next == Destinations.Count)
                { 
                    Next = 0;
                }

        }
               
    }
}
