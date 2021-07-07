using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Extensions.Sensors;

namespace Unity.MLAgents.Extensions.Sensors
{/*
    public class GridSensorBoatTest : GridSensor
    {
        protected override float[] GetObjectData(GameObject currentColliderGo,
            float typeIndex, float normalized_distance)
        {
            float[] channelValues = new float[ChannelDepth.Length];

            channelValues[0] = typeIndex;

            Rigidbody goRb = currentColliderGo.GetComponent<Rigidbody>();

            if (goRb != null)
            {
                if (goRb.gameObject.layer == 0)
                {
                    channelValues[1] = goRb.position.normalized.y;
                    if (channelValues[1] < 0f)
                    {
                        channelValues[1] = 0.0f;
                    }
                }
                else if (goRb.gameObject.layer == 9)
                {
                    channelValues[2] = goRb.gameObject.GetComponent<BoatHealth>().m_NormalizedCurrentHealth;
                    if (channelValues[2] < 0f)
                    {
                        channelValues[2] = 0.0f;
                    }
                }
            }
            return channelValues;
        }
    }
}*/
}