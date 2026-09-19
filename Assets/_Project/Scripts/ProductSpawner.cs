using UnityEngine;

public class ProductSpawner : MonoBehaviour
{
    [Header("Product")]
    [SerializeField] private ConveyorProduct productPrefab;
    [SerializeField] private ConveyorSegment startingSegment;

    [Header("Timing")]
    [SerializeField, Min(0.25f)]
    private float spawnInterval = 2f;

    [SerializeField]
    private bool spawnImmediately = true;

    private float timeUntilNextSpawn;

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
        ConveyorProduct product =
            Instantiate(productPrefab);

        if (!product.BeginMovingOn(startingSegment))
        {
            Destroy(product.gameObject);
        }
    }
}