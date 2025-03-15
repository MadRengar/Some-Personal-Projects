#pragma once
#include <memory>
class Game;
class PlayerInputHandler;

class InputComponent
{
public:
	virtual ~InputComponent() = default;
	virtual void update(Game& game) = 0;
};

class PlayerInputComponent : InputComponent
{
public:
	PlayerInputComponent();
	void update(Game& game) override;
private:
	std::unique_ptr<PlayerInputHandler> playerInputHandler;
};