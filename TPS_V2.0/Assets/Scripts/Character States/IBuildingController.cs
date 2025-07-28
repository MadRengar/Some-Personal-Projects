public interface IBuildingController
{
    BuildingData_SO GetBuildingData();
    void TakeDamage(int damage);
    bool IsDestroyed();
}