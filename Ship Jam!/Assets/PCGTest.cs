using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCGTest : MonoBehaviour
{
    public IslandGenerator islandGen;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            islandGen.Generate();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            islandGen.Generate(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            islandGen.Generate(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            islandGen.Generate(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            islandGen.Generate(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            islandGen.Generate(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            islandGen.Generate(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            islandGen.Generate(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            islandGen.Generate(7);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            islandGen.Generate(8);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            islandGen.Generate(9);
        }
    }
}
