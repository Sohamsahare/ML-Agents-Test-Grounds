using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PvSPredator : CustomAgent
{
    public TextMeshPro currentScoreTmp;
    public TextMeshPro prevScoreTmp;
    public TextMeshPro avgScoreTmp;
    protected float averageScore;
    [SerializeField]
    protected PvSArenaManager pvsArenaManager;

    protected override void PreSpawnLogic()
    {
        pvsArenaManager.MoveWalls();
    }

    protected override void PreEpisodeBegin()
    {
        movementSpeed = pvsArenaManager.speed;
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

    // need to override 
    // InitialiseParameters in arena manager
    // prespawnlogic  -> move walls?
    // spawnenemies  -> empty
    public override void SpawnEnemies() { }

    // resetenemies null
    public override void ResetEnemies() { }

    // arenamanager isepisode ended overload needed

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("obstacle"))
        {
            AddReward(-0.01f);
        }
        else if (other.gameObject.CompareTag("wall"))
        {
            AddReward(-0.01f);
        }
        else if (other.gameObject.CompareTag("shooter"))
        {
            AddReward(maxPositiveReward);
            HandleScore();
            UpdateScore();
            pvsArenaManager.ResetEnvironment(true);
        }
        else if (other.gameObject.CompareTag("bullet"))
        {
            AddReward(maxNegativeReward);
            HandleScore();
            UpdateScore();
            pvsArenaManager.ResetEnvironment(false);
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
}
