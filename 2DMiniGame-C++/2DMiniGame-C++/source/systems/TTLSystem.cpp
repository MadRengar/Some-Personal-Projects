#include "../../include/systems/Systems.h"
#include <stdexcept>

TTLSystem::TTLSystem()
{
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::TTL));
}

void TTLSystem::update(Game* game, Entity* entity, float elapsed)
{
	auto ttl = entity->getTTLComponent();
	if (!ttl) { throw std::runtime_error("Entity does not have TTL component!"); }
	ttl->decreaseTTL();
	if (ttl->getTTL() <= 0)
	{
		//entity->deleteEntity();

		/*Fire ObjectPool*/
		entity->setActive(false);// 1.set flag
		ttl->resetTTL();// 2.reset TTL
	}
}