#pragma once
#include "../entities/Player.h"
#include "Components.h"
class Fire;
class GraphicsComponent;

class LogicComponent : public Component
{
public:
	virtual void update(Entity* entity, Game* game, float elapsed) = 0;
	virtual ComponentID getID() const override { return ComponentID::PLAYERSTATE; }
};

class PlayerStateComponent : public LogicComponent
{

public:
	PlayerStateComponent() : attacking(false), shouting(false), shootCooldown(0), wood(0) {}

	// Sets the current animation on the graphics component, updates shooting cooldown, 
	// manages fire creation if appropriate and resets flags for attacking and shouting at end of the animations.
	virtual void update(Entity* entity, Game* game, float elapsed) override;

	//Getters and setters for the members of this class.
	bool isAttacking() const { return attacking; }
	bool isShouting() const { return shouting; }
	float getShootCooldown() const { return shootCooldown; }
	int getWood() const { return wood; }
	constexpr int getShootingCost() { return shootingCost; }
	constexpr float getFireSpeed() { return fireSpeed; }
	constexpr float getShootCooldownTime() { return shootCooldownTime; }
	void setAttacking(bool at) { attacking = at; }
	void setShouting(bool sh) { shouting = sh; }
	void setCooldown(float cd) { shootCooldown = cd; }
	
	//Adds "w" amount of wood, respecting the limits (0, maxWood)
	void addWood(int w);


protected:
	//Functio to create fire on the sprite's position.
	std::shared_ptr<Fire> createFire(Player& entity, std::shared_ptr<GraphicsComponent> graphics, Game* game) const;

	//Constants for this component.
	const int maxWood = 100;
	const int shootingCost = 20;
	const float fireSpeed = 200.f;
	const float shootCooldownTime = 3.f; //in seconds

	//Variables for this component.
	int wood;
	bool attacking;
	bool shouting;
	float shootCooldown;

};