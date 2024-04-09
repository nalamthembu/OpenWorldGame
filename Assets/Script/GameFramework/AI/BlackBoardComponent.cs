using UnityEngine;
using System.Collections.Generic;

namespace OWFramework.AI
{
    [SerializeField]
    public class BlackBoardComponent
    {
        // Dictionary to store key-value pairs
        private readonly Dictionary<string, object> keyValuePairs = new();

        // set a value (will add if it doesn't exist.
        public void SetValue<T>(string key, T value)
        {
            if (keyValuePairs.ContainsKey(key))
            {
                keyValuePairs[key] = value;
            }
            else
            {
                keyValuePairs.Add(key, value);
            }
        }

        // get a value
        public T GetValue<T>(string key)
        {
            if (keyValuePairs.ContainsKey(key))
            {
                return (T)keyValuePairs[key];
            }
            else
            {
                // If key is not found, return default value for type T
                return default;
            }
        }

        // check if key exists
        public bool ContainsKey(string key)
        {
            return keyValuePairs.ContainsKey(key);
        }

        // Remove a key
        public void RemoveKey(string key)
        {
            if (keyValuePairs.ContainsKey(key))
            {
                keyValuePairs.Remove(key);
            }
        }

        //Clear all keys
        public void Clear()
        {
            keyValuePairs.Clear();
        }
    }
}