using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    // Singleton instance to access the object pool from other scripts.
    public static ObjectPoolManager Instance;

    // The object to be pooled.
    public GameObject prefab;

    // Number of objects to initially create in the pool.
    public int poolSize = 10;

    // List to store the inactive objects.
    private List<GameObject> pooledObjects;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the object pool.
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
    }

    private void CreateNewObject()
    {
        GameObject newObj = Instantiate(prefab);
        newObj.SetActive(false);
        pooledObjects.Add(newObj);
    }

    public GameObject GetObjectFromPool()
    {
        // Search for an inactive object to reuse.
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // If no inactive object found, create a new one and return it.
        GameObject newObj = Instantiate(prefab);
        pooledObjects.Add(newObj);
        return newObj;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        // Deactivate the object and move it back to the pool.
        obj.SetActive(false);
    }
}
