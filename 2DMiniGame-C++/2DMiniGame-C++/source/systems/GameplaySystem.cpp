#include "../../include/systems/Systems.h"
#include "../../include/components/PlayerStateComponent.h"
#include <stdexcept>

GameplaySystem::GameplaySystem() {
    componentMask.turnOnBit(static_cast<unsigned int>(ComponentID::PLAYERSTATE));
}

void GameplaySystem::update(Game* game, Entity* entity, float elapsed) {
    auto logic = entity->getLogicComponent();
    if (!logic) {
        throw std::runtime_error("Entity does not have a LogicComponent.");
    }
    logic->update(entity, game, elapsed);
}
