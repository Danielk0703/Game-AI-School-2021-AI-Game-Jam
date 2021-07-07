using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Extensions.Sensors;

public class TankAgent : Agent
{
    public enum Team
    {
        Green = 0,
        Blue = 1,
    }

    [HideInInspector]
    public Team team;

    public Rigidbody m_AgentRb;

    public float m_Speed;
    public float m_RotationSpeed;

    public bool m_PossibleShoot;
    public Transform m_FireTransform;
    public float m_NextFire;
    [HideInInspector]
    public float m_ShootForce;
    public Rigidbody m_Shell; // Prefab of the shell.

    public float m_Health;

    [HideInInspector]
    public float m_Penalty;
    [HideInInspector]
    public float timePenalty;

    public TankArea m_Area;

    int m_PlayerIndex;

    public TankHealth m_TankHealthSystem;

    // In order that all tanks face the same thing in the beginning
    public Transform m_CenterOfMap;

    BehaviorParameters m_BehaviorParameters;

    public void shootShell(float m_ShootForce)
    {
        // Create an instance of the shell and store a reference to it's rigidbody.
        Rigidbody shellInstance =
            Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        shellInstance.gameObject.GetComponent<ShellExplosion>().m_AgentLauncher = this;
        // Set the shell's velocity to the launch force (discrete) in the fire position's forward direction.
        shellInstance.velocity = m_ShootForce * m_FireTransform.forward;

        // Change the clip to the firing clip and play it.
        //m_ShootingAudio.clip = m_FireClip;
        //m_ShootingAudio.Play();
    }

    public void beingKilled()
    {
        m_Area.BulletKilled(team);
    }

    public override void Initialize()
    {
        // Calculate the penality rate (this push our agent to meet faster its goal)
        m_Penalty = 1f / MaxStep;
         
        // Get the behavior parameters (to get the team)
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        // Place the agent based on its team
        if (m_BehaviorParameters.TeamId == (int)Team.Green)
        {
            team = Team.Green;
        }
        else if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
        }

        // SnowballFightPlayerState
        var playerState = new TankPlayerState
        {
            agentRb = m_AgentRb,
            agentScript = this,
        };

        m_TankHealthSystem = gameObject.GetComponent<TankHealth>();

        // Add this agent state to the playerStates List
        m_Area.playerStates.Add(playerState);
        m_PlayerIndex = m_Area.playerStates.IndexOf(playerState);
        playerState.playerIndex = m_PlayerIndex;
    }

    public override void OnEpisodeBegin()
    {
        // Remove all the shell?
        GameObject[]  shells = GameObject.FindGameObjectsWithTag("shell");

        foreach (GameObject shell in shells)
        {
            Destroy(shell);
        }

        timePenalty = 0;

        // Reset the health
        m_TankHealthSystem.ResetHealth();

        if (m_PlayerIndex == 0)
        {
            m_Area.PlaceAssets();
        }
        // Place the agent
        m_Area.PlaceAgent(m_AgentRb);
        transform.LookAt(m_CenterOfMap);

        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Bool can I shoot or not (to avoid spamming)
        sensor.AddObservation(m_PossibleShoot);

        // Current Health
        sensor.AddObservation(m_TankHealthSystem.GetHealthStatus());

        // Canon rotation
        sensor.AddObservation(m_AgentRb.transform.localRotation.y);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var move = act[0];
        var rotate = act[1];
        var shoot = act[2];

        if (Time.time > m_NextFire)
        {
            m_PossibleShoot = true;
            switch(move)
            {
                // Move up/down
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
            }
            switch (rotate)
            {
                // Rotation left/right
                case 1:
                    rotateDir = transform.up * 1f;
                    break;
                case 2:
                    rotateDir = transform.up * -1f;
                    break;
            }
            switch (shoot)
            {
                // Small shoot (15)
                case 1:
                    m_ShootForce = 15f;
                    // Update the time when our player can fire next
                    m_NextFire = Time.time + 1.5f;
                    shootShell(m_ShootForce);
                    m_PossibleShoot = false;
                    break;
                
                /* Middle shoot (20)
                case 2:
                    m_ShootForce = 20f;
                    // Update the time when our player can fire next
                    m_NextFire = Time.time + 1.5f;
                    shootShell(m_ShootForce);
                    m_PossibleShoot = false;
                    break;

                // Fast shoot (25)
                case 3:
                    m_ShootForce = 25f;
                    // Update the time when our player can fire next
                    m_NextFire = Time.time + 1.5f;
                    shootShell(m_ShootForce);
                    m_PossibleShoot = false;
                    break;*/
            }
        }
        else
        {
            switch (move)
            {
                // Move up/down
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;

            }
            switch(rotate)
            {
                // Rotation left/right
                case 1:
                    rotateDir = transform.up * 1f;
                    break;
                case 2:
                    rotateDir = transform.up * -1f;
                    break;
            }
        }

        transform.Rotate(rotateDir, Time.deltaTime * m_RotationSpeed);
        
        m_AgentRb.AddForce(dirToGo * m_Speed, ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut.Clear();

        //forward
        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 2;
        }
        // rotate
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[1] = 2;
        }
        //shoot
        if (Input.GetKey(KeyCode.G))
        {
            discreteActionsOut[2] = 1;
        }
        /*if (Input.GetKey(KeyCode.H))
        {
            discreteActionsOut[2] = 2;
        }
        if (Input.GetKey(KeyCode.J))
        {
            discreteActionsOut[2] = 3;
        }*/
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Add t penality
        timePenalty -= m_Penalty;

        // Move the agent
        MoveAgent(actions.DiscreteActions);

    }
}
