using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PvSShooter : CustomAgent
{
    public PvSPredator predator;
    public Gun gun;
    public TextMeshPro currentScoreTmp;
    public TextMeshPro prevScoreTmp;
    public TextMeshPro avgScoreTmp;
    public bool useVectorObservers = true;
    protected float averageScore;


    protected override void PreSpawnLogic()
    {
        arenaManager.MoveWalls();
    }

    protected override void PreEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.eulerAngles = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObservers)
        {
            // add relative distance of closest enemy
            // so that agent is aware of avoiding/killing them
            // TODO: get predator's transform position
            Vector3 closestEnemyPosition = predator.transform.position;
            float distance = Vector3.Distance(transform.position, closestEnemyPosition);

            // using gun's muzzle to get position
            Vector3 closestEnemyDirection = gun.muzzleTransform.position - closestEnemyPosition;
            closestEnemyDirection = closestEnemyDirection.normalized;
            // direction vector for closest enemy
            sensor.AddObservation(closestEnemyDirection);
            sensor.AddObservation(distance / arenaManager.spawnRange);
            // current direction at which it is looking at
            sensor.AddObservation(transform.forward.normalized);
        }
    }
    public override void OnActionReceived(float[] vectorAction)
    {
        // logic to shoot according to enemyMovementType
        switch (vectorAction[0])
        {
            case 0:
                // do nothing
                break;
            case 1:
                // shoot
                gun.Fire();
                // discourage agent to spam fire button
                AddReward(-0.005f);
                break;
            default:
                Debug.LogWarning("Invalid input -> " + vectorAction[0]);
                break;
        }

        switch (vectorAction[1])
        {
            case 0:
                // do nothing
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

        Debug.DrawRay(transform.position, transform.forward, Color.yellow, 0.2f);
        switch (vectorAction[2])
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

        // negative reward for existing
        AddReward(-1f / MaxStep);
    }
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        // add keyboard controls
        if (Input.GetKey(KeyCode.Space))
        {
            actionsOut[0] = 1;
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

        // Vertical axis for movement
        actionsOut[2] = 0;
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[2] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[2] = 2;
        }
    }

    protected override void HandleScore()
    {
        currentScore = Utilities.RoundTo(GetCumulativeReward(), 2);
        currentScoreTmp.text = "Score: " + currentScore;
    }


    protected void UpdateScore()
    {
        prevScoreTmp.text = "Prev: " + currentScore;
        averageScore = Utilities.RoundTo((averageScore + currentScore) / 2, 2);
        avgScoreTmp.text = "Avg: " + averageScore;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("obstacle"))
        {
            AddReward(-0.01f);
        }
        else if (other.gameObject.CompareTag("wall"))
        {
            AddReward(-0.05f);
        }
    }
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("obstacle"))
        {
            AddReward(-0.01f);
        }
        else if (other.gameObject.CompareTag("wall"))
        {
            AddReward(-0.01f);
        }
    }

    public void EndEpisodeAndScore(bool predatorWin)
    {
        if (predatorWin)
        {
            AddReward(maxNegativeReward);
        }
        else
        {
            AddReward(maxPositiveReward);
        }

        HandleScore();
        UpdateScore();
        EndEpisode();
    }
}
