using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class BackgroundManager : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private List<GameObject> backgroundPrefabs;
    [SerializeField] private float backgroundHeight = 10f;
    [SerializeField] private int initialBackgroundCount = 3;

    [Header("Decoration Settings")]
    [SerializeField] private List<GameObject> decorationPrefabs;
    [SerializeField] private float decorationDensity = 0.3f;
    [SerializeField] private float decorationSpawnChance = 0.7f;

    [Header("Shop Integration")]
    [SerializeField] private GameObject defaultBackgroundPrefab;

    private GameObject activeBackgroundPrefab;

    private List<GameObject> activeBackgrounds = new List<GameObject>();
    private Transform playerTransform;
    private float lastBackgroundEndY = 0f;
    private ObjectPooler objectPooler;
    private bool isScrolling = false;

    private void Awake()
    {
        objectPooler = GetComponent<ObjectPooler>();
        if (objectPooler == null)
        {
            objectPooler = gameObject.AddComponent<ObjectPooler>();
        }

        InitializePools();
    }

    public void SetActivePrefab(GameObject prefab)
    {
        if (prefab == null)
            return;

        // Store the active background prefab
        activeBackgroundPrefab = prefab;

        // Check if the prefab is already in our list
        bool prefabInList = false;
        int prefabIndex = -1;

        for (int i = 0; i < backgroundPrefabs.Count; i++)
        {
            if (backgroundPrefabs[i].name == prefab.name)
            {
                prefabInList = true;
                prefabIndex = i;
                break;
            }
        }

        // Handle prefab list management
        if (!prefabInList)
        {
            // If the prefab is not in our list, replace the list with only this prefab
            backgroundPrefabs.Clear();
            backgroundPrefabs.Add(prefab);
        }
        else if (prefabIndex != 0)
        {
            // If the prefab is in our list but not first, move it to the front
            GameObject selectedPrefab = backgroundPrefabs[prefabIndex];
            backgroundPrefabs.RemoveAt(prefabIndex);
            backgroundPrefabs.Insert(0, selectedPrefab);
        }


        Debug.Log($"Active background prefab set to: {prefab.name}");
    }

    private void InitializePools()
    {
        if (backgroundPrefabs != null && backgroundPrefabs.Count > 0)
        {
            foreach (GameObject prefab in backgroundPrefabs)
            {
                if (prefab != null)
                {
                    objectPooler.CreatePool(prefab, 3);
                }
            }
        }

        if (decorationPrefabs != null && decorationPrefabs.Count > 0)
        {
            foreach (GameObject prefab in decorationPrefabs)
            {
                if (prefab != null)
                {
                    objectPooler.CreatePool(prefab, 5);
                }
            }
        }
    }

    public void Initialize(Transform player)
    {
        playerTransform = player;

        Debug.Log("BackgroundManager initialized with player: " + (playerTransform != null));
    }

    public void StartScrolling()
    {
        if (isScrolling)
            return;

        isScrolling = true;
        ResetBackground();

        Debug.Log("Background scrolling started");
    }

    public void StopScrolling()
    {
        isScrolling = false;

        Debug.Log("Background scrolling stopped");
    }

    public void ResetBackground()
    {
        ClearAllBackgrounds();

        lastBackgroundEndY = -10f; // Start below the player position

        // Create initial backgrounds
        for (int i = 0; i < initialBackgroundCount; i++)
        {
            SpawnBackground();
        }

        Debug.Log("Backgrounds reset");
    }

    private void Update()
    {
        if (!isScrolling || playerTransform == null)
            return;

        // Check if we need to spawn a new background
        CheckBackgroundSpawn();

        // Remove backgrounds that are too far behind
        RemoveOldBackgrounds();
    }

    private void CheckBackgroundSpawn()
    {
        // If the player is approaching the last background's end, spawn a new one
        if (playerTransform.position.y + backgroundHeight * 1.5f > lastBackgroundEndY)
        {
            SpawnBackground();
        }
    }

    private void SpawnBackground()
    {
        if (backgroundPrefabs == null || backgroundPrefabs.Count == 0)
            return;

        // Choose a random background
        GameObject prefab = activeBackgroundPrefab;

        // Calculate position for new background
        Vector3 spawnPosition = new Vector3(0, lastBackgroundEndY, 1); // z = 1 to be behind other objects

        // Spawn the background from pool
        GameObject backgroundObj = objectPooler.SpawnFromPool(prefab.name, spawnPosition, Quaternion.identity);

        if (backgroundObj != null)
        {
            backgroundObj.transform.SetParent(transform);
            activeBackgrounds.Add(backgroundObj);

            // Get background height from sprite renderer
            SpriteRenderer spriteRenderer = backgroundObj.GetComponent<SpriteRenderer>();
            float bgHeight = spriteRenderer != null ? spriteRenderer.bounds.size.y : backgroundHeight;

            // Update end position for next background
            lastBackgroundEndY += bgHeight;

            // Animate background appearance
            //backgroundObj.transform.localScale = new Vector3(1.05f, 1.05f, 1);
            //backgroundObj.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            // Spawn decorations on this background
            SpawnDecorations(backgroundObj, bgHeight);

            Debug.Log($"Spawned background at Y:{spawnPosition.y}, end Y:{lastBackgroundEndY}");
        }
    }

    private void SpawnDecorations(GameObject backgroundObj, float bgHeight)
    {
        if (decorationPrefabs == null || decorationPrefabs.Count == 0 || Random.value > decorationSpawnChance)
            return;

        // Calculate how many decorations to spawn based on density and background height
        int decorationCount = Mathf.RoundToInt(decorationDensity * bgHeight);

        for (int i = 0; i < decorationCount; i++)
        {
            // Choose a random decoration
            GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Count)];

            // Calculate a random position on the background
            float randomY = Random.Range(0, bgHeight);
            float randomX = Random.Range(-2.5f, 2.5f);

            Vector3 decorationPosition = new Vector3(
                randomX,
                backgroundObj.transform.position.y + randomY,
                0.5f // z = 0.5 to be in front of background but behind other objects
            );

            // Spawn decoration from pool
            GameObject decorationObj = objectPooler.SpawnFromPool(prefab.name, decorationPosition, Quaternion.identity);

            if (decorationObj != null)
            {
                decorationObj.transform.SetParent(backgroundObj.transform);

                // Add some variation
                float randomScale = Random.Range(0.8f, 1.2f);
                decorationObj.transform.localScale = Vector3.one * randomScale;

                float randomRotation = Random.Range(-15f, 15f);
                decorationObj.transform.rotation = Quaternion.Euler(0, 0, randomRotation);

                // Add subtle animation
                decorationObj.transform.DOScale(decorationObj.transform.localScale * 1.1f, Random.Range(1f, 2f))
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
    }

    private void RemoveOldBackgrounds()
    {
        if (playerTransform == null)
            return;

        List<GameObject> backgroundsToRemove = new List<GameObject>();

        foreach (GameObject bg in activeBackgrounds)
        {
            if (bg == null)
            {
                backgroundsToRemove.Add(bg);
                continue;
            }

            // If background is completely below the player view, remove it
            SpriteRenderer spriteRenderer = bg.GetComponent<SpriteRenderer>();
            float bgHeight = spriteRenderer != null ? spriteRenderer.bounds.size.y : backgroundHeight;

            if (bg.transform.position.y + bgHeight < playerTransform.position.y - 10f)
            {
                backgroundsToRemove.Add(bg);
                ReturnBackgroundToPool(bg);

                Debug.Log($"Removed background at Y:{bg.transform.position.y}");
            }
        }

        // Remove from tracking list
        foreach (GameObject bg in backgroundsToRemove)
        {
            activeBackgrounds.Remove(bg);
        }
    }



    private void ReturnBackgroundToPool(GameObject bg)
    {
        // First, return all child decorations to pool
        for (int i = bg.transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = bg.transform.GetChild(i).gameObject;

            // Kill any DOTween animations
            DOTween.Kill(child.transform);

            // Return to pool
            objectPooler.ReturnToPool(child);
        }

        // Then return the background itself
        objectPooler.ReturnToPool(bg);
    }

    public void ClearAllBackgrounds()
    {
        // Create a copy of the list to avoid modification during iteration
        List<GameObject> bgsCopy = new List<GameObject>(activeBackgrounds);

        foreach (GameObject bg in bgsCopy)
        {
            if (bg != null)
            {
                ReturnBackgroundToPool(bg);
            }
        }

        activeBackgrounds.Clear();
        lastBackgroundEndY = 0f;

        // Kill all DOTween animations
        DOTween.Kill(transform);

        Debug.Log("All backgrounds fully cleared");
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);

        foreach (GameObject bg in activeBackgrounds)
        {
            if (bg != null)
            {
                DOTween.Kill(bg.transform);

                for (int i = 0; i < bg.transform.childCount; i++)
                {
                    DOTween.Kill(bg.transform.GetChild(i));
                }
            }
        }
    }
}