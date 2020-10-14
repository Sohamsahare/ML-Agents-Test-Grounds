using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGoal : MonoBehaviour
{
    [HideInInspector]
    public BasicAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("agent"))
        {
            agent.ReachedGoal();
        }
        Debug.LogWarning(other.tag + " entered");
    }
}
