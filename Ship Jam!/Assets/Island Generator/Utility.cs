using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T GetClosestObjectToPoint<T>(List<T> list, Vector3 point, DistanceMethod distanceMethod = DistanceMethod.EUCLIDEAN, bool useColliderCenter = false) where T : Component
    {
        float shortestDistance = float.MaxValue;
        T item = null;

        Vector3 vector = new Vector3();
        foreach (T current in list)
        {
            vector = current.transform.position - point;
            if (useColliderCenter && current.TryGetComponent<Collider2D>(out Collider2D collider)) {
                vector.x += collider.offset.x;
                vector.y += collider.offset.y;
            }
            float d = vector.Length(distanceMethod);
            if (d < shortestDistance)
            {
                shortestDistance = d;
                item = current;
            }
        }

        return item;
    }

    public static void SortBy<T, V>(this List<T> list, System.Func<T, V> predicate) where V:IComparable
    {
        Dictionary<T, V> map = new Dictionary<T, V>();
        
        foreach (T current in list)
        {
            map.Add(current,predicate(current));
        }
        list.Sort((T a, T b)=> {
            V ad;
            V bd;
            if (map.TryGetValue(a, out ad) && map.TryGetValue(b, out bd))
            {
                return ad.CompareTo(bd);
            }
            else {
                return 0;
            }
        });
    }

}


public enum DistanceMethod { EUCLIDEAN, EUCLIDEAN_SQRD, MANHATTAN, SQUARE }