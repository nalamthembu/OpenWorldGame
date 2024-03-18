using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Blackboard
{
    public Dictionary<string, object> data = new();

    // Method to set data on the blackboard
    public void SetValue(string key, object value)
    {
        if (data.ContainsKey(key))
        {
            data[key] = value;
        }
        else
        {
            data.Add(key, value);
        }
    }

    // Method to get data from the blackboard
    public T GetValue<T>(string key)
    {
        if (data.ContainsKey(key))
        {
            return (T)data[key];
        }
        else
        {
            Debug.LogWarning("Key not found on blackboard: " + key);
            return default;
        }
    }
}