using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // Dictionary of object pools
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabDictionary;

    private void Awake()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();

        Debug.Log("ObjectPooler initialized");
    }

    public void CreatePool(GameObject prefab, int size)
    {
        if (prefab == null)
        {
            Debug.LogError("Cannot create pool for null prefab");
            return;
        }

        string tag = prefab.name;

        // Skip if pool already exists
        if (poolDictionary.ContainsKey(tag))
        {
            Debug.Log($"Pool for {tag} already exists");
            return;
        }

        // Create the pool
        Queue<GameObject> objectPool = new Queue<GameObject>();
        prefabDictionary[tag] = prefab;

        // Create container for this pool's objects
        GameObject poolContainer = new GameObject($"Pool-{tag}");
        poolContainer.transform.SetParent(transform);

        // Create the initial objects
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.name = $"{tag}_{i}";
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer.transform);
            objectPool.Enqueue(obj);
        }

        // Add the pool to the dictionary
        poolDictionary.Add(tag, objectPool);

        Debug.Log($"Created pool for {tag} with size {size}");
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        // Check if pool exists
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist. Creating a new pool.");

            if (prefabDictionary.ContainsKey(tag))
            {
                CreatePool(prefabDictionary[tag], 5);
            }
            else
            {
                Debug.LogError($"Cannot spawn {tag}: prefab not found");
                return null;
            }
        }

        // If pool is empty, expand it
        if (poolDictionary[tag].Count == 0)
        {
            GameObject prefab = prefabDictionary[tag];

            if (prefab != null)
            {
                Debug.Log($"Expanding pool for {tag}");

                GameObject parent = GameObject.Find($"Pool-{tag}");

                GameObject obj = Instantiate(prefab);
                obj.name = $"{tag}_expanded";
                obj.SetActive(false);

                if (parent != null)
                {
                    obj.transform.SetParent(parent.transform);
                }
                else
                {
                    obj.transform.SetParent(transform);
                }

                poolDictionary[tag].Enqueue(obj);
            }
        }

        // Get an object from the pool
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        if (objectToSpawn == null)
        {
            Debug.LogError($"Object from pool {tag} is null");
            return null;
        }

        // Set the object's position and rotation
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Reset any components like Rigidbody
        Rigidbody2D rb = objectToSpawn.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Enable the object
        objectToSpawn.SetActive(true);

        // Reset any poolable components
        IPoolable poolableObject = objectToSpawn.GetComponent<IPoolable>();
        if (poolableObject != null)
        {
            poolableObject.OnObjectSpawn();
        }

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null)
            return;

        // Get the tag from the object name
        string[] nameParts = obj.name.Split('_');
        if (nameParts.Length == 0)
        {
            Debug.LogWarning($"Cannot return {obj.name} to pool: Invalid name format");
            Destroy(obj);
            return;
        }

        string tag = nameParts[0];

        // Check if pool exists
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist. Object will be destroyed.");
            Destroy(obj);
            return;
        }

        // Disable the object and return it to the pool
        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

    public void ClearPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
            return;

        Queue<GameObject> pool = poolDictionary[tag];

        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        poolDictionary.Remove(tag);

        GameObject poolContainer = GameObject.Find($"Pool-{tag}");
        if (poolContainer != null)
        {
            Destroy(poolContainer);
        }
    }

    public void ClearAllPools()
    {
        // Create a copy of keys to avoid modification during iteration
        List<string> poolKeys = new List<string>(poolDictionary.Keys);

        foreach (string tag in poolKeys)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                Queue<GameObject> pool = poolDictionary[tag];

                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        // Kill any DOTween animations
                        DOTween.Kill(obj.transform);

                        // Destroy the object
                        Destroy(obj);
                    }
                }

                GameObject poolContainer = GameObject.Find($"Pool-{tag}");
                if (poolContainer != null)
                {
                    Destroy(poolContainer);
                }
            }
        }

        poolDictionary.Clear();
        prefabDictionary.Clear();

        Debug.Log("All object pools cleared");
    }
}

// Interface for poolable objects
public interface IPoolable
{
    void OnObjectSpawn();
}