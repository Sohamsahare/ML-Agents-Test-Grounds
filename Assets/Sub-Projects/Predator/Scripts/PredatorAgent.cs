using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PredatorAgent : CustomAgent
{
    public TextMeshPro hpTmp;
    public TextMeshPro enemiesKilledTmp;
    float enemyCountShotAt;
    // float enemyCountShotBy;
    protected override void PreEpisodeBegin()
    {
        enemyCountShotAt = 0;
        // enemyCountShotBy = 0;
    }
    protected override void PreSpawnLogic()
    {
        arenaManager.MoveWalls();
    }

    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     Vector3 closestEnemyPosition = FindClosestEnemyPosition();
    //     Vector3 closestEnemyDirection = transform.position - closestEnemyPosition;
    //     closestEnemyDirection = closestEnemyDirection.normalized;
    //     // direction of closest enemy
    //     sensor.AddObservation(closestEnemyDirection);
    //     // distance of closest enemy
    //     sensor.AddObservation(Vector3.Distance(transform.position, closestEnemyPosition));
    // }

    Vector3 FindClosestEnemyPosition()
    {
        List<GameObject> enemies = arenaManager.GetEnemyObjects();
        Vector3 closestPostion = Vector3.zero;
        float minDistance = float.PositiveInfinity;
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPostion = enemy.transform.position;
            }
        }
        return closestPostion;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Debug.DrawRay(transform.position, transform.forward, Color.yellow, 0.2f);
        switch (vectorAction[0])
        {
            case 0:
                // do nothing but set velocity to 0
                rb.velocity = Vector3.zero;
                break;
            case 1:
                // go forward relative to agent
                Vector3 forward = transform.forward * movementSpeed;
                rb.AddForce(forward, ForceMode.VelocityChange);
                break;
            case 2:
                // go backwards relative to agent
                Vector3 backward = transform.forward * movementSpeed * -1;
                rb.AddForce(backward, ForceMode.VelocityChange);
                break;
            default:
                // invalid scenario
                break;
        }

        switch (vectorAction[1])
        {
            case 0:
                // do nothing
                rb.angularVelocity = Vector3.zero;
                break;
            case 1:
                // rotate left relative to agent
                Vector3 left = transform.up * -1;
                transform.Rotate(left, Time.fixedDeltaTime * rotationSpeed);
                break;
            case 2:
                // rotate right relative to agent
                Vector3 right = transform.up;
                transform.Rotate(right, Time.fixedDeltaTime * rotationSpeed);
                break;
            default:
                // invalid scenario
                break;
        }
        // negative reward for existing
        AddReward(-1f / MaxStep);
    }

    public override void Heuristic(float[] actionsOut)
    {
        // Vertical axis for movement
        actionsOut[0] = 0;
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }

        // Horizontal axis for rotation
        actionsOut[1] = 0;
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = 2;
        }
    }

    protected override void PostAgentRewarded()
    {
        Debug.Log("called");
        enemyCountShotAt++;
        stats.Add("Enemies hit", enemyCountShotAt);
        enemiesKilledTmp.text = "Enemies killed: " + enemyCountShotAt;
    }

    // protected override void PostAgentPunished()
    // {
    //     enemyCountShotBy++;
    //     float hp = arenaManager.enemyCount - enemyCountShotBy;
    //     stats.Add("HP", hp);
    //     hpTmp.text = "HP: " + hp;
    // }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("wall"))
        {
            AddReward(-0.01f);
        }
    }
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("wall"))
        {
            AddReward(-0.01f);
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("wall"))
        {
            AddReward(0.1f);
        }
    }
}