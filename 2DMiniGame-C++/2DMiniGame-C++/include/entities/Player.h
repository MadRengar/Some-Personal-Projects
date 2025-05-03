#pragma once
#include "Entity.h"
#include "../../include/core/InputHandler.h"
#include "../components/HealthComponent.h"
#include "../components/InputComponent.h"
#include "../components/VelocityComponent.h"
#include "../components/ColliderComponent.h"
#include "../components/PlayerStateComponent.h"
#include "../components/PositionComponent.h"

class Fire;

// VI.A (2/2): Add a forward declaration to the class PlayerInputHandler
class PlayerInputHandler;
class PlayerStateComponent;
class Player :  public Entity
{
public:

	const float playerSpeed = 100.f;
	const int startingHealth = 60;
	const int maxHealth = 100;

	Player();
	~Player();

	virtual void update(Game* game, float elapsed = 1.0f) override;

	void positionSprite(int row, int col, int spriteWH, float tileScale);
	std::shared_ptr<InputComponent> getInputComponent() override { return input; }
	std::shared_ptr<HealthComponent> getHealthComponent() const { return health; }
	std::shared_ptr<VelocityComponent> getVelocityComponent() override { return velocity; }
	std::shared_ptr<BoxColliderComponent> getColliderComponent() override { return collider;}
	std::shared_ptr<PlayerStateComponent> getPlayerStateComponent() const { return state; }
	std::shared_ptr<LogicComponent> getLogicComponent() const override {
		return std::static_pointer_cast<LogicComponent>(getPlayerStateComponent());
	}
	void onPotionCollision(Game* game, Entity* other);
	void onLogCollision(Game* game, Entity* other);


private:

	//std::shared_ptr<Fire> createFire() const;

	// VI.A (1/2): Declare a unique pointer to a player input handler.
	//std::unique_ptr<PlayerInputHandler> playerInputHandler; //had been deleted in 1-C 5. Adding the Input Component
	std::shared_ptr<PlayerInputComponent> input;
	std::shared_ptr<HealthComponent> health;/*Deleted 'health' member variable*/
	std::shared_ptr<VelocityComponent> velocity;
	std::shared_ptr<PlayerStateComponent> state;
	std::shared_ptr<PositionComponent> position;
};

