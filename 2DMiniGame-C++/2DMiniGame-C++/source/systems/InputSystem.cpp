#include "../../include/systems/Systems.h"
#include <stdexcept>

InputSystem::InputSystem()
{
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::INPUT));
	componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::VELOCITY));
}

void InputSystem::update(Game* game, Entity* entity, float elapsed) 
{
    auto input = entity->getInputComponent();
    auto velocity = entity->getVelocityComponent();

    if (!input || !velocity) return;

    velocity->setVel(0.f, 0.f);

    if (auto handler = input->getInputHandler()) {
        auto& commands = handler->handleInput();
        for (auto& cmd : commands) {
            cmd->execute(*game);
        }
    }
}