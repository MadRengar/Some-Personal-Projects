#pragma once
#include "Entity.h"
#include "../components/PositionComponent.h"
#include "../components/ColliderComponent.h"

class Potion : public Entity
{
public:
	Potion() : Entity(EntityType::POTION) {}
	~Potion() {}

	void init(const std::string& textureFile, std::shared_ptr<GraphicsComponent> gc) override
	{
		Entity::init(textureFile, gc);
		//collider = std::make_shared<BoxColliderComponent>();
		// VIII.C (1/2) Set the top left and bottom right corners of the bounding box for this entity.
		collider->setBoundingBox(position->getPosition(), position->getPosition() + collider->getSize());
	}
	std::shared_ptr<BoxColliderComponent> getColliderComponent() override { return collider; }
	virtual void update(Game* game, float elapsed = 1.0f) override 
	{ 
		Entity::update(game, elapsed);
	}

	int getHealth() const { return potionHealth; }

protected:
	const int potionHealth = 10;
	//std::shared_ptr<BoxColliderComponent> collider;
};


class Log : public Entity
{
public:
	Log() : Entity(EntityType::LOG) {}
	~Log() {}

	void init(const std::string& textureFile, std::shared_ptr<GraphicsComponent> gc) override
	{
		Entity::init(textureFile, gc);

		// VIII.C (2/2) Set the top left and bottom right corners of the bounding box for this entity.
		//collider = std::make_shared<BoxColliderComponent>();
		collider->setBoundingBox(position->getPosition(), position->getPosition() + collider->getSize());
	}
	std::shared_ptr<BoxColliderComponent> getColliderComponent() override { return collider; }
	virtual void update(Game* game, float elapsed = 1.0f) override 
	{ 
		Entity::update(game, elapsed); 
	}

	int getWood() const { return woodAdded; }

protected:
	const int woodAdded = 15;
	//std::shared_ptr<BoxColliderComponent> collider;
};