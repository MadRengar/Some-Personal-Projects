#include "../../include/systems/Systems.h"
#include <stdexcept>

GraphicsSystem::GraphicsSystem()
{
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::GRAPHICS));
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::POSITION));
}

void GraphicsSystem::update(Game* game, Entity* entity, float elapsed)
{
    auto graphicsComp = entity->getGraphicsComponent();
    auto pos = entity->getPositionComp();

    if (!graphicsComp) { throw std::runtime_error("Entity does not have Graphics component!"); }

    graphicsComp->update(*entity, elapsed);
}