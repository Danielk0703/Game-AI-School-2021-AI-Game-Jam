using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using System.Linq;

public class BoatArea : MonoBehaviour
{
    // List of the agents
    public List<BoatPlayerState> playerStates = new List<BoatPlayerState>();

    // GameObject array of the spawnpoints (to randomly place the two agents)
    public List<GameObject> spawnPoints = new List<GameObject>();
    public List<GameObject> spawnPointsTemp = new List<GameObject>();

    [SerializeField] int team0Size = 4;
    public int Team0Size { get { return team0Size; } }
    [SerializeField] int team1Size = 4;
    public int Team1Size { get { return team1Size; } }
    [SerializeField] int maxSteps = 10000;
    public int MaxSteps { get { return maxSteps; } }
    int currentStep = 0;
    public List<BoatAgent> team0Agents = new List<BoatAgent>();
    public List<BoatAgent> team1Agents = new List<BoatAgent>();
    SimpleMultiAgentGroup team0AgentGroup = new SimpleMultiAgentGroup();
    SimpleMultiAgentGroup team1AgentGroup = new SimpleMultiAgentGroup();
    // List containing the agent prefabs
    public GameObject agent;

    public int currentDifficulty = 1;
    // public bool enableDifficulty1 = false;
    // public bool enableDifficulty2 = false;
    // public bool enableDifficulty3 = false;
    // public bool enableDifficulty4 = false;
    // private void OnValidate()
    // {
    //     if (enableDifficulty1)
    //     {
    //         transform.Find("IslandScene").gameObject.SetActive(false);
    //         transform.Find("Boundaries").localScale = Vector3.one * 0.75f;
    //         transform.Find("Team 0 Spawn Area").localScale = Vector3.one * 0.75f;
    //         transform.Find("Team 1 Spawn Area").localScale = Vector3.one * 0.75f;
    //         transform.Find("Team 0 Spawn Area").position = new Vector3(12, 0.4f, 0.9f);
    //         transform.Find("Team 1 Spawn Area").position = new Vector3(-14.4f, 0.4f, 0.9f);
    //         enableDifficulty1 = false;
    //     }
    //     else if (enableDifficulty2)
    //     {
    //         transform.Find("IslandScene").gameObject.SetActive(false);
    //         transform.Find("Boundaries").localScale = Vector3.one;
    //         transform.Find("Team 0 Spawn Area").localScale = Vector3.one;
    //         transform.Find("Team 1 Spawn Area").localScale = Vector3.one;
    //         transform.Find("Team 0 Spawn Area").position = new Vector3(18, 0.4f, 0.9f);
    //         transform.Find("Team 1 Spawn Area").position = new Vector3(-22, 0.4f, 0.9f);
    //         enableDifficulty2 = false;
    //     }
    //     else if (enableDifficulty3)
    //     {
    //         transform.Find("IslandScene").gameObject.SetActive(true);
    //         GetComponentInChildren<IslandGenerator>().maxIslandCount = 5;
    //         GetComponentInChildren<IslandGenerator>().extraDistanceBetweenIslands = 5f;
    //         enableDifficulty3 = false;
    //     }
    //     else if (enableDifficulty4)
    //     {
    //         GetComponentInChildren<IslandGenerator>().maxIslandCount = 13;
    //         GetComponentInChildren<IslandGenerator>().extraDistanceBetweenIslands = 1f;
    //         enableDifficulty4 = false;
    //     }
    // }
    private void Awake()
    {
        Initialize();
    }
    private void CreateWarriorForTeam(int teamId)
    {
        agent.GetComponent<BehaviorParameters>().TeamId = teamId;
        GameObject newWarrior = GameObject.Instantiate(agent, transform, false);
        Vector3 rotation = GetRandomRotation();
        Vector3 position = GetRandomPosition(teamId);
        newWarrior.transform.localPosition = position;
        newWarrior.transform.localEulerAngles = rotation;
        newWarrior.gameObject.SetActive(true);
        newWarrior.gameObject.tag = "Team " + (teamId).ToString();

        if (teamId == 0)
        {
            team0Agents.Add(newWarrior.GetComponent<BoatAgent>());
            team0AgentGroup.RegisterAgent(team0Agents.Last());
        }
        else
        {
            team1Agents.Add(newWarrior.GetComponent<BoatAgent>());
            team1AgentGroup.RegisterAgent(team1Agents.Last());
        }
    }
    private void Initialize()
    {
        // remove from teamXAgentGroup (they are removed when they are disabled)
        foreach (BoatAgent w in team0Agents)
        {
            w.gameObject.SetActive(false);
            GameObject.DestroyImmediate(w);
        }
        foreach (BoatAgent w in team1Agents)
        {
            w.gameObject.SetActive(false);
            GameObject.DestroyImmediate(w);
        }
        // clear lists
        team0Agents.Clear();
        team1Agents.Clear();

        for (int i = 0; i < team0Size; i++)
        {
            CreateWarriorForTeam((int)Team.Green);
            team0Agents.Last().name += string.Format(" {0}", i + 1);
        }
        for (int i = 0; i < team1Size; i++)
        {
            CreateWarriorForTeam((int)Team.Blue);
            team1Agents.Last().name += string.Format(" {0}", i + 1);
        }

        foreach (BoatAgent agent in team0Agents)
            agent.enabled = true;
        foreach (BoatAgent agent in team1Agents)
            agent.enabled = true;

        try
        {
            agent.SetActive(false);
        }
        catch
        {

        }
    }

