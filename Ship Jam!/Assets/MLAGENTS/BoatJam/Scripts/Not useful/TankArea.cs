using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TankArea : MonoBehaviour
{
    // List of the agents
    public List<TankPlayerState> playerStates = new List<TankPlayerState>();

    // GameObject array of the spawnpoints
    public List<GameObject> spawnPoints = new List<GameObject>();
    public List<GameObject> spawnPointsTemp = new List<GameObject>();

    // List of environment spawnpoints
    public List<GameObject> environmentSpawnPoints = new List<GameObject>();
    public List<GameObject> environmentSpawnPointsTemp = new List<GameObject>();

    // List of agents
    public List<GameObject> agents = new List<GameObject>();

    // List of assets
    public List<GameObject> assets = new List<GameObject>();

    // GameObject
    public GameObject assetsStore;

    private void Awake()
    {
        // Reset the spawnlist for the first time
        spawnPointsTemp = new List<GameObject>(spawnPoints);
        environmentSpawnPointsTemp = new List<GameObject>(environmentSpawnPoints);
    }

    // Place the agents
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

            // Remove this spawnpoint to the temp spawnPoints list
            spawnPointsTemp.RemoveAt(randomIndex);

        //}
    }

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


    // When a bullet killed the ennemy
    // TODO: Ok for 2 players states but what about more, how to check which bullet killed which?
    public void BulletKilled(TankAgent.Team touchedTeam)
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
}
