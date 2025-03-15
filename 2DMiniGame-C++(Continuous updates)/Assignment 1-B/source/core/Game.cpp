#include "../../include/core/Game.h"
#include "../../include/entities/Fire.h"
#include "../../include/entities/StaticEntities.h"
#include "../../include/core/InputHandler.h"
#include <iostream>

// III.F Add the initialization (to 0) of the entity counter to the initalizers list of this constructor
Game::Game() : paused(false), entityCounter(0)
{
	// V.B: Create the unique pointer to the Input Handler object.
	inputHandler = std::make_unique<InputHandler>();
}

Game::~Game()
{
}

template <typename T>
std::shared_ptr<T> Game::buildEntityAt(const std::string& filename, int col, int row)
{
	auto ent = std::make_shared<T>();
	float x = col * spriteWH * tileScale;
	float y = row * spriteWH * tileScale;
	float cntrFactor = (tileScale - itemScale) * spriteWH * 0.5f;

	ent->setPosition(x + cntrFactor, y + cntrFactor);
	ent->init(filename, itemScale);
	
	return ent;
}

void Game::buildBoard(size_t width, size_t height)
{
	// II.B Instantiate the unique pointer of type "Board". 
	//     Use the constructor of Board that receives the width and the height for the game grid.
	board = std::make_unique<Board>(width, height);
}

void Game::initWindow(size_t width, size_t height)
{
	int wdt = static_cast<int>(width * spriteWH * tileScale);
	int hgt = static_cast<int>(height * spriteWH * tileScale);
	window.setSize(sf::Vector2u(wdt, hgt));
	window.redraw();
}

void Game::init(std::vector<std::string> lines)
{
	// Make sure that the vector of lines is not emtpy. h is the number of lines (height of the level)
	size_t h = lines.size() - 1;
	if (h < 0)
		throw std::exception("No data in level file");
	size_t w = -1;

	// Load the font for the window and set its title
	window.loadFont("font/AmaticSC-Regular.ttf");
	window.setTitle("Mini-Game");
	
	auto it = lines.cbegin();
	int row = 0;
	while (it != lines.cend())
	{
		int col = 0;

		// First iteration: we determine the width of the level (w) by the number of symbols in the first line.
		if(w == -1)
		{
			//The size of the window must be equal to the number of tiles it has, factoring their dimensions.
			w = it->size();
			buildBoard(w, h);
			initWindow(w, h);		
		}
		
		std::string::const_iterator is = it->cbegin();
		while (is != it->cend())
		{
			switch (*is)
			{
			case '.':
			{
				// II.C (1/5) Use the function addTile from Board to add a CORRIDOR tile to this position.
				//      The parameters are the column (for x) and row (for y) where the tile is to be placed, the scale 
				//      of the tile (you can use the variable "tileScale") and the tile type. TileTypes are 
				//      defined in the enumerator TileType in Tile.h. You DON'T need to pass a filename parameter for the texture.
				board->addTile(col, row, tileScale,TileType::CORRIDOR, "");
				break;
			}
			case 'w':
			{
				// II.C (2/5) Use the function addTile from Board to add a WALL tile to this position.
				board->addTile(col, row, tileScale, TileType::WALL, "");
				break;
			}
			case 'x':
			{
				// reate a Log entity and add to the game
				auto ent = buildEntityAt<Log>("img/log.png", col, row);
				addEntity(ent);
				
				//By default, entities stand on corridors
				// II.C (3/5) Use the function addTile from Board to add a CORRIDOR tile to this position.
				board->addTile(col, row, tileScale, TileType::CORRIDOR, "");
				break;
			}
			case 'p':
			{

				//  Create a Potion entity and add to the game
				auto ent = buildEntityAt<Potion>("img/potion.png", col, row);
				addEntity(ent);
	
				//By default, entities stand on corridors
				// II.C (4/5) Use the function addTile from Board to add a CORRIDOR tile to this position.
				board->addTile(col, row, tileScale, TileType::CORRIDOR, "");
				break;
			}
			case '*':
				{
				// Create the player shared pointer
				player = std::make_shared<Player>();

				// Initialize the sprite sheet
				player->initSpriteSheet("img/DwarfSpriteSheet_data.txt");
				
				// positions the sprite of the player in the board 
				player->positionSprite(row, col, spriteWH, tileScale);

				// Add the player to the vector of entities
				addEntity(player);

				//By default, entities stand on corridors:
				// II.C (5/5) Use the function addTile from Board to add a CORRIDOR tile to this position.
				board->addTile(col, row, tileScale, TileType::CORRIDOR, "");
				break;
				}
			}

			col++; is++;
		}
		row++; it++;
	}
}

