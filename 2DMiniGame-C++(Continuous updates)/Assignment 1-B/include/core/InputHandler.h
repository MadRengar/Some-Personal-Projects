#pragma once
#include <memory>
#include <vector>
#include <SFML/Window/Keyboard.hpp>
#include "Command.h"

class InputHandler
{
public:
	InputHandler();
	std::shared_ptr<Command> handleInput();
	
private:
	std::shared_ptr<Command> pauseCommand;
};

class PlayerInputHandler
{
public:
    PlayerInputHandler();
    //Command* handleInput();
    std::vector<std::shared_ptr<Command>>& handleInput();


private:
    std::vector<std::shared_ptr<Command>> commands;

    std::shared_ptr<Command> moveRightCommand;
    std::shared_ptr<Command> moveDownCommand;
    std::shared_ptr<Command> moveLeftCommand;
    std::shared_ptr<Command> moveUpCommand;
    std::shared_ptr<Command> attackCommand;
    std::shared_ptr<Command> shoutCommand;
};

