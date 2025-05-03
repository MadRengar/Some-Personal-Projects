#include "../../include/entities/Fire.h"
#include <iostream>

Fire::Fire() : Entity(EntityType::FIRE)
{
	//ttl = std::make_unique<TTLComponent>(startTimeToLive); // change unique to shared
	ttl = std::make_shared<TTLComponent>(startTimeToLive);
	addComponent(ttl);
	velocity = std::make_shared<VelocityComponent>(1.0f);
	addComponent(velocity);
	position = std::make_shared<PositionComponent>();
	addComponent(position);
	collider = std::make_shared<BoxColliderComponent>();
	addComponent(collider);
}

void Fire::update(Game* game, float elapsed)
{
	//velocity->update(*this, elapsed); // Assignment1-D need to delete this update, cause this logic is handled by ECS
	Entity::update(game, elapsed);
	//ttl->update();
	//if (ttl->getTTL() <= 0) { deleted = true;}
	
}

std::shared_ptr<TTLComponent> Fire::getTTLComponent() { return ttl; }

std::shared_ptr<VelocityComponent> Fire::getVelocityComponent() { return velocity; }
