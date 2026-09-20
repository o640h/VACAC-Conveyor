using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SimulationController : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField]
    private ConveyorPlacementManager placementManager;

    [SerializeField]
    private ProductSpawner productSpawner;

    public bool IsRunning { get; private set; }
    public int CompletedProducts { get; private set; }
    public float RunningTime { get; private set; }

    public int ActiveProducts =>
        FindObjectsByType<ConveyorProduct>().Length;

    public float SpeedMultiplier =>
        ConveyorProduct.SpeedMultiplier;

    public float SpawnInterval =>
        productSpawner != null
            ? productSpawner.SpawnInterval
            : 0f;

    public float ThroughputPerMinute =>
        RunningTime > 0.01f
            ? CompletedProducts / (RunningTime / 60f)
            : 0f;

    private void Awake()
    {
        if (placementManager == null)
        {
            placementManager =
                FindAnyObjectByType<ConveyorPlacementManager>();
        }

        if (productSpawner == null)
        {
            productSpawner =
                FindAnyObjectByType<ProductSpawner>();
        }
    }

    private void OnEnable()
    {
        ConveyorProduct.ProductCompleted +=
            HandleProductCompleted;
    }

    private void Start()
    {
        SetBuildMode();
    }

    private void Update()
    {
        if (IsRunning)
        {
            RunningTime += Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        ConveyorProduct.ProductCompleted -=
            HandleProductCompleted;

        ConveyorProduct.MovementEnabled = true;
        ConveyorProduct.SpeedMultiplier = 1f;
    }

    public void SetBuildMode()
    {
        IsRunning = false;
        ConveyorProduct.MovementEnabled = false;

        placementManager?.SetPlacementEnabled(true);
        productSpawner?.SetSpawning(false);
    }

    public void SetRunMode()
    {
        IsRunning = true;
        ConveyorProduct.MovementEnabled = true;

        placementManager?.SetPlacementEnabled(false);
        productSpawner?.SetSpawning(true);
    }

    public void ResetSimulation()
    {
        foreach (ConveyorProduct product in
                 FindObjectsByType<ConveyorProduct>())
        {
            Destroy(product.gameObject);
        }

        CompletedProducts = 0;
        RunningTime = 0f;
        placementManager?.ClearPlacedConveyors();
        productSpawner?.RestartSpawningTimer();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        ConveyorProduct.SpeedMultiplier =
            Mathf.Clamp(multiplier, 0.25f, 3f);
    }

    public void SetSpawnInterval(float interval)
    {
        productSpawner?.SetSpawnInterval(interval);
    }

    private void HandleProductCompleted()
    {
        CompletedProducts++;
    }
}
