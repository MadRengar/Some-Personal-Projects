#include "../../include/entities/Fire.h"
#include <iostream>

Fire::Fire() : Entity(EntityType::FIRE)
{
	ttl = std::make_unique<TTLComponent>(startTimeToLive);
	velocity = std::make_shared<VelocityComponent>(5.0f);
}

void Fire::update(Game* game, float elapsed)
{
	// XI.C First, update the position of the Fire object by calling the parent Entity::update() function.
	velocity->update(*this, elapsed);
	Entity::update(game, elapsed);

	// XI.D Time to live (Fire::ttl member variable) needs to be reduced by 1 at every frame. If this gets
	//		to 0, the entity must be deleted (here, just setting the deleted flat to ture).
	// 

	ttl->update();
	if (ttl->getTTL() <= 0) { deleted = true;}

}
