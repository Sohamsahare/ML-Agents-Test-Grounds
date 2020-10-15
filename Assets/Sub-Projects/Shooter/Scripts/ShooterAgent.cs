using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShooterAgent : Agent
{
    [Serializable]
    public enum EnemyMovementType
    {
        Static,
        Dynamic,
        Miscelleanous,
    }

    public EnemyMovementType enemyMovementType;
    public Gun gun;
    public GameObject ground;
    public GameObject enemyPrefab;
    public TextMeshPro currentScoreTmp;
    public TextMeshPro prevScoreTmp;
    public TextMeshPro avgScoreTmp;
    public float rotationSpeed = 150f;
    public float movementSpeed = 1f;
    public float defaultSpawnRange = 4f;
    public Material passMaterial;
    public Material failMaterial;
    float averageScore = 0;
    float currentScore = 0;
    List<GameObject> enemies;
    Material originalMaterial;
    Renderer groundRenderer;
    Rigidbody rb;
    float enemyCount;
    float spawnRange;
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        groundRenderer = ground.GetComponent<Renderer>();
        originalMaterial = groundRenderer.material;
        if (gun == null)
        {
            gun = GetComponentInChildren<Gun>();
        }
        enemies = new List<GameObject>();
        spawnRange = defaultSpawnRange;
    }
    public override void OnEpisodeBegin()
    {
        // get spawn range 
        spawnRange = Academy
                        .Instance
                        .EnvironmentParameters
                        .GetWithDefault("spawnRange", defaultSpawnRange);
        // get total enemy count
        enemyCount = Academy
                        .Instance
                        .EnvironmentParameters
                        .GetWithDefault("enemies", 5);

        // spawn any remaining enemies
        for (int i = 0; i < enemyCount - enemies.Count; i++)
        {
            Vector3 pos = RandomPositionInSpawnCircle();
            pos = ground.transform.position +
                    new Vector3(pos.x, 1f, pos.y);
            GameObject enemy = Instantiate(
                enemyPrefab,
                pos,
                Quaternion.identity,
                transform.parent
            );
            enemy.GetComponent<ShooterEnemy>().agent = this;
            enemies.Add(enemy);
        }

        // reset agent position
        Vector2 agentPos = RandomPositionInSpawnCircle();
        transform.position =
            ground.transform.position +
            new Vector3(agentPos.x, 1f, agentPos.y);

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
            }
            Vector3 pos = RandomPositionInSpawnCircle();
            pos = ground.transform.position +
                    new Vector3(pos.x, 1f, pos.y);
            enemy.transform.position = pos;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // add relative distance of closest enemy
        // so that agent is aware of avoiding/killing them
        Vector3 closestEnemyPosition = Vector3.zero;
        float distance = float.PositiveInfinity;
        // finding the closest enemy
        foreach (GameObject enemy in enemies)
        {
            float enemyDistance = Vector3.Distance(gun.muzzleTransform.position, enemy.transform.position);
            if (distance < enemyDistance)
            {
                distance = enemyDistance;
                closestEnemyPosition = enemy.transform.position;
            }
        }

        // using gun's muzzle to get position
        Vector3 closestEnemyDirection = gun.muzzleTransform.position - closestEnemyPosition;
        closestEnemyDirection = closestEnemyDirection.normalized;
        // direction vector for closest enemy
        sensor.AddObservation(closestEnemyDirection);
        // normalized distance from closest enemy
        float distObs = distance == float.PositiveInfinity ? 0 : distance / spawnRange;
        sensor.AddObservation(distObs);
        // current normalized rotation 
        sensor.AddObservation(transform.rotation.eulerAngles.normalized);

    }

    public void RespawnEnemy(GameObject enemy)
    {
        Vector3 pos = RandomPositionInSpawnCircle();
        enemy.transform.position =
            ground.transform.position +
            new Vector3(pos.x, 1f, pos.y);
    }

    private Vector2 RandomPositionInSpawnCircle()
    {
        Vector2 pos = Random.insideUnitCircle * spawnRange;
        // 50% prob of negative value
        if (Random.value >= 0.5f)
        {
            pos *= -1;
        }

        return pos;
    }

    private void FixedUpdate()
    {
        // currentScore = Mathf.Round(GetCumulativeReward() * 100) / 100;
        currentScore = Utilities.RoundTo(GetCumulativeReward(), 2);
        currentScoreTmp.text = "Score: " + currentScore;
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

    public void DamageEnemy(GameObject enemy)
    {
        // disable enemy gameobject
        enemy.SetActive(false);

        // positive reward for achieving goal
        AddReward(1f);

        CheckEpisodeStatus(true);
    }

    public void DamageAgent(GameObject enemy)
    {
        // disable enemy gameobject
        enemy.SetActive(false);

        // negative reward for being attacked
        AddReward(-1f);

        CheckEpisodeStatus(false);
    }

    void CheckEpisodeStatus(bool killedEnemy)
    {
        bool allDead = true;
        foreach (GameObject enemy in enemies)
        {
            if (enemy.activeInHierarchy)
            {
                allDead = false;
            }
        }
        // Debug.Log("Episode status " + allDead + "with score "+ GetCumulativeReward());
        if (allDead)
        {
            prevScoreTmp.text = "Prev: " + currentScore;
            averageScore = Utilities.RoundTo((averageScore + currentScore) / 2, 2);
            avgScoreTmp.text = "Avg: " + averageScore;
            EndEpisode();
            Material mat = (killedEnemy) ? passMaterial : failMaterial;
            StartCoroutine(SwapGroundMaterial(mat, 0.2f));
        }
    }

    IEnumerator SwapGroundMaterial(Material material, float time)
    {
        groundRenderer.material = material;
        yield return new WaitForSeconds(time); // Wait for 'time' secs
        groundRenderer.material = originalMaterial;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("obstacle"))
        {
            // reset agent position
            Vector2 agentPos = RandomPositionInSpawnCircle();
            transform.position =
                ground.transform.position +
                new Vector3(agentPos.x, 1f, agentPos.y);
            AddReward(-0.01f);
        }
        else if (other.gameObject.CompareTag("wall"))
        {
            AddReward(-0.05f);
        }
    }
}
