#pragma once
#include "Entity.h"
#include "../components/PositionComponent.h"

class Potion : public Entity
{
public:
	Potion() : Entity(EntityType::POTION) {}
	~Potion() {}

	void init(const std::string& textureFile, float scale) override
	{
		Entity::init(textureFile, scale);

		// VIII.C (1/2) Set the top left and bottom right corners of the bounding box for this entity.
		boundingBox.setTopLeft(position->getPosition());
		boundingBox.setBottomRight(position->getPosition() + bboxSize);
	}

	virtual void update(Game* game, float elapsed = 1.0f) override { }

	int getHealth() const { return potionHealth; }

protected:
	const int potionHealth = 10;
};


class Log : public Entity
{
public:
	Log() : Entity(EntityType::LOG) {}
	~Log() {}

	void init(const std::string& textureFile, float scale) override
	{
		Entity::init(textureFile, scale);

		// VIII.C (2/2) Set the top left and bottom right corners of the bounding box for this entity.
		boundingBox.setTopLeft(position->getPosition());
		boundingBox.setBottomRight(position->getPosition() + bboxSize);
	}

	virtual void update(Game* game, float elapsed = 1.0f) override {}

	int getWood() const { return woodAdded; }

protected:
	const int woodAdded = 15;
};