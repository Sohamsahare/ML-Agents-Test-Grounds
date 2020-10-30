using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;

public class CustomEnemy : MonoBehaviour
{
    [SerializeField]
    protected float defaultParameter = .95f;
    [HideInInspector]
    public CustomAgent agent;
    [HideInInspector]
    public ArenaManager arenaManager;
    protected NavMeshAgent navMeshAgent;
    public bool enableMovement = false;

    protected void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected void OnEnable()
    {
        if (enableMovement)
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
}
