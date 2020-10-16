using UnityEngine;
using Unity.MLAgents;

public class CustomAgent : Agent
{
    public float rotationSpeed = 150f;
    public float movementSpeed = 1f;
    public float maxPositiveReward = 1;
    public float maxNegativeReward = -1;
    public bool enableObstacles = false;
    public float currentScore { get; private set; }
    public ArenaManager arenaManager;
    protected Rigidbody rb;
    protected StatsRecorder stats;

    public override void Initialize()
    {
        PreInitialize();

        rb = GetComponent<Rigidbody>();
        arenaManager.Initialise();
        stats = Academy.Instance.StatsRecorder;

        PostInitialize();
    }

    protected virtual void PreInitialize() { }
    protected virtual void PostInitialize() { }

    public override void OnEpisodeBegin()
    {
        PreEpisodeBegin();

        // read parameters from config file
        InitialiseParameters();
        // everything that needs to be done before spawning
        // such as setting up the arena
        PreSpawnLogic();
        // place obstacles if enabled
        PlaceObstacles();
        // spawn any remaining enemies
        SpawnEnemies();
        // Set random agent position
        SpawnAgent();
        // if agents spawned already, then reset their positions
        // at start of each episode
        ResetEnemies();

        PostEpisodeBegin();
    }

    protected virtual void PlaceObstacles()
    {
        arenaManager.PlaceObstacles(enableObstacles);
    }

    protected virtual void PreEpisodeBegin() { }
    protected virtual void PreSpawnLogic() { }
    protected virtual void PostEpisodeBegin() { }

    public virtual void SpawnAgent()
    {
        arenaManager.SpawnAgent();
    }
    public virtual void SpawnEnemies()
    {
        arenaManager.SpawnEnemies();
    }
    public virtual void ResetEnemies()
    {
        arenaManager.ResetEnemies();
    }
    public virtual void InitialiseParameters()
    {
        arenaManager.ReadParameters();
    }

    public void RewardAgent(GameObject enemy)
    {
        PreAgentRewarded();
        // positive reward for achieving goal
        AddReward(maxPositiveReward);
        currentScore = Utilities.RoundTo(GetCumulativeReward(), 2);
        arenaManager.SetScore(currentScore);
        PostAgentRewarded();
        if (arenaManager.IsEpisodeEnded(enemy, true))
        {
            PreEpisodeEnded();
            EndEpisode();
        }
    }

    protected virtual void PreAgentRewarded() { }
    protected virtual void PostAgentRewarded() { }

    public void PunishAgent(GameObject enemy)
    {
        PreAgentPunished();
        // negative reward for being attacked
        AddReward(maxNegativeReward);
        currentScore = Utilities.RoundTo(GetCumulativeReward(), 2);
        arenaManager.SetScore(currentScore);
        PostAgentPunished();

        if (arenaManager.IsEpisodeEnded(enemy, false))
        {
            PreEpisodeEnded();
            EndEpisode();
        }
    }
    protected virtual void PreAgentPunished() { }
    protected virtual void PostAgentPunished() { }


    protected virtual void PreEpisodeEnded() { }
    protected virtual void FixedUpdate()
    {
        currentScore = Utilities.RoundTo(GetCumulativeReward(), 2);
        arenaManager.SetScore(currentScore);
    }
}