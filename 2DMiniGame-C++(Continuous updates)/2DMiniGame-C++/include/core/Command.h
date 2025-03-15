#pragma once
#include <memory>

class Game; // forward declaration 
class Player;

class Command
{
public:
	virtual ~Command() = default;
	virtual void execute(Game& game) = 0;
	virtual void execute(Player& player) = 0;
};

class PauseCommand : public Command {
public:
	void execute(Game& game) override;
	void execute(Player& player) override {}
};

class MoveRightCommand : public Command {
public:
	void execute(Player& player) override;
	void execute(Game& game) override{}
};

class MoveDownCommand : public Command {
public:
	void execute(Player& player) override;
	void execute(Game& game) override {}
};

class MoveLeftCommand : public Command {
public:
	void execute(Player& player) override;
	void execute(Game& game) override {}
};

class MoveUpCommand : public Command {
public:
	void execute(Player& player) override;
	void execute(Game& game) override {}
};

class AttackCommand : public Command {
public:
	void execute(Player& player) override;
	void execute(Game& game) override {}
};

class ShoutCommand : public Command {
public:
	void execute(Player& player) override;
	void execute(Game& game) override {}
};
