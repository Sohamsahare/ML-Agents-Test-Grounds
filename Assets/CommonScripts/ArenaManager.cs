using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;

public class ArenaManager : MonoBehaviour
{
    public float defaultSpawnRange = 5f;
    public float defaultEnemyCount = 1f;
    public GameObject ground;
    public GameObject enemyPrefab;
    public CustomAgent agent;
    public TextMeshPro currentScoreTmp;
    public TextMeshPro prevScoreTmp;
    public TextMeshPro avgScoreTmp;
    public Material passMaterial;
    public Material failMaterial;
    public Wall[] walls;
    public Transform[] obstacles;
    public float minObstacleSpawnRange = 3f;
    public float maxObstacleSpawnRange = 10f;
    public float spawnRange { get; protected set; }
    public float enemyCount { get; protected set; }
    protected float averageScore;
    protected float currentScore;
    protected List<GameObject> enemies;
    protected Material originalMaterial;
    protected Renderer groundRenderer;

    public void Initialise()
    {
        enemies = new List<GameObject>();
        groundRenderer = ground.GetComponent<Renderer>();
        originalMaterial = groundRenderer.material;
        spawnRange = defaultSpawnRange;
        enemyCount = defaultEnemyCount;
        averageScore = 0;
        currentScore = 0;
    }

    public virtual void SetScore(float currentScore)
    {
        this.currentScore = currentScore;
        currentScoreTmp.text = "Score: " + currentScore;
    }
    public virtual void SetScore(float currentScore, int id)
    {
        // do something with id
        SetScore(currentScore);
    }
    public virtual void ReadParameters()
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
                        .GetWithDefault("enemies", defaultEnemyCount);
    }

    public virtual void MoveWalls()
    {
        for (int i = 0; i < walls.Length; i++)
        {
            Wall wall = walls[i];
            switch (wall.fourWayDirection)
            {
                case FourWayDirection.Right:
                    wall.transform.position = ground.transform.position + new Vector3(spawnRange + 2, 1, 0);
                    wall.transform.localScale = new Vector3(1, 2, 2 * (spawnRange + 2));
                    break;
                case FourWayDirection.Left:
                    wall.transform.position = ground.transform.position + new Vector3(-1 * (spawnRange + 2), 1, 0);
                    wall.transform.localScale = new Vector3(1, 2, 2 * (spawnRange + 2));
                    break;
                case FourWayDirection.Bottom:
                    wall.transform.position = ground.transform.position + new Vector3(0, 1, -1 * (spawnRange + 2));
                    wall.transform.localScale = new Vector3(2 * (spawnRange + 2), 2, 1);
                    break;
                case FourWayDirection.Top:
                    wall.transform.position = ground.transform.position + new Vector3(0, 1, spawnRange + 2);
                    wall.transform.localScale = new Vector3(2 * (spawnRange + 2), 2, 1);
                    break;
                default: break;
            }
        }
    }

    public virtual void PlaceObstacles(bool enableObstacles)
    {
        foreach (Transform obs in obstacles)
        {
            if (enableObstacles)
            {
                // Vector2 pos = Random.insideUnitCircle * Random.Range(minObstacleSpawnRange, maxObstacleSpawnRange);
                Vector2 pos = Random.insideUnitCircle * spawnRange;
                obs.position = ground.transform.position + new Vector3(pos.x, 1f, pos.y);
                obs.eulerAngles = new Vector3(0, Random.Range(0, 180f), 0);
            }
            else
            {
                obs.gameObject.SetActive(false);
            }
        }
        ground.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    public virtual void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount - enemies.Count; i++)
        {
            Vector3 pos = Utilities.RandomPositionInSpawnCircle(spawnRange);
            pos = ground.transform.position +
                    new Vector3(pos.x, 1f, pos.y);
            GameObject enemy = Instantiate(
                enemyPrefab,
                pos,
                Quaternion.identity,
                transform.parent
            );
            CustomEnemy customEnemy = enemy.GetComponent<CustomEnemy>();
            customEnemy.agent = agent;
            customEnemy.arenaManager = this;
            enemies.Add(enemy);
        }
    }
    public virtual void SpawnAgent()
    {
        // reset agent position
        Vector2 agentPos = Utilities.RandomPositionInSpawnCircle(spawnRange);
        transform.position =
            ground.transform.position +
            new Vector3(agentPos.x, 1f, agentPos.y);
    }

    public virtual void RespawnEnemy(GameObject enemy)
    {
        Vector3 pos = Utilities.RandomPositionInSpawnCircle(spawnRange);
        enemy.transform.position =
            ground.transform.position +
            new Vector3(pos.x, 1f, pos.y);
    }

    public virtual void ResetEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
            }
            Vector3 pos = Utilities.RandomPositionInSpawnCircle(spawnRange);
            pos = ground.transform.position +
                    new Vector3(pos.x, 1f, pos.y);
            enemy.transform.position = pos;
        }
    }
    public virtual bool IsEpisodeEnded(GameObject enemy, bool killedEnemy)
    {
        // disable enemy gameobject
        enemy.SetActive(false);

        bool allDead = true;
        foreach (GameObject enem in enemies)
        {
            if (enem.activeInHierarchy)
            {
                allDead = false;
            }
        }
        if (allDead)
        {
            UpdateScore(killedEnemy);
            return true;
        }
        return false;
    }
    public virtual void UpdateScore(bool killedEnemy)
    {
        prevScoreTmp.text = "Prev: " + currentScore;
        averageScore = Utilities.RoundTo((averageScore + currentScore) / 2, 2);
        avgScoreTmp.text = "Avg: " + averageScore;
        Material mat = (killedEnemy) ? passMaterial : failMaterial;
        StartCoroutine(SwapGroundMaterial(mat, 0.2f));
    }
    protected virtual IEnumerator SwapGroundMaterial(Material material, float time)
    {
        groundRenderer.material = material;
        // Wait for 'time' secs
        yield return new WaitForSeconds(time);
        groundRenderer.material = originalMaterial;
    }

    public List<GameObject> GetEnemyObjects()
    {
        return enemies;
    }
}