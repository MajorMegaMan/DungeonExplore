using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Entity/SpawnSettings")]
public class SpawnControllerSettings : ScriptableObject
{
    [Header("Wave control")]

    [Tooltip("The desired number of agents to be spawned, without going over the spawner limit.")]
    public int desiredWaveCount = 5;

    [Tooltip("The maximum number of agents that this spawner allows to exist.")]
    public int maximumEnemyPopulation = 5;

    [Tooltip("If the maximum limit has already been reached. How many enemies are still allowed to spawn from the wave.")]
    public int allowableOverSpawnLimit = 2;

    [Tooltip("Allowable Over spawn Limit will not go over this. The absolute Maximum number of agents that is allowed to spawn")]
    public int absoluteLimit = 12;

    [Header("Timers")]
    [Tooltip("The time it takes to initiate a wave of enemies from this spawner.")]
    public float waveSeperationTime = 5.0f;

    [Tooltip("The time it takes from a single enemy spawn to the next enemy spawn during a wave.")]
    public float miniWaveTime = 0.01f;

    public int CalculateAmountToSpawn(int currentActiveAgentCount)
    {
        int amountToAdd = desiredWaveCount;
        
        int absoluteRoof = absoluteLimit - currentActiveAgentCount;
        int roof = maximumEnemyPopulation + allowableOverSpawnLimit - currentActiveAgentCount;
        int min = allowableOverSpawnLimit;
        if (roof >= min)
        {
            amountToAdd = Mathf.Clamp(amountToAdd, min, roof);
        }
        else
        {
            amountToAdd = min;
        }

        amountToAdd = Mathf.Min(amountToAdd, absoluteRoof);

        return amountToAdd;
    }

    private void OnValidate()
    {
        if (desiredWaveCount < 0)
        {
            desiredWaveCount = 0;
        }

        if (maximumEnemyPopulation < 0)
        {
            maximumEnemyPopulation = 0;
        }

        if (allowableOverSpawnLimit < 0)
        {
            allowableOverSpawnLimit = 0;
        }

        if (miniWaveTime < 0.0001f)
        {
            miniWaveTime = 0.0001f;
        }
    }
}
