#pragma once
#include <memory>
#include "Components.h"
class Game;
class PlayerInputHandler;

class InputComponent : public Component
{
public:
	virtual ~InputComponent() = default;
	virtual void update(Game& game) = 0;
	virtual PlayerInputHandler* getInputHandler() const { return nullptr; }
	ComponentID getID() const override { return ComponentID::INPUT; }
};

class PlayerInputComponent : public InputComponent
{
public:
	PlayerInputComponent();
	void update(Game& game) override;
	PlayerInputHandler* getInputHandler() const override {
		return playerInputHandler.get();
	}

private:
	std::unique_ptr<PlayerInputHandler> playerInputHandler;
};