#include "Game.h"

Game::Game(int entityCount, float entitySpeed)
	: applicationWindow("Game Window", sf::Vector2u(800, 600)),
	isPaused(false) {
	// Create Entity Instance
	for (int i = 0; i < entityCount; i++)
	{
		entities.push_back(new Entity("mushroom50-50.png", &applicationWindow, entitySpeed, 2.0f));
	}
}

Game::~Game()
{ 
	//delete mushroom;
	for (Entity* entity : entities) {
		delete entity; 
	}
	entities.clear(); // clear vector
}

// Fundamental structure
void Game::userInput()
{
	/*Pause*/
	static bool spacePressed = false;
	static bool aPressed = false;

	if (sf::Keyboard::isKeyPressed(sf::Keyboard::Space))
	{
		if (!spacePressed)
		{
			isPaused = !isPaused;
			// std::cout << "Game " << (isPaused ? "Paused" : "Resumed") << std::endl;
		}
		spacePressed = true;
	}
	else
	{
		spacePressed = false;
	}

	if (sf::Keyboard::isKeyPressed(sf::Keyboard::A)) {
		if (!aPressed) {
			sf::Vector2f randomPosition(rand() % 800, rand() % 600);
			float angle = static_cast<float>(rand()) / RAND_MAX * 2 * 3.14159265f;
			sf::Vector2f randomDirection(std::cos(angle), std::sin(angle));
			float randomSpeed = 1.0f + static_cast<float>(rand() % 50);

			addEntity(randomPosition, randomDirection, randomSpeed);
		}
		aPressed = true;
	}
	else {
		aPressed = false;
	}
}

void Game::addEntity(const sf::Vector2f& position, const sf::Vector2f& direction, float speed)
{
	Entity* newEntity = new Entity("mushroom50-50.png", &applicationWindow, speed, 2.0f);
	
	entities.push_back(newEntity);
	
	entities.back()->update(0);
	std::cout << "Entity added! Total entities: " << entities.size() << std::endl;
}


void Game::update(float deltaTime)
{
	/*Entity update+window update*/
	for (auto it = entities.begin(); it != entities.end();)
	{
		(*it)->update(deltaTime);
		(*it)->moveMushroom(applicationWindow);
		if ((*it)->getCreateTime() >= (*it)->getTimeToLive())
		{
			delete* it;
			it = entities.erase(it);
		}
		else {
			++it;
		}
	}
	applicationWindow.update();
}

void Game::render()
{
	applicationWindow.beginDraw();
	//applicationWindow.draw(mushroomSprite); 
	for (Entity* entity : entities) {
		entity->draw(applicationWindow);
	}
	applicationWindow.drawFps(applicationWindow.getGUIText());
	applicationWindow.endDraw();
}

Window* Game::getWindow()
{
	return &applicationWindow;
}

sf::Time Game::getElapsedTime() const {
	return gameClock.getElapsedTime();
}

const sf::Vector2f& Game::getEntityPosition(int index) const {
	if (index < 0 || index >= static_cast<int>(entities.size())) {
		throw std::runtime_error("Invalid entity index: " + std::to_string(index));
	}
	return entities[index]->getPosition();
}

void Game::setFPS(int fps)
{
	applicationWindow.getGUIText().setString("FPS: " + std::to_string(fps));
}