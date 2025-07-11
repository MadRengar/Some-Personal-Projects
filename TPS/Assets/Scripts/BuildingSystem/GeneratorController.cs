using UnityEngine;

public class GeneratorController : MonoBehaviour, IBuildingController
{
    [Header("Generator Data")]
    public GeneratorData_SO generatorData;

    [Header("Current Stats")]
    [SerializeField] private int currentHealth;

    private InventoryManager inventoryManager;
    public BuildingData_SO GetBuildingData() => generatorData;

    private void Start()
    {
        inventoryManager = GameManager.Instance.GetInventoryManager();
        if (generatorData != null)
        {
            currentHealth = generatorData.maxHealth;
            Debug.Log($"发电机启动，提供 {generatorData.powerOutput} 点电力");
        }
        // 自动注册到 InventoryManager
        RegisterToInventoryManager();
    }

    public void RegisterToInventoryManager()
    {
        if (inventoryManager != null)
        {
            inventoryManager.RegisterGenerator(this);
            inventoryManager.GeneratorInvokeEvent();
        }
        else
        {
            Debug.LogError("找不到 InventoryManager，无法注册发电机");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"发电机受到 {damage} 点伤害，剩余血量: {currentHealth}");

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    public bool IsDestroyed()
    {
        return currentHealth <= 0;
    }

    private void DestroyBuilding()
    {
        Debug.Log("发电机被摧毁！");
        // 这里可以添加爆炸效果、电力系统更新等
        Destroy(gameObject);
        inventoryManager.UnregisterGenerator(this);
        inventoryManager.GeneratorInvokeEvent();

    }

    // 获取电力输出
    public int GetPowerOutput()
    {
        return IsDestroyed() ? 0 : generatorData.powerOutput;
    }
}