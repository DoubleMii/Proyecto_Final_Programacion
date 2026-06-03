using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCControl : MonoBehaviour
{
    public Transform[] puntos;

    private NavMeshAgent agent;

    private int posActual = 0;

    private bool wait = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.SetDestination(puntos[0].position);
    }

    void Update()
    {
        if (!wait && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(Esperar());
        }
    }

    IEnumerator Esperar()
    {
        wait = true;

        yield return new WaitForSeconds(2);

        posActual++;

        if (posActual >= puntos.Length)
        {
            posActual = 0;
        }

        agent.SetDestination(puntos[posActual].position);

        wait = false;
    }
}
