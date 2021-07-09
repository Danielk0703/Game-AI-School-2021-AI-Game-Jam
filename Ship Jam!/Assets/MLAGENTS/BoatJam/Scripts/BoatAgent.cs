using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Extensions.Sensors;

public class BoatAgent : Agent
{

    /// <summary>
    /// What team we are
    /// </summary>
    public enum Team
    {
        Green = 0,
        Blue = 1,
    }

    [HideInInspector]
    public Team team;

    public Rigidbody m_AgentRb;
    public Rigidbody m_EnemyAgentRb;

    public float m_Speed;
    public float m_RotationSpeed;

    public bool m_PossibleShoot;
    public Transform m_LeftFireTransform;
    public Transform m_RightFireTransform;
    public float m_NextFire;

    [HideInInspector]
    public float m_ShootForce;
    public Rigidbody m_Cannonball; // Prefab of the cannonball.

    public float m_Health;

    [HideInInspector]
    public float m_Penalty;
    [HideInInspector]
    public float timePenalty;

    public BoatArea m_Area;

    int m_PlayerIndex;

    public BoatHealth m_BoatHealthSystem;

    // In order that all boats face the same thing in the beginning
    public Transform m_CenterOfMap;

    BehaviorParameters m_BehaviorParameters;

    public bool useContinuousActions = false;

    public AudioSource fireSound;

    /// <summary>
    /// Shoot the cannonball
    /// </summary>
    /// <param name="m_ShootForce">The force applied to the cannonball</param>
    /// <param name="side">The side shot (left or right)</param>
    public void shootCannonball(float m_ShootForce, string side)
    {
        if (side == "left")
        {
            // Create an instance of the cannonball and store a reference to it's rigidbody.
            Rigidbody cannonballInstance =
                Instantiate(m_Cannonball, m_LeftFireTransform.position, m_LeftFireTransform.rotation) as Rigidbody;

            cannonballInstance.gameObject.GetComponent<CannonballExplosion>().m_AgentLauncher = this;

            // Set the shell's velocity to the launch force (discrete) in the fire position's forward direction.
            cannonballInstance.velocity = m_ShootForce * m_LeftFireTransform.forward;
        }
        else
        {
            // Create an instance of the cannonball and store a reference to it's rigidbody.
            Rigidbody cannonballInstance =
                Instantiate(m_Cannonball, m_RightFireTransform.position, m_RightFireTransform.rotation) as Rigidbody;
            cannonballInstance.gameObject.GetComponent<CannonballExplosion>().m_AgentLauncher = this;

            // Set the shell's velocity to the launch force (discrete) in the fire position's forward direction.
            cannonballInstance.velocity = m_ShootForce * m_RightFireTransform.forward;
        }
        fireSound.Play();
        AddReward(-5f);

        // I let the audio if we have time

        // Change the clip to the firing clip and play it.
        //m_ShootingAudio.clip = m_FireClip;
        //m_ShootingAudio.Play();
        }

    /// <summary>
    /// Call Area CannonballKilled() function when someone is dead
    /// </summary>
    public void beingKilled()
    {
        m_Area.CannonballKilled(team);
    }

    /// <summary>
    /// Call Area distributeDamageReward() function when someone is touched by a cannonball
    /// </summary>
    public void distributeDamageReward()
    {
        m_Area.distributeDamageReward(team);
    }

    /// <summary>
    /// Initialize the environment when we start to play
    /// </summary>
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

        // BoatPlayerState
        var playerState = new BoatPlayerState
        {
            agentRb = m_AgentRb,
            agentScript = this,
        };

        m_BoatHealthSystem = gameObject.GetComponent<BoatHealth>();

