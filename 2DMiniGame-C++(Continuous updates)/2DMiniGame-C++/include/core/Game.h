#include "../graphics/Window.h"
#include "../core/Board.h"
#include "../entities/Player.h"

class InputHandler;

class Game
{
public:

	const int spriteWH = 50;
	const float tileScale = 1.4f;
	const float itemScale = 0.7f;

	Game();
	~Game();

	void init(std::vector<std::string> lines);
	void addEntity(std::shared_ptr<Entity> newEntity);

	void buildBoard(size_t width, size_t height);
	void initWindow(size_t width, size_t height);

	void handleInput();
	void update(float elapsed);
	void render(float elapsed);
	Window* getWindow() { return &window; }

	sf::Time getElapsed() const;
	void setFPS(int FPS);
	void togglePause() { paused = !paused; }
	bool isPaused() const { return paused; }

	// Returns the shared pointer of the player
	std::shared_ptr<Player> getPlayer() const { return player; }

	EntityID getIDCounter();
	std::shared_ptr<Entity> getEntity(unsigned int idx);

	template <typename T>
	std::shared_ptr<T> buildEntityAt(const std::string& filename, int col, int row);


private:

	Window window;
	bool paused;
	sf::Clock gameClock;
	sf::Time elapsed;

	// II.A Declare a unique pointer of type Board 
	std::unique_ptr<Board> board;

	// III.D Declare a vector from the standard template library that 
	//       contains shared pointers to Entity classes. Recommended name: entities.
	std::vector<std::shared_ptr<Entity>> entities;

	// III.E Declare a variable of type EntityID (which is declared in Entity.h). This variable will
	//       be incremented by one every time an entity is added to the game.
	EntityID entityCounter;
	
	// pointer to a player object
	std::shared_ptr<Player> player;

	// V.A Declare a unique pointer to an Input Handler object for this class.
	std::unique_ptr<InputHandler> inputHandler;

};

