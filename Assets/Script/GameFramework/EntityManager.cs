using UnityEngine;
using System.Collections.Generic;

public class EntityManager : MonoBehaviour
{
    public readonly List<Entity> m_EntitiesInScene = new();

    public static EntityManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        Entity.OnOutOfWorld += OnEntityOutOfWorld;
        Entity.OnAddToWorld += OnEntityAddedToWorld;
    }


    private void OnDisable()
    {
        Entity.OnOutOfWorld -= OnEntityOutOfWorld;
        Entity.OnAddToWorld -= OnEntityAddedToWorld;
    }

    private void OnEntityAddedToWorld(Entity new_entity)
    {
        if (m_EntitiesInScene != null)
            m_EntitiesInScene.Add(new_entity);
        else
        {
            Debug.LogError("Entity list on the entity manager is NULL");
        }
    }


    //This method gets rid of any entities that leave the world and
    //could potentially cause performance issues if kept for no reason.
    private void OnEntityOutOfWorld(Entity entityToBeDeleted)
    {
        if (entityToBeDeleted == null)
            return;

        if (m_EntitiesInScene != null && m_EntitiesInScene.Count > 0)
        {
            m_EntitiesInScene.Remove(entityToBeDeleted);
            Destroy(entityToBeDeleted.gameObject);
        }
    }

    //This gets rid of all entities in a specified area,
    //this only works if the entity has a collider on it.
    public void ClearEntitiesInArea(Vector3 position, float radius)
    {
        Collider[] entityCols = Physics.OverlapSphere(position, radius);

        foreach (Collider col in entityCols)
        {
            if (col.TryGetComponent<Entity>(out var entity))
            {
                OnEntityOutOfWorld(entity);
            }
        }
    }

    /*
    //This gets all instances of entities in the scene.
    private void FindAllEntitiesInScene()
    {
        Entity[] entities = FindObjectsOfType<Entity>();

        for (int i = 0; i < entities.Length; i++)
        {
            m_EntitiesInScene.Add(entities[i]);
        }
    }
    */
}