using UnityEngine;
using UnityEngine.AI;

public class PreyEnemy : CustomEnemy
{
    public float randomDestinationDuration = 3f;
    public float destinationThreshold = .5f;
    Transform groundTransform;
    float destinationSetTime;
    Vector3 currentDestination;

    private void Start()
    {
        groundTransform = arenaManager.ground.transform;
        if (enableMovement)
        {
            SetRandomDestinationInSpawnRange();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("agent"))
        {
            agent.RewardAgent(gameObject);
        }
    }

    public void SetRandomDestinationInSpawnRange()
    {
        destinationSetTime = Time.time;
        Vector2 randomSpawnInCircle = Utilities.RandomPositionInSpawnCircle(arenaManager.spawnRange);
        currentDestination = groundTransform.position + new Vector3(randomSpawnInCircle.x, 1, randomSpawnInCircle.y);
        navMeshAgent.SetDestination(currentDestination);
        Debug.DrawLine(transform.position, currentDestination, Color.white, randomDestinationDuration);
    }

    private void FixedUpdate()
    {
        if (
            enableMovement &&
            (
                Time.time - destinationSetTime >= randomDestinationDuration
            || Vector3.Distance(transform.position, currentDestination) <= destinationThreshold
            )
        )
        {
            SetRandomDestinationInSpawnRange();
        }
    }
}