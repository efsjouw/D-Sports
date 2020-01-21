using UnityEngine;

/// <summary>
/// Also finds disabled scripts with FindObjectsOfTypeAll
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour
{
    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null) _instance = (T) (object) Resources.FindObjectsOfTypeAll(typeof(T))[0];
            return _instance;
        }
    }
}
