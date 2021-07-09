using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public Agent blueAgent;
    public Agent greenAgent;
    public BoatArea boatArea;
    public LayerMask agentLayerMask;
    public GameObject playerControlIndicator;
    Agent currentPossesdAgent = null;

    Camera mainCamera;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        mainCamera = Camera.main;   
    }
    
    private void Update() {
        // if(Input.GetMouseButtonDown(0))
        // {
        //     Ray r = mainCamera.ScreenPointToRay(Input.mousePosition);
        //     if(Physics.Raycast(r, out RaycastHit hit, 100, agentLayerMask))
        //     {
        //         Agent hitAgent = hit.rigidbody?.GetComponent<Agent>();
        //         if(hitAgent != null)
        //         {
        //             if(currentPossesdAgent != null)
        //             {
        //                 ReturnToBotControl(currentPossesdAgent);
        //                 currentPossesdAgent = null;
        //             }

        //             TakePlayerControl(hitAgent);
        //             currentPossesdAgent = hitAgent;
        //         }
        //     }
        // }
    }
    
    public void TakePlayerControl(Agent targetAgent)
    {
        targetAgent.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
        Instantiate(playerControlIndicator, targetAgent.transform).name = "Indicator";
    }

    public void ReturnToBotControl(Agent targetAgent)
    {
        targetAgent.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
        Destroy(targetAgent.transform.Find("Indicator"));
    }
}
