#include "../../include/systems/Systems.h"
#include "../../include/components/VelocityComponent.h"
#include "../../include/components/PositionComponent.h"
#include "../../include/entities/Entity.h"
#include <stdexcept>

MovementSystem::MovementSystem() 
{
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::POSITION));
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::VELOCITY));
}

void MovementSystem::update(Game* game, Entity* entity, float elapsed)
{
	auto velocity = entity->getVelocityComponent();
	auto position = entity->getPositionComp();

	if (!velocity || !position) {
		throw std::runtime_error("MovementSystem: Missing velocity or position component");
	}

	Vector2f vel = velocity->getVel();
	Vector2f pos = position->getPosition();
	float speed = velocity->getSpeed();

	position->setPosition(pos.x + vel.x * speed * elapsed, pos.y + vel.y * speed * elapsed);
}