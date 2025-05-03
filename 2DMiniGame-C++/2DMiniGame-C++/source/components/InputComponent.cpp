#include "../../include/core/Game.h"
#include "../../include/entities/Player.h"
#include "../../include/core/InputHandler.h"
#include "../../include/components/InputComponent.h"
#include "../../include/components/VelocityComponent.h"

PlayerInputComponent::PlayerInputComponent() 
{
	playerInputHandler = std::make_unique<PlayerInputHandler>();
}

void PlayerInputComponent::update(Game& game)
{
    auto playerPtr = game.getPlayer();
    if (playerPtr) {
        auto velocityComp = playerPtr->getVelocityComponent();
        if (velocityComp) {
            velocityComp->setVel(0.f, 0.f);
        }
    }

    auto commands = playerInputHandler->handleInput();
    for (auto& command : commands)
    {
        if (command)
        {
            command->execute(game);
        }
    }
}
