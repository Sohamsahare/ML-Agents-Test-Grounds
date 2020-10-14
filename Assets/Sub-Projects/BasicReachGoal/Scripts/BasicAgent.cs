using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class BasicAgent : Agent
{
    public GameObject ground;
    public GameObject goal;
    public float movementSpeed = 10f;
    public float spawnRange = 4f;
    public Material goalReachedMaterial;
    Rigidbody rb;

    Renderer groundRenderer;
    Material originalMaterial;

    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     base.CollectObservations(sensor);
    // }

    public override void Heuristic(float[] actionsOut)
    {
        // Horizontal axis
        actionsOut[0] = 0;
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }

        // Vertical axis
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

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        BasicGoal basicGoal = goal.GetComponent<BasicGoal>();
        basicGoal.agent = this;

        groundRenderer = ground.GetComponent<Renderer>();
        originalMaterial = groundRenderer.material;
    }

    public void ReachedGoal()
    {
        AddReward(5f);
        EndEpisode();
        StartCoroutine(SwapGroundMaterial(goalReachedMaterial, 0.2f));
        Debug.Log("Episode Ended with score " + GetCumulativeReward());
    }

    IEnumerator SwapGroundMaterial(Material material, float time)
    {
        groundRenderer.material = material;
        yield return new WaitForSeconds(time); // Wait for 'time' secs
        groundRenderer.material = originalMaterial;
    }

    public override void OnEpisodeBegin()
    {
        bool spawnLocationsFound = false;
        Vector2 random = Vector2.zero;
        Vector2 random2 = Vector2.zero;
        int tries = 0;

        // if overlapping recalculate spawn positions
        while (!spawnLocationsFound)
        {
            // spawn random agent within boundaries
            random = Random.insideUnitCircle * spawnRange;
            // spawn random goal within boundaries
            random2 = Random.insideUnitCircle * spawnRange;
            // check if they are not overlapping
            spawnLocationsFound = Vector2.Distance(random, random2) > 1;
            ++tries;
        }

        if(tries != 1){
            Debug.Log("Got position after tries -> " + tries);
        }

        transform.position = ground.transform.position + new Vector3(random.x, 1f, random.y);
        goal.transform.position = ground.transform.position + new Vector3(random2.x, 1f, random2.y);
        rb.velocity = Vector3.zero;
        base.OnEpisodeBegin();
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Debug.DrawRay(transform.position, transform.forward, Color.yellow, 0.2f);
        switch (vectorAction[0])
        {
            case 0:
                // do nothing
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
                break;
            case 1:
                // rotate left relative to agent
                Vector3 left = transform.up * -1;
                // rb.AddForce(left, ForceMode.VelocityChange);
                transform.Rotate(left, Time.fixedDeltaTime * 200f);
                break;
            case 2:
                // rotate right relative to agent
                Vector3 right = transform.up;
                // rb.AddForce(right, ForceMode.VelocityChange);
                transform.Rotate(right, Time.fixedDeltaTime * 200f);
                break;
            default:
                // invalid scenario
                break;
        }

        // add negative reward for existing
        AddReward(-1f / MaxStep);
    }

    public void PunishHittingWall()
    {
        AddReward(-0.1f);
    }


}