    Vector3 GetRandomRotation()
    {
        float rotationAngle = Random.Range(0f, 1f) * 360f;
        return new Vector3(0f, rotationAngle, 0f);
    }
    Vector3 GetRandomPosition(int teamId)
    {
        GameObject spawnArea = spawnPoints[teamId];
        Bounds area = spawnArea.GetComponent<Collider>().bounds;
        return new Vector3(Random.Range(area.min.x, area.max.x), 0f, Random.Range(area.min.z, area.max.z));
    }

    private bool TeamDead(int teamId)
    {
        return teamId == 0 ? team0Agents.All(b => !b.IsAlive || !b.gameObject.activeSelf) : team1Agents.All(b => !b.IsAlive || !b.gameObject.activeSelf);
    }
    private int CheckWin()
    {
        if (team0Agents.FindAll(b => b.IsDead).Count < team1Agents.FindAll(b => b.IsDead).Count)
        {
            return 0;
        }
        else if (team0Agents.FindAll(b => b.IsDead).Count > team1Agents.FindAll(b => b.IsDead).Count)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }
    private void FixedUpdate()
    {
        Bounds arenaBounds = new Bounds(transform.position, new Vector3(60, 1, 60));
        foreach (BoatAgent agent in team0Agents)
        {
            if (!arenaBounds.Contains(agent.transform.position))
            {
                agent.transform.position = GetRandomPosition(0);
            }
        }
        foreach (BoatAgent agent in team1Agents)
        {
            if (!arenaBounds.Contains(agent.transform.position))
            {
                agent.transform.position = GetRandomPosition(1);
            }
        }


        currentStep += 1;
        if (currentStep >= MaxSteps)
        {
            team0AgentGroup.GroupEpisodeInterrupted();
            team1AgentGroup.GroupEpisodeInterrupted();
            ResetScene();
            return;
        }
    }
    public void Update()
    {
        if (TeamDead(0))
            ApplyRewards(1);
        else if (TeamDead(1))
            ApplyRewards(0);
    }
    void ApplyRewards(int winningTeam)
    {
        SimpleMultiAgentGroup winningGroup = winningTeam == 1 ? team1AgentGroup : team0AgentGroup;
        SimpleMultiAgentGroup losingGroup = winningTeam == 1 ? team0AgentGroup : team1AgentGroup;
        List<BoatAgent> winningAgents = winningTeam == 1 ? team1Agents : team0Agents;
        List<BoatAgent> losingAgents = winningTeam == 1 ? team0Agents : team1Agents;
        int winningTeamSize = winningTeam == 1 ? team1Size : team0Size;

        winningGroup.SetGroupReward(1f - currentStep / maxSteps);
        losingGroup.SetGroupReward(-1f);

        team0AgentGroup.EndGroupEpisode();
        team1AgentGroup.EndGroupEpisode();
        Debug.Log(string.Format("Team {0} win", winningTeam));
        ResetScene();
    }
    void ResetScene()
    {
        currentStep = 0;
        if (GetComponentInChildren<IslandGenerator>(true).enabled)
            GetComponentInChildren<IslandGenerator>(true).Generate();

        foreach (BoatAgent agent in team0Agents)
        {
            agent.transform.localEulerAngles = GetRandomRotation();
            agent.transform.localPosition = GetRandomPosition(0);
        }
        foreach (BoatAgent agent in team1Agents)
        {
            agent.transform.localEulerAngles = GetRandomRotation();
            agent.transform.localPosition = GetRandomPosition(1);
        }

        foreach (BoatAgent agent in team0Agents)
        {
            agent.gameObject.SetActive(true);
            team0AgentGroup.RegisterAgent(agent);
        }

        foreach (BoatAgent agent in team1Agents)
        {
            agent.gameObject.SetActive(true);
            team1AgentGroup.RegisterAgent(agent);
        }

        float level = Academy.Instance.StepCount + 13859801;
        if (currentDifficulty == 1 && level >= 10000000)
        {
            print("Reached level " + level);
            transform.Find("Boundaries").localScale = Vector3.one;
            transform.Find("Team 0 Spawn Area").localScale = Vector3.one;
            transform.Find("Team 1 Spawn Area").localScale = Vector3.one;
            transform.Find("Team 0 Spawn Area").position = new Vector3(18, 0.4f, 0.9f);
            transform.Find("Team 1 Spawn Area").position = new Vector3(-22, 0.4f, 0.9f);
            currentDifficulty++;
        }
        else if (currentDifficulty == 2 && level >= 16000000)
        {
            print("Reached level " + level);
            transform.Find("IslandScene").gameObject.SetActive(true);
            GetComponentInChildren<IslandGenerator>().maxIslandCount = 5;
            GetComponentInChildren<IslandGenerator>().extraDistanceBetweenIslands = 5f;
            currentDifficulty++;
        }
        else if (currentDifficulty == 3 && level >= 18000000)
        {
            print("Reached level " + level);
            GetComponentInChildren<IslandGenerator>().maxIslandCount = 13;
            GetComponentInChildren<IslandGenerator>().extraDistanceBetweenIslands = 1f;
            currentDifficulty++;
        }
    }
}
