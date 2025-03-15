#pragma once
#include "Window.h"
#include "Vector2.h"
#include "Entity.h"
#include <iostream>

class Game
{
public:
	Game(int entityCount, float entitySpeed = 0.1f);

	~Game();

	void userInput();

	void update(float deltaTime);

	void render();

	Window* getWindow(); // Return the pointer to the 'Window'

	const sf::Vector2f& getEntityPosition(int index) const;

	sf::Time getElapsedTime() const; // Obtain timestamp

	void setFPS(int fps);

	void addEntity(const sf::Vector2f& position, const sf::Vector2f& direction, float speed);

private:
	// void updateMushroomMovement();

	Window applicationWindow;
	//sf::Vector2f mushroomMovement;
	//sf::Texture mushroomTexture;
	//sf::Sprite mushroomSprite;
	//Entity* mushroom;
	std::vector<Entity*> entities; // Store multiple Entity pointers

	bool isPaused; // Flag - pause the game

	sf::Clock gameClock; // Timer
};

