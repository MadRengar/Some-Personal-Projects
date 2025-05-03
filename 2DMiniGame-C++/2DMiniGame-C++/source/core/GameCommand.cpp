#include "../../include/core/Command.h"
#include "../../include/core/Game.h"

void PauseCommand::execute(Game& game)
{
	game.togglePause();
}

void SwapInputModeCommand::execute(Game& game)
{
	auto player = game.getPlayer();
	if (!player) return;
	auto input = player->getInputComponent();
	auto handler = input->getInputHandler();
	if (handler) {
		handler->toggleInputMode();
	}
}