void Game::addEntity(std::shared_ptr<Entity> newEntity)
{
	// Increase the counter first; the first entity will have ID = 1.
	entityCounter++;

	// Assign the new ID to the entity.
	newEntity->setID(entityCounter);

	// Add the entity to the vector.
	entities.push_back(newEntity);
}

void Game::handleInput()
{
	// V.C: Call the fucntion that handles the input for the game and retrieve the command returned in a variable.
	//      Then, call the "execute" method of the returned object to run this command.
	if (auto command = inputHandler->handleInput()) {
		command->execute(*this);
	}
	// V.D: Call the function handleInput on the player's object.
	if (player) {
		player->handleInput(*this);
	}

}


void Game::update(float elapsed)
{
	// V.E Only update the game entities if the game is not paused.
	auto it = entities.begin();  // Get iterator pointing to the first entity
	while (it != entities.end() && !paused) // Loop until the end of the vector
	{
		(*it)->update(this, elapsed); // Dereference the iterator and call update()
		++it; // Move to the next entity
	}

		// Collisions block:

		// IX.C: Retrieve a reference to the player's bounding box and run through all entities (using an itereator)  
		//      in the game with a while loop. You don't need to check the player's bounding box to itself, 
		//      so include a check that skips the player entity while looping through the entities vector.
		const Rectangle& playerBox = player->getBoundingBox();
		
		auto colIt = entities.begin();
		while (colIt != entities.end()) {
			// Skip the player entity
			if ((*colIt)->getID() == player->getID()) {
				++colIt;
				continue;
			}

			// IX.D: (Inside the loop) Once you have a different entity to player, retrieve it's bounding box
			//       and check if they intersect.
			const Rectangle& otherBox = (*colIt)->getBoundingBox();
			if (playerBox.intersects(otherBox)) {
				// IX.E: Determine collision type based on the entity's type.
				switch ((*colIt)->getEntityType()) {
				case EntityType::POTION:
				{
					// Dynamically cast the entity to a Potion pointer.
					Potion* potion = dynamic_cast<Potion*>((*colIt).get());
					if (potion)
					{
						// Retrieve the potion's health restore value.
						int restoreValue = potion->getHealth();

						// Add this health to the player.(Modified in 1-C)
						player->getHealthComponent()->changeHealth(restoreValue);

						// Print an informative message.(Modified in 1-C)
						std::cout << "Collide with potion: Restored " << restoreValue << "health. "
							<< " Player health now: " << player->getHealthComponent()->getHealth() << std::endl;

						potion->deleteEntity();
					}
					break;
				}
				case EntityType::LOG:
				{
					// checks if the player is currently playing the 'Attack' animation
					if (player->isAttacking() &&
						player->getSpriteSheet()->getCurrentAnim()->isInAction())
					{
						Log* log = dynamic_cast<Log*>((*colIt).get());
						if (log)
						{
							int woodValue = log->getWood(); // retrieve the wood value
							player->addWood(woodValue); // add wood to the player

							std::cout << "Chopped log: Collected " << woodValue << std::endl;

							log->deleteEntity();
						}
					}
					break;
				}
			}
		}
		++colIt;
	}							

		// X.D Write a loop that iterates through all entities and removes them from the vector of entities.
		//     Use the function erase from std::vector, which receives an iterator. 
		//     Q? Should you ALWAYS advance the iterator in this loop?
		auto eraseIt = entities.begin();
		while (eraseIt != entities.end()) {
			if ((*eraseIt)->isDeleted()) {
				eraseIt = entities.erase(eraseIt); // erase() 會返回新的 iterator
			}
			else {
				++eraseIt;
			}
		}

	//Update the window for refreshing the graphics (leave this OUTSIDE the !paused block)
	window.update();
}

void Game::render(float elapsed)
{
	//Empty the screen
	window.beginDraw();

	// II.D Call the draw method of the board object passing a pointer to the window.
	board->draw(&window);

	// Draw all entities
	for (const auto& entity : entities)
	{
		entity->draw(&window);
	}

	//Draw FPS
	window.drawGUI(*this);

	//Close up for this frame.
	window.endDraw();
}


sf::Time Game::getElapsed() const { return gameClock.getElapsedTime(); }


void Game::setFPS(int FPS)
{
	std::string text("FPS: " + std::to_string(FPS));
	window.getFPSText().setString(text);
}

// Return the current ID counter.
EntityID Game::getIDCounter()
{
	return entityCounter;
}

std::shared_ptr<Entity> Game::getEntity(unsigned int idx)
{
	// Return the corresponding shared pointer
	if (idx < entities.size())
		return entities[idx];
	else
		throw std::out_of_range("Invalid entity index");
}
