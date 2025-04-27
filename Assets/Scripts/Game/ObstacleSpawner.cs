using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float initialSpawnRate = 5f;
    [SerializeField] private float minSpawnRate = 2f;
    [SerializeField] private float startDistance = 10f;
    [SerializeField] private float spawnDistance = 15f;
    [SerializeField] private float despawnDistance = -5f;

    [Header("Obstacle Prefabs")]
    [SerializeField] private List<GameObject> jumpableObstacles; // Obstacles that can be jumped over
    [SerializeField] private List<GameObject> slideObstacles;    // Obstacles that player can slide under
    [SerializeField] private List<GameObject> avoidObstacles;    // Obstacles that must be avoided by lane change

    [Header("Coin Prefabs")]
    [SerializeField] private List<GameObject> coinPrefabs;

    [Header("Spawn Chances")]
    [SerializeField][Range(0f, 1f)] private float coinSpawnChance = 0.3f;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 1.5f;
    [SerializeField] private int lanes = 3;

    // Object pooling
    private ObjectPooler objectPooler;

    // Runtime variables
    private Transform playerTransform;
    private GameManager gameManager;
    private float spawnRate;
    private float nextSpawnTime;
    private bool isSpawning = false;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        // Set default values
        spawnRate = initialSpawnRate;

        // Get object pooler
        objectPooler = GetComponent<ObjectPooler>();
        if (objectPooler == null)
        {
            objectPooler = gameObject.AddComponent<ObjectPooler>();
        }
    }

    public void Initialize(Transform player, GameManager manager)
    {
        playerTransform = player;
        gameManager = manager;

        // Initialize object pools
        InitializePools();

        Debug.Log("ObstacleSpawner initialized");
    }

    private void InitializePools()
    {
        // Create pools for all prefabs
        CreatePools(jumpableObstacles, 3);
        CreatePools(slideObstacles, 3);
        CreatePools(avoidObstacles, 3);
        CreatePools(coinPrefabs, 10);
    }

    private void CreatePools(List<GameObject> prefabs, int poolSize)
    {
        if (prefabs == null || prefabs.Count == 0 || objectPooler == null)
            return;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                objectPooler.CreatePool(prefab, poolSize);
            }
        }
    }

    public void StartSpawning()
    {
        if (isSpawning || playerTransform == null)
            return;

        isSpawning = true;
        nextSpawnTime = Time.time + spawnRate;

        // Spawn initial obstacles
        StartCoroutine(SpawnInitialObstacles());

        Debug.Log("Obstacle spawning started");
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    private IEnumerator SpawnInitialObstacles()
    {
        // Wait a moment before spawning initial obstacles to give player time to get ready
        yield return new WaitForSeconds(1f);

        // Spawn obstacles ahead of the player
        for (float y = startDistance; y < spawnDistance; y += 5f)
        {
            Vector3 spawnPosition = new Vector3(0, playerTransform.position.y + y, 0);
            SpawnRandomObjectAt(spawnPosition);
            yield return null;
        }
    }

    private void Update()
    {
        if (!isSpawning || playerTransform == null)
            return;

        // Spawn new obstacles
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomObject();
            nextSpawnTime = Time.time + spawnRate;
        }

        // Cleanup old obstacles
        CleanupObjects();
    }

    private void SpawnRandomObject()
    {
        if (playerTransform == null)
            return;

        Vector3 spawnPosition = new Vector3(0, playerTransform.position.y + spawnDistance, 0);
        SpawnRandomObjectAt(spawnPosition);
    }

    private void SpawnRandomObjectAt(Vector3 basePosition)
    {
        // Determine what to spawn based on random value
        float randomValue = Random.value;
        Vector3 spawnPosition = basePosition;

        // Choose a random lane
        int randomLane = Random.Range(-(lanes / 2), lanes / 2 + (lanes % 2));
        spawnPosition.x = randomLane * laneDistance;

        // Spawn a coin or an obstacle
        if (randomValue < coinSpawnChance && coinPrefabs.Count > 0)
        {
            SpawnCoinPattern(spawnPosition);
        }
        else
        {
            SpawnObstaclePattern(spawnPosition);
        }
    }

    private void SpawnCoinPattern(Vector3 basePosition)
    {
        if (coinPrefabs.Count == 0)
            return;

        int patternType = Random.Range(0, 4);

        switch (patternType)
        {
            case 0: // Line of coins
                SpawnCoinLine(basePosition);
                break;

            case 1: // Curve of coins
                SpawnCoinCurve(basePosition);
                break;

            case 2: // Coins in all lanes
                SpawnCoinsAllLanes(basePosition);
                break;

            case 3: // Random coins
                SpawnRandomCoins(basePosition);
                break;
        }
    }

    private void SpawnCoinLine(Vector3 basePosition)
    {
        if (coinPrefabs.Count == 0)
            return;

        GameObject prefab = coinPrefabs[Random.Range(0, coinPrefabs.Count)];

        for (int i = 0; i < 5; i++)
        {
            Vector3 coinPosition = basePosition + Vector3.up * (i * 1f);
            SpawnObject(prefab, coinPosition);
        }
    }

    private void SpawnCoinCurve(Vector3 basePosition)
    {
        if (coinPrefabs.Count == 0 || lanes < 3)
            return;

        GameObject prefab = coinPrefabs[Random.Range(0, coinPrefabs.Count)];

        int startLane = -(lanes / 2);
        int endLane = lanes / 2;

        if (Random.value > 0.5f)
        {
            int temp = startLane;
            startLane = endLane;
            endLane = temp;
        }

        int steps = Mathf.Abs(endLane - startLane) + 1;

        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);
            int currentLane = Mathf.RoundToInt(Mathf.Lerp(startLane, endLane, t));

            Vector3 coinPosition = basePosition + Vector3.up * (i * 1f);
            coinPosition.x = currentLane * laneDistance;

            SpawnObject(prefab, coinPosition);
        }
    }

    private void SpawnCoinsAllLanes(Vector3 basePosition)
    {
        if (coinPrefabs.Count == 0)
            return;

        GameObject prefab = coinPrefabs[Random.Range(0, coinPrefabs.Count)];

        for (int i = -(lanes / 2); i <= lanes / 2; i++)
        {
            Vector3 coinPosition = basePosition;
            coinPosition.x = i * laneDistance;

            SpawnObject(prefab, coinPosition);
        }
    }

    private void SpawnRandomCoins(Vector3 basePosition)
    {
        if (coinPrefabs.Count == 0)
            return;

        GameObject prefab = coinPrefabs[Random.Range(0, coinPrefabs.Count)];
        int coinCount = Random.Range(3, 8);

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 coinPosition = basePosition + Vector3.up * (Random.Range(0, 5f));
            int randomLane = Random.Range(-(lanes / 2), lanes / 2 + (lanes % 2));
            coinPosition.x = randomLane * laneDistance;

            SpawnObject(prefab, coinPosition);
        }
    }

    private void SpawnObstaclePattern(Vector3 basePosition)
    {
        int patternType = Random.Range(0, 5);

        switch (patternType)
        {
            case 0: // Single obstacle
                SpawnSingleObstacle(basePosition);
                break;

            case 1: // Two obstacles side by side
                SpawnSideObstacles(basePosition);
                break;

            case 2: // Three obstacles with gap
                SpawnThreeObstaclesWithGap(basePosition);
                break;

            case 3: // Alternating obstacles
                SpawnAlternatingObstacles(basePosition);
                break;

            case 4: // Random obstacles
                SpawnRandomObstacles(basePosition);
                break;
        }
    }

    private void SpawnSingleObstacle(Vector3 basePosition)
    {
        // Choose a random obstacle type
        GameObject prefab = ChooseRandomObstaclePrefab();
        if (prefab == null)
            return;

        int randomLane = Random.Range(-(lanes / 2), lanes / 2 + (lanes % 2));
        Vector3 spawnPosition = basePosition;
        spawnPosition.x = randomLane * laneDistance;

        SpawnObject(prefab, spawnPosition);
    }

    private void SpawnSideObstacles(Vector3 basePosition)
    {
        // Create a gap in one lane
        int gapLane = Random.Range(-(lanes / 2), lanes / 2 + (lanes % 2));

        for (int i = -(lanes / 2); i <= lanes / 2; i++)
        {
            if (i == gapLane)
                continue;

            GameObject prefab = ChooseRandomObstaclePrefab();
            if (prefab == null)
                continue;

            Vector3 spawnPosition = basePosition;
            spawnPosition.x = i * laneDistance;

            SpawnObject(prefab, spawnPosition);
        }
    }

    private void SpawnThreeObstaclesWithGap(Vector3 basePosition)
    {
        SpawnSingleObstacle(basePosition);

        Vector3 position2 = basePosition + Vector3.up * 2f;
        SpawnSingleObstacle(position2);

        Vector3 position3 = basePosition + Vector3.up * 4f;
        SpawnSingleObstacle(position3);
    }

    private void SpawnAlternatingObstacles(Vector3 basePosition)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPosition = basePosition + Vector3.up * (i * 2f);
            int lane = (i % 2 == 0) ? -1 : 1;
            spawnPosition.x = lane * laneDistance;

            GameObject prefab = ChooseRandomObstaclePrefab();
            if (prefab != null)
            {
                SpawnObject(prefab, spawnPosition);
            }
        }
    }

    private void SpawnRandomObstacles(Vector3 basePosition)
    {
        int obstacleCount = Random.Range(1, 4);

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 spawnPosition = basePosition + Vector3.up * (Random.Range(0, 5f));
            int randomLane = Random.Range(-(lanes / 2), lanes / 2 + (lanes % 2));
            spawnPosition.x = randomLane * laneDistance;

            GameObject prefab = ChooseRandomObstaclePrefab();
            if (prefab != null)
            {
                SpawnObject(prefab, spawnPosition);
            }
        }
    }

    private GameObject ChooseRandomObstaclePrefab()
    {
        // Choose a random obstacle type
        int obstacleType = Random.Range(0, 3);
        List<GameObject> selectedList;

        switch (obstacleType)
        {
            case 0:
                selectedList = jumpableObstacles;
                break;
            case 1:
                selectedList = slideObstacles;
                break;
            case 2:
                selectedList = avoidObstacles;
                break;
            default:
                // Fallback to jumpable obstacles
                selectedList = jumpableObstacles;
                break;
        }

        if (selectedList == null || selectedList.Count == 0)
            return null;

        return selectedList[Random.Range(0, selectedList.Count)];
    }

    private GameObject SpawnObject(GameObject prefab, Vector3 position)
    {
        if (prefab == null || objectPooler == null)
            return null;

        GameObject spawnedObject = objectPooler.SpawnFromPool(prefab.name, position, Quaternion.identity);

        if (spawnedObject != null)
        {
            // Add to tracking list
            spawnedObjects.Add(spawnedObject);

            // Entry animation
            spawnedObject.transform.localScale = Vector3.zero;
            spawnedObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        return spawnedObject;
    }

    private void CleanupObjects()
    {
        if (playerTransform == null || objectPooler == null)
            return;

        // Remove objects that are too far behind the player
        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj == null)
            {
                objectsToRemove.Add(obj);
                continue;
            }

            if (obj.transform.position.y < playerTransform.position.y + despawnDistance)
            {
                objectsToRemove.Add(obj);
                objectPooler.ReturnToPool(obj);
            }
        }

        foreach (GameObject obj in objectsToRemove)
        {
            spawnedObjects.Remove(obj);
        }
    }

    public void ClearObstacles()
    {
        if (objectPooler == null)
            return;

        // Create a copy to avoid modification during iteration
        List<GameObject> objectsToRemove = new List<GameObject>(spawnedObjects);

        foreach (GameObject obj in objectsToRemove)
        {
            if (obj != null)
            {
                // Kill any DOTween animations
                DOTween.Kill(obj.transform);

                // Return to pool
                objectPooler.ReturnToPool(obj);
            }
        }

        spawnedObjects.Clear();

        Debug.Log("All obstacles cleared");
    }

    public void IncreaseDifficulty(float multiplier)
    {
        // Decrease spawn rate (make obstacles appear more frequently)
        spawnRate = Mathf.Max(spawnRate / multiplier, minSpawnRate);

        Debug.Log($"Difficulty increased: Spawn rate = {spawnRate}");
    }
}