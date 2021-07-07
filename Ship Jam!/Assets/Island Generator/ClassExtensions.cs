using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

public static class ClassExtensions
{

    public static List<T> ToList<T>(this T[] array) {
        List<T> list = new List<T>();
        foreach (T t in array) {
            list.Add(t);
        }
        return list;
    }

    public static string CreateString(this IList<object> objects)
    {
        StringBuilder s = new StringBuilder();
        foreach (object o in objects)
        {
            s.Append(o.ToString());
        }
        return s.ToString();
    }
    public static string CreateString(this List<char> chars)
    {
        StringBuilder s = new StringBuilder();
        foreach (char c in chars)
        {
            s.Append(c);
        }
        return s.ToString();
    }


    public static float Length(this Vector3 vector, DistanceMethod distanceMethod = DistanceMethod.EUCLIDEAN) {

        switch (distanceMethod)
        {
            case DistanceMethod.EUCLIDEAN_SQRD:
                return vector.sqrMagnitude;
            case DistanceMethod.MANHATTAN:
                return Mathf.Abs(vector.x) + Mathf.Abs(vector.y) + Mathf.Abs(vector.z);
            case DistanceMethod.SQUARE:
                return Mathf.Max(Mathf.Abs(vector.x),Mathf.Abs(vector.y),Mathf.Abs(vector.z));
            case DistanceMethod.EUCLIDEAN:
            default:
                return vector.magnitude;
        }
    }

    public static bool Remove<T>(this List<T> list, System.Func<T, bool> predicate) {
        bool flag = false;
        for (int i = list.Count - 1; i >= 0; i--) {
            if (predicate(list[i]))
            {
                list.RemoveAt(i);
                flag = true;
            }
        }
        return flag;
    }

    public static void LocalisePositionAndRotation(this Transform transform) {
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }

    public static Vector3 ToVec3(this Vector2Int v) { return new Vector3(v.x, v.y, 0); }
    public static Vector3 ToVec3(this Vector2 v) { return new Vector3(v.x, v.y, 0); }

    public static int Width<T>(this T[,] twoDArray){ return twoDArray.GetLength(0); }
    public static int Height<T>(this T[,] twoDArray){ return twoDArray.GetLength(1); }

    public static T[] To1DArray<T>(this T[,] twoDArray)
    {
        T[] array = new T[twoDArray.Width() * twoDArray.Height()];
        for (int x = 0; x < twoDArray.Width(); x++)
        {
            for (int y = 0; y < twoDArray.Height(); y++)
            {
                array[x + (y * twoDArray.Width())] = twoDArray[x, y];
            }
        }
        return array;
    }


    public static T[,] To2DArray<T>(this T[] oneDArray, int width)
    {
        T[,] array = new T[width, oneDArray.Length/width];

        for (int i = 0; i < oneDArray.Length; i++) {
            array[i%width,(int)(i/width)] = oneDArray[i];
        }

        return array;
    }

    public static string SecsToMinAndSecs(this float secs) { return ((int)secs).SecsToMinAndSecs(); }
    public static string SecsToMinAndSecs(this int secs) {
        int minutes = (int)(secs / 60);
        int seconds = (int)(secs % 60);
        return (minutes > 9 ? "" : "0") + minutes + ":" + (seconds > 9 ? "" : "0") + seconds;
    }


    public static T GetRandom<T>(this List<T> list)
    {
        if (list.Count <= 0) { return default(T); }
        return list[Random.Range(0, list.Count)];
    }
    public static List<T> Shuffled<T>(this List<T> list)
    {
        List<T> temp = new List<T>();
        if (list == null) { return temp; }
        List<T> original = list.ToArray().ToList();
        while (original.Count > 0)
        {
            int i = Random.Range(0, original.Count);
            temp.Add(original[i]);
            original.RemoveAt(i);
        }
        return temp;
    }

    public static void AddIfNotDuplicate<T>(this List<T> list, T item) {
        if (!list.Contains(item)) { list.Add(item); }
    }
    

}
