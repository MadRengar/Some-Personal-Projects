#pragma once
#include <memory>

class Game; // forward declaration 
class Player;

class Command
{
public:
	virtual ~Command() = default;
	virtual void execute(Game& game) = 0;
};
/*Game Input*/
class PauseCommand : public Command {
public:
	void execute(Game& game) override;
};
class SwapInputModeCommand : public Command
{
public:
	void execute(Game& game) override;
};
/*Player Input*/
class MoveRightCommand : public Command {
public:
	void execute(Game& game) override;
};

class MoveDownCommand : public Command {
public:
	void execute(Game& game) override;
};

class MoveLeftCommand : public Command {
public:
	void execute(Game& game) override;
};

class MoveUpCommand : public Command {
public:
	void execute(Game& game) override;
};

class AttackCommand : public Command {
public:
	void execute(Game& game) override;
};

class ShoutCommand : public Command {
public:
	void execute(Game& game) override;
};
