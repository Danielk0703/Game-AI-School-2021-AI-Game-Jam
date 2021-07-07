using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatArea : MonoBehaviour
{
    // List of the agents
    public List<BoatPlayerState> playerStates = new List<BoatPlayerState>();

    // GameObject array of the spawnpoints (to randomly place the two agents)
    public List<GameObject> spawnPoints = new List<GameObject>();
    public List<GameObject> spawnPointsTemp = new List<GameObject>();

    // List of environment spawnpoints (to randomly place the assets) => will be removed to the Procedural Generation
    public List<GameObject> environmentSpawnPoints = new List<GameObject>();
    public List<GameObject> environmentSpawnPointsTemp = new List<GameObject>();

    // List containing the agent prefabs
    public List<GameObject> agents = new List<GameObject>();

    // List containing the asset prefets
    public List<GameObject> assets = new List<GameObject>();
    public GameObject assetsStore;

    private void Awake()
    {
        // Reset the spawnlist for the first time
        spawnPointsTemp = new List<GameObject>(spawnPoints);
        environmentSpawnPointsTemp = new List<GameObject>(environmentSpawnPoints);
    }

    /// <summary>
    /// Place the agents on spawnpoints
    /// </summary>
    /// <param name="agent"></param>
    public void PlaceAgent(Rigidbody agent)
    {
        // For each agent, place it in a random spawnpoint
        //foreach (GameObject agent in agents)
        //{
        if (spawnPointsTemp.Count <= 0)
        {
            // Not enough spawn points
            return;
        }
        // Get a random spawnpoint index
        int randomIndex = Random.Range(0, spawnPointsTemp.Count);

        // Place agent to a random spawnpoint
        agent.transform.position = spawnPointsTemp[randomIndex].transform.position;
        //agent.transform.rotation = spawnPointsTemp[randomIndex].transform.rotation;

        // Remove this spawnpoint to the temp spawnPoints list
        spawnPointsTemp.RemoveAt(randomIndex);

        //}
    }

    /// <summary>
    /// Place the assets randomly
    /// </summary>
    public void PlaceAssets()
    {
        Destroy(assetsStore);
        assetsStore = new GameObject();
        for (int i = 0; i < environmentSpawnPoints.Count; i++)
        {
            if (environmentSpawnPointsTemp.Count <= 0)
            {
                return;
            }
            // Get a random asset
            int randomAssetIndex = Random.Range(0, assets.Count);

            // Get a random spawnpoint index
            int randomIndex = Random.Range(0, environmentSpawnPointsTemp.Count);


            // Place random asset to a random spawnpoint
            //Instantiate(original: assets[randomAssetIndex], position: environmentSpawnPointsTemp[randomIndex].transform.position,  parent: assetsStore);
            Instantiate(assets[randomAssetIndex], environmentSpawnPointsTemp[randomIndex].transform.position, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f), assetsStore.transform);

            // Remove this spawnpoint to the temp spawnPoints list
            environmentSpawnPointsTemp.RemoveAt(randomIndex);
        }
    }

    /// <summary>
    /// When a bullet killed the enemy
    /// </summary>
    /// <param name="touchedTeam">The team killed</param>
    public void CannonballKilled(BoatAgent.Team touchedTeam)
    {
        // End of the episode
        // Reset the spawnlist (we can't just say spawnPointsTemp = spawnPoints because it will reference the original list
        spawnPointsTemp = new List<GameObject>(spawnPoints);
        environmentSpawnPointsTemp = new List<GameObject>(environmentSpawnPoints);

        foreach (var ps in playerStates)
        {
            if (ps.agentScript.team == touchedTeam)
            {
                ps.agentScript.AddReward(-1);
            }
            else
            {
                ps.agentScript.AddReward(1f + ps.agentScript.timePenalty);
            }
            ps.agentScript.EndEpisode();  //all agents need to be reset
        }

    }

    /// <summary>
    /// Distribute Damage Reward: if you touched an enemy you'll receive +0.25 and he'll receive -0.25
    /// </summary>
    /// <param name="touchedTeam"></param>
    public void distributeDamageReward(BoatAgent.Team touchedTeam)
    {
        foreach (var ps in playerStates)
        {

            if (ps.agentScript.team == touchedTeam)
            {
                ps.agentScript.AddReward(-0.25f);
            }
            else
            {
                ps.agentScript.AddReward(0.25f);
            }
        }
    }

}
