using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;

public class ShooterEnemy : MonoBehaviour
{
    [SerializeField]
    private float defaultParameter = .95f;
    [HideInInspector]
    public ShooterAgent agent = null;
    public bool disableEnemy = false;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        if (!disableEnemy)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    private void OnEnable()
    {
        if (!disableEnemy)
        {

            float og = navMeshAgent.speed;
            navMeshAgent.speed = Academy
                                    .Instance
                                    .EnvironmentParameters
                                    .GetWithDefault("speed", defaultParameter);
            // if (navMeshAgent.speed != og)
            // {
            //     Debug.Log("Speed Changed to -> " + navMeshAgent.speed + " from " + og);
            // }

            og = navMeshAgent.acceleration;
            navMeshAgent.acceleration = Academy
                                    .Instance
                                    .EnvironmentParameters
                                    .GetWithDefault("acceleration", defaultParameter);
            // if (navMeshAgent.acceleration != og)
            // {
            //     Debug.Log("Acceleration Changed to -> " + navMeshAgent.acceleration + " from " + og);
            // }
        }
    }

    private void FixedUpdate()
    {
        if (!disableEnemy)
        {
            navMeshAgent.SetDestination(agent.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("bullet"))
        {
            agent.DamageEnemy(gameObject);
        }
        else if (other.transform.CompareTag("agent"))
        {
            agent.DamageAgent(gameObject);
        }
    }
}
