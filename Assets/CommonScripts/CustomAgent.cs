using UnityEngine;
using Unity.MLAgents;

public class CustomAgent : Agent
{
    public float rotationSpeed = 150f;
    public float movementSpeed = 1f;
    public float maxPositiveReward = 1;
    public float maxNegativeReward = -1;
    public float currentScore { get; private set; }
    public ArenaManager arenaManager;
    protected Rigidbody rb;

    public override void Initialize()
    {
        PreInitialize();

        rb = GetComponent<Rigidbody>();
        arenaManager.Initialise();

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
        // spawn any remaining enemies
        SpawnEnemies();
        // Set random agent position
        SpawnAgent();
        // if agents spawned already, then reset their positions
        // at start of each episode
        ResetEnemies();

        PostEpisodeBegin();
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
        // positive reward for achieving goal
        AddReward(maxPositiveReward);
        currentScore = Utilities.RoundTo(GetCumulativeReward(), 2);
        arenaManager.SetScore(currentScore);

        if (arenaManager.IsEpisodeEnded(enemy, true))
        {
            EndEpisode();
        }
    }

    public void PunishAgent(GameObject enemy)
    {
        // negative reward for being attacked
        AddReward(maxNegativeReward);
        currentScore = Utilities.RoundTo(GetCumulativeReward(), 2);
        arenaManager.SetScore(currentScore);

        if (arenaManager.IsEpisodeEnded(enemy, false))
        {
            EndEpisode();
        }
    }

    protected virtual void FixedUpdate()
    {
        currentScore = Utilities.RoundTo(GetCumulativeReward(), 2);
        arenaManager.SetScore(currentScore);
    }
}