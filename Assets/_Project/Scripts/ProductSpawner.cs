using UnityEngine;

public class ProductSpawner : MonoBehaviour
{
    [Header("Product")]
    [SerializeField] private ConveyorProduct productPrefab;
    [SerializeField] private ConveyorProduct alternateProductPrefab;
    [SerializeField] private ConveyorSegment startingSegment;

    [Header("Timing")]
    [SerializeField, Min(0.25f)]
    private float spawnInterval = 2f;

    [SerializeField]
    private bool spawnImmediately = true;

    [Header("Conveyor network")]
    [SerializeField, Min(0.01f)]
    private float connectionDistance = 0.1f;

    private float timeUntilNextSpawn;
    private bool spawnAlternateNext;

    private void Start()
    {
        if (productPrefab == null)
        {
            Debug.LogError(
                "ProductSpawner requires a product prefab.",
                this);

            enabled = false;
            return;
        }

        if (startingSegment == null)
        {
            Debug.LogError(
                "ProductSpawner requires a starting conveyor.",
                this);

            enabled = false;
            return;
        }

        // Scene conveyors may already be touching before play begins.
        // Build their runtime links once from their snap positions.
        ConveyorSegment.RebuildConnectionsFromSnaps(
            connectionDistance);

        if (spawnImmediately)
        {
            SpawnProduct();
        }

        timeUntilNextSpawn = spawnInterval;
    }

    private void Update()
    {
        timeUntilNextSpawn -= Time.deltaTime;

        if (timeUntilNextSpawn > 0f)
        {
            return;
        }

        SpawnProduct();
        timeUntilNextSpawn = spawnInterval;
    }

    private void SpawnProduct()
    {
        ConveyorSegment firstSegment =
            startingSegment.FindFirstSegment();

        ConveyorProduct prefabToSpawn = productPrefab;

        if (alternateProductPrefab != null &&
            spawnAlternateNext)
        {
            prefabToSpawn = alternateProductPrefab;
        }

        ConveyorProduct product =
            Instantiate(prefabToSpawn);

        if (alternateProductPrefab != null)
        {
            spawnAlternateNext = !spawnAlternateNext;
        }

        if (!product.BeginMovingOn(firstSegment))
        {
            Destroy(product.gameObject);
        }
    }
}
