#include "../../include/components/PositionComponent.h"
#include "../../include/components/VelocityComponent.h"
#include "../../include/entities/Entity.h"

void VelocityComponent::update(Entity& entity, float elapsed) {
    auto positionComp = entity.getPositionComp();

    // calculate new position
    Vector2f newPosition = positionComp->getPosition();
    newPosition.x += velocity.x * speed * elapsed;
    newPosition.y += velocity.y * speed * elapsed;

    // update PositionComponent
    positionComp->setPosition(newPosition.x, newPosition.y);
}