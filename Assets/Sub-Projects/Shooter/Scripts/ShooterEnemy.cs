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
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        float og = navMeshAgent.speed;
        navMeshAgent.speed = Academy
                                .Instance
                                .EnvironmentParameters
                                .GetWithDefault("speed", defaultParameter);
        if (navMeshAgent.speed != og)
        {
            Debug.Log("Speed Changed to -> " + navMeshAgent.speed + " from " + og);
        }

        og = navMeshAgent.acceleration;
        navMeshAgent.acceleration = Academy
                                .Instance
                                .EnvironmentParameters
                                .GetWithDefault("acceleration", defaultParameter);
        if (navMeshAgent.acceleration != og)
        {
            Debug.Log("Acceleration Changed to -> " + navMeshAgent.acceleration + " from " + og);
        }
    }

    private void FixedUpdate() {
        navMeshAgent.SetDestination(agent.transform.position);
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
        else if (other.transform.CompareTag("obstacle"))
        {
            agent.RespawnEnemy(gameObject);
        }
    }
}
