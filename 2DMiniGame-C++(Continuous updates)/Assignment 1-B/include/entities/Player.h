#pragma once
#include "Entity.h"
#include "../../include/core/InputHandler.h"
#include "../components/HealthComponent.h"
#include "../components/InputComponent.h"
#include "../components/VelocityComponent.h"

class Fire;

// VI.A (2/2): Add a forward declaration to the class PlayerInputHandler
class PlayerInputHandler;

class Player :  public Entity
{
public:

	const float playerSpeed = 100.f;
	const int startingHealth = 60;
	const int maxHealth = 100;
	const int maxWood = 100;
	const int shootingCost = 20;
	const float fireSpeed = 200.f;
	const float shootCooldownTime = 3.f; //in seconds

	Player();
	~Player();

	virtual void update(Game* game, float elapsed = 1.0f) override;

	void handleInput(Game& game);

	bool isAttacking() const { return attacking; }
	void setAttacking(bool at) { attacking = at; }

	bool isShouting() const { return shouting; }
	void setShouting(bool sh) { shouting = sh; }

	int getWood() const { return wood; }
	void addWood(int w);

	bool hasSpriteSheet() const { return isSpriteSheet; }

	void positionSprite(int row, int col, int spriteWH, float tileScale);

	std::shared_ptr<HealthComponent> getHealthComponent() const { return health; }
	std::shared_ptr<VelocityComponent> getVelocityComp() const { return velocity; }

private:

	std::shared_ptr<Fire> createFire() const;

	bool attacking;
	bool shouting;
	int wood;
	float shootCooldown;

	// VI.A (1/2): Declare a unique pointer to a player input handler.
	//std::unique_ptr<PlayerInputHandler> playerInputHandler; //had been deleted in 1-C 5. Adding the Input Component
	std::unique_ptr<PlayerInputComponent> playerInputComponent;
	std::shared_ptr<HealthComponent> health;/*Deleted 'health' member variable*/
	std::shared_ptr<VelocityComponent> velocity;
};

