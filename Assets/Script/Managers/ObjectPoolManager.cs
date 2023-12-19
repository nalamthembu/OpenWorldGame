using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    public Pool[] pools;

    public Dictionary<string, Pool> poolDictionary;

    public static ObjectPoolManager Instance;

    private void Awake()
    {
        if (Instance is not null)
            Destroy(gameObject);
        else Instance = this;

        InitialisePools();
    }

    public Pool? GetPool(string poolName)
    {
        if (poolDictionary.TryGetValue(poolName, out Pool value))
        {
            return value;
        }

        Debug.LogError("Couldn't find " + poolName + " pool");

        return null;
    }

    private void InitialisePools()
    {
        poolDictionary = new();

        for (int i = 0; i < pools.Length; i++)
        {
            pools[i].Awake(transform);
            poolDictionary.Add(pools[i].name, pools[i]);
        }
    }

    private IEnumerator DelayedDeactivate(GameObject gameObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public void ReturnGameObject(GameObject gameObject, float delay = 0f) => StartCoroutine(DelayedDeactivate(gameObject, delay));
    
}

[System.Serializable]
public struct Pool
{
    public string name;
    [SerializeField] GameObject gameObject;
    [SerializeField] int amount;
    private List<GameObject> pooledObjects;
    [HideInInspector] public Transform objectPoolManager;

    public void OnValidate()
    {
        if (amount <= 0)
        {
            amount = 1;
        }
    }

    public void Awake(Transform poolManagerTransform)
    {
        pooledObjects = new();

        //Pool Parent
        Transform poolParent = new GameObject(name + "s").transform;

        poolParent.parent = poolManagerTransform;

        poolParent.transform.localPosition = poolParent.localEulerAngles *= 0;
        //End of pool parent initialisation.

        for (int i = 0; i < amount; i++)
        {
            GameObject gObj = Object.Instantiate(gameObject, objectPoolManager);

            gObj.SetActive(false);

            //Set gObj Parent to pool parent (this is just to organise the hierachy a little bit)

            gObj.transform.parent = poolParent;

            pooledObjects.Add(gObj);
        }
    }

    public GameObject GetGameObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                pooledObjects[i].SetActive(true);

                return pooledObjects[i];
            }
        }

        Debug.LogError("Could not get object from " + name + " pool.");

        return null;
    }
}