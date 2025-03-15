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
	std::shared_ptr<Player> playerPtr = game.getPlayer();
	Player* player = playerPtr.get();

	if (player)
	{
		auto velocityComp = player->getVelocityComp();
		velocityComp->setVel(0.0f, 0.0f);
	}
	
	auto& commands = playerInputHandler->handleInput();

	for (auto& command : commands)
	{
		command->execute(*player);
	}
}
