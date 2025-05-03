#pragma once
#include "../utils/Bitmask.h"
#include "../entities/Entity.h"
#include "../components/TTLComponent.h"
#include "../components/InputComponent.h"
#include "../components/VelocityComponent.h"
#include "../components/GraphicsComponent.h"
#include "../components/ColliderComponent.h"
#include "../components/PlayerStateComponent.h"

class Game;//Forward declaration, cna't use include Game.h cause in Game.h also include Systems.h , which will lead redefine, i guess...

class System 
{
public:
	virtual ~System() {}

	bool validate(Entity* entity) //Determine whether the system is applicable to the entity
	{
		if (componentMask.getMask() == 0) return false;// To detect the entity has no component
		return entity->hasComponent(componentMask); // To detect this system is compatible
	}

	virtual void update(Game* game, Entity* entity, float elapsed) = 0; //System main update logic (pure virtual function)

protected:
	Bitmask componentMask;

};

class TTLSystem :public System
{
public:
	TTLSystem();
	void update(Game* game, Entity* entity, float elapsed) override;
};

class InputSystem : public System {
public:
	InputSystem();
	void update(Game* game, Entity* entity, float elapsed) override;
};

class MovementSystem : public System {
public:
	MovementSystem();
	void update(Game* game, Entity* entity, float elapsed) override;
};

class GraphicsSystem : public System {
public:
	GraphicsSystem();
	void update(Game* game, Entity* entity, float elapsed) override;
};

class ColliderSystem : public System {
public:
	ColliderSystem();
	void update(Game* game, Entity* entity, float elapsed) override;
};

class GameplaySystem : public System {
public:
	GameplaySystem();
	void update(Game* game, Entity* entity, float elapsed) override;
};