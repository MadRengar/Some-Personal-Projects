#include "../../include/systems/Systems.h"
#include <stdexcept>

ColliderSystem::ColliderSystem() 
{
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::COLLIDER));
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::POSITION));
}

void ColliderSystem::update(Game* game, Entity* entity, float elapsed)
{
    auto colliderComp = entity->getColliderComponent();
    auto posComp = entity->getPositionComp();

    if (!colliderComp || !posComp)
        throw std::runtime_error("Components do not exist");

    colliderComp->update(*entity);
}