#include "../../include/core/InputHandler.h"


InputHandler::InputHandler() {
    pauseCommand = std::make_shared<PauseCommand>();
    swapModeCommand = std::make_shared<SwapInputModeCommand>();
}

bool lastEnterPressed = false;
std::shared_ptr<Command> InputHandler::handleInput() {
    bool nowPressed = sf::Keyboard::isKeyPressed(sf::Keyboard::Enter);
    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Escape)) {
        return pauseCommand;
    }
    if (nowPressed && !lastEnterPressed) {
        lastEnterPressed = true;
        return swapModeCommand;
    }
    if (!nowPressed) {
        lastEnterPressed = false;
    }
    return nullptr;
}

PlayerInputHandler::PlayerInputHandler()
    : moveRightCommand(std::make_shared<MoveRightCommand>()),
    moveDownCommand(std::make_shared<MoveDownCommand>()),
    moveLeftCommand(std::make_shared<MoveLeftCommand>()),
    moveUpCommand(std::make_shared<MoveUpCommand>()),
    attackCommand(std::make_shared<AttackCommand>()),
    shoutCommand(std::make_shared<ShoutCommand>()){}


std::vector<std::shared_ptr<Command>>& PlayerInputHandler::handleInput() {
    commands.clear();
    if (mode == InputMode::WASD)
    {
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::D)) {
            commands.push_back(moveRightCommand);
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::A)) {
            commands.push_back(moveLeftCommand);
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::W)) {
            commands.push_back(moveUpCommand);
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::S)) {
            commands.push_back(moveDownCommand);
        }
    }
    else if (mode == InputMode::ARROWS)
    {
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right)) {
            commands.push_back(moveRightCommand);
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left)) {
            commands.push_back(moveLeftCommand);
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up)) {
            commands.push_back(moveUpCommand);
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down)) {
            commands.push_back(moveDownCommand);
        }
    }
    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Space)) {
        commands.push_back(attackCommand);
    }
    if (sf::Keyboard::isKeyPressed(sf::Keyboard::LShift)) {
        commands.push_back(shoutCommand);
    }
    return commands;
}

void PlayerInputHandler::toggleInputMode()
{
    mode = (mode == InputMode::WASD) ? InputMode::ARROWS : InputMode::WASD;
    std::cout << "[Info] Switched to " << ((mode == InputMode::WASD) ? "WASD" : "Arrow") << " input mode.\n";
}