        // Add this agent state to the playerStates List
        m_Area.playerStates.Add(playerState);
        m_PlayerIndex = m_Area.playerStates.IndexOf(playerState);
        playerState.playerIndex = m_PlayerIndex;
    }

    /// <summary>
    /// Reset the environment when we start a new episode
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Remove all the shell?
        GameObject[] shells = GameObject.FindGameObjectsWithTag("cannonball");

        foreach (GameObject shell in shells)
        {
            Destroy(shell);
        }

        timePenalty = 0;

        // Reset the health
        m_BoatHealthSystem.ResetHealth();

        if (m_PlayerIndex == 0)
        {
            Debug.LogError("OnEpisodeBegin " + this.name + this.transform.parent.name, this.gameObject);
            m_Area.PlaceAssets();
        }
        // Place the agent
        m_Area.PlaceAgent(m_AgentRb);
        //transform.LookAt(m_CenterOfMap);

        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Collect the vector observations: if we can shoot (bool), our current health, our current rotation (to help navigate)
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Bool can I shoot or not (to avoid spamming)
        sensor.AddObservation(m_PossibleShoot);

        // Current Health
        sensor.AddObservation(m_BoatHealthSystem.GetHealthStatus());

        // TODO: Left and right cannon rotation?
        // Left Canon rotation ?
        Vector3 scaleVec = new Vector3 (1/m_Area.mapBoundsCollider.bounds.size.x, 1, 1/m_Area.mapBoundsCollider.bounds.size.z);
        Vector3 myNormalizedPos = Vector3.Scale(m_AgentRb.transform.localPosition, scaleVec);
        Vector3 enemyNormalizedPos = Vector3.Scale(m_EnemyAgentRb.transform.localPosition, scaleVec);
        sensor.AddObservation(myNormalizedPos);
        sensor.AddObservation(m_AgentRb.transform.rotation);
        sensor.AddObservation(enemyNormalizedPos);

        // Left cannon rotation
        //sensor.AddObservation(TransformDirection(m_LeftFireTransform.z);
        //Debug.Log("m_LeftFireTransform" + m_LeftFireTransform.localRotation.y);
        // Right Canon rotation
    }
    
    /// <summary>
    /// If it's before spamming time we mask the shooting action
    /// </summary>
    /// <param name="actionMask"></param>
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (Time.time > m_NextFire)
        {
            if(useContinuousActions)
            {
                actionMask.SetActionEnabled(1, 0, false);
                actionMask.SetActionEnabled(1, 1, false);
            }
            else
            {
                actionMask.SetActionEnabled(2, 0, false);
                actionMask.SetActionEnabled(2, 1, false);
            }
        }
    }

    public void AgentActionConituous(ActionBuffers vectorAction)
    {
        const float speed = 5f ;
        const float rotateSpeed = 180f;
        float moveVal = Mathf.Clamp01((0.5f + vectorAction.ContinuousActions[0]) /1.5f) * speed ;
        transform.localPosition += transform.forward * moveVal * Time.deltaTime;
        AddReward(moveVal/(1f + Vector3.Distance(m_AgentRb.transform.localPosition, m_EnemyAgentRb.transform.localPosition)));
        transform.Rotate(transform.up, vectorAction.ContinuousActions[1] * rotateSpeed  * Time.deltaTime);
        if(Time.time > m_NextFire)
        {
            m_PossibleShoot = true;
            if(Mathf.Abs(vectorAction.ContinuousActions[2]) > 0.3f)
            {
                // Left Small shoot (20)
                m_ShootForce = 20f;
                // Update the time when our player can fire next
                m_NextFire = Time.time + 1.5f;
                shootCannonball(m_ShootForce, Mathf.Sign(vectorAction.ContinuousActions[2]) > 0? "right" : "left");
                m_PossibleShoot = false;

            }
        }
    }

    /// <summary>
    /// Function to move the agent
    /// </summary>
    /// <param name="act"></param>
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var move = act[0];
        var rotate = act[1];
        var shoot = act[2];

        // Avoid spamming
        if (Time.time > m_NextFire)
        {
            m_PossibleShoot = true;
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
                // Left Small shoot (20)
                case 1:
                    m_ShootForce = 20f;
                    // Update the time when our player can fire next
                    m_NextFire = Time.time + 1.5f;
                    shootCannonball(m_ShootForce, "left");
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
                // Right Small shoot (15)
                case 2:
                    m_ShootForce = 20f;
                    // Update the time when our player can fire next
                    m_NextFire = Time.time + 1.5f;
                    shootCannonball(m_ShootForce, "right");
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
        }

        transform.Rotate(rotateDir, Time.deltaTime * m_RotationSpeed);

        m_AgentRb.AddForce(dirToGo * m_Speed, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Heuristic 
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if(useContinuousActions)
        {
            var continuousActions = actionsOut.ContinuousActions;
            continuousActions.Clear();
            continuousActions[0] = Input.GetAxis("Vertical");

            // rotate

            continuousActions[1] = Input.GetAxis("Horizontal");

            //shoot
            if (Input.GetKey(KeyCode.G))
            {
                continuousActions[2] = 0.5f;
            }
            else if (Input.GetKey(KeyCode.H))
            {
                continuousActions[2] = -0.5f;
            }
        }
        else
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
            if (Input.GetKey(KeyCode.H))
            {
                discreteActionsOut[2] = 2;
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
    }

    /// <summary>
    /// When receive an action add a t penality => by adding that we push the agent to kill the enemy "faster"
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Add t penality
        timePenalty -= m_Penalty;

        // Move the agent
        if(useContinuousActions)
        {
            AgentActionConituous(actions);
        }
        else
        {
            MoveAgent(actions.DiscreteActions);
        }

    }

    private void OnCollisionStay(Collision other) {
        if(other.gameObject.CompareTag("border") || other.gameObject.CompareTag("island") || other.gameObject.CompareTag("islandObstacle") 
            || other.gameObject.CompareTag("greenAgent") || other.gameObject.CompareTag("blueAgent"))
        {
            m_BoatHealthSystem.TakeDamage(10f * Time.deltaTime);
            AddReward(-10f*Time.deltaTime);
        }
    }
}


