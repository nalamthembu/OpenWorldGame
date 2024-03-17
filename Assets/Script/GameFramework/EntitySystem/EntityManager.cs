using UnityEngine;
using System.Collections.Generic;

public class EntityManager : MonoBehaviour
{
    // <ID, Actual Entity>
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
        {
            m_EntitiesInScene.Add(new_entity);
        }
        else
        {
            Debug.LogError("Entity Dictionary on the entity manager is NULL");
        }
    }


    //This method gets rid of any entities that leave the world and
    //could potentially cause performance issues if kept for no reason.
    private void OnEntityOutOfWorld(Entity entity)
    {
        foreach (Entity ent in m_EntitiesInScene)
        {
            if (ent == entity)
            {
                Destroy(entity.gameObject);
                m_EntitiesInScene.Remove(ent);
                break;
            }
        }
    }

    //This gets rid of all entities in a specified area,
    //this only works if the entity has a collider on it.
    public void ClearEntitiesInArea(Vector3 position, float radius)
    {
        Collider[] entityCols = null;

        Physics.OverlapBoxNonAlloc(position, Vector3.one * radius, entityCols);

        if (entityCols != null && entityCols.Length > 0)
        {
            foreach (Collider col in entityCols)
            {
                if (col.TryGetComponent<Entity>(out var entity))
                {
                    OnEntityOutOfWorld(entity);
                }
            }
        }
    }
}