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
        if (damage < 0) // 负伤害表示修复
        {
            int healAmount = -damage;
            int maxHealth = generatorData.maxHealth;

            if (currentHealth >= maxHealth)
            {
                Debug.Log($"发电机 {name} 已满血，无需修复");
                return;
            }

            // 计算实际修复量，不能超过最大生命值
            int actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);
            currentHealth += actualHeal;

            Debug.Log($"发电机 {name} 修复 {actualHeal} 点，当前耐久: {currentHealth}/{maxHealth}");
        }
        else // 正常伤害
        {
            currentHealth -= damage;
            Debug.Log($"发电机受到 {damage} 点伤害，剩余生命: {currentHealth}");

            if (currentHealth <= 0)
            {
                DestroyBuilding();
            }
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

    #region Getter
    public int GetPowerOutput() => generatorData.powerOutput;

    public int GetCurrentHealth() => currentHealth;

    #endregion
}

