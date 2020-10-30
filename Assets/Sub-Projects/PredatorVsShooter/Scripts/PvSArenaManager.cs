using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using UnityEngine;

public class PvSArenaManager : ArenaManager
{
    public PvSPredator predator;
    public PvSShooter shooter;
    public bool enableObstacles;
    public TextMeshPro winTrackerTmp;
    public float defaultSpeed = 0.1f;
    public float speed { get; protected set; }
    int predatorWinCount = 0;
    int shooterWinCount = 0;

    // reset env new function at the end of episode
    public void ResetEnvironment(bool predatorWin)
    {
        predator.EndEpisode();
        shooter.EndEpisodeAndScore(predatorWin);

        StatsRecorder stats = Academy.Instance.StatsRecorder;
        if (predatorWin)
        {
            predatorWinCount++;
            stats.Add("PredatorWin", 1);
        }
        else
        {
            shooterWinCount++;
            stats.Add("ShooterWin", 1);
        }

        winTrackerTmp.text = predatorWinCount + " - " + shooterWinCount;
        
        // red for pink, green for shooter
        Material mat = (!predatorWin) ? passMaterial : failMaterial;
        StartCoroutine(SwapGroundMaterial(mat, 0.2f));
    }

    // add new params for two agents
    // initialise them (readparameters())
    public override void ReadParameters()
    {
        // get spawn range 
        spawnRange = Academy
                        .Instance
                        .EnvironmentParameters
                        .GetWithDefault("spawnRange", defaultSpawnRange);
        // get spawn range 
        speed = Academy
                        .Instance
                        .EnvironmentParameters
                        .GetWithDefault("speed", defaultSpeed);
    }
    // overload for spawnagent with object/prefab
    public override void SpawnAgent()
    {
        // reset predator position
        Vector2 agentPos = Utilities.RandomPositionInSpawnCircle(spawnRange);
        predator.transform.position =
            ground.transform.position +
            new Vector3(agentPos.x, 1f, agentPos.y);
        // reset shooter position
        agentPos = Utilities.RandomPositionInSpawnCircle(spawnRange);
        shooter.transform.position =
            ground.transform.position +
            new Vector3(agentPos.x, 1f, agentPos.y);
    }
    public override void PlaceObstacles(bool enableObstacles)
    {
        base.PlaceObstacles(this.enableObstacles);
    }

    public override void SpawnEnemies() { }
}