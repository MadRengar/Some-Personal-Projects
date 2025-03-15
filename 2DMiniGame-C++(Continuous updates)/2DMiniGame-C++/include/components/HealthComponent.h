#pragma once
class HealthComponent
{
protected:
	int currentHealth;
	int maxHealth;
public:
	explicit HealthComponent(int startHP, int maxHP) :currentHealth(startHP), maxHealth(maxHP) {}
	int getHealth() const { return currentHealth; }
	void changeHealth(int changeValue)
	{
		currentHealth += changeValue;
		if (currentHealth > maxHealth)
		{
			currentHealth = maxHealth;
		}
		if (currentHealth < 0)
		{
			currentHealth = 0;
		}
	}
};