#include "../graphics/Window.h"
#include "../core/Board.h"
#include "../entities/Player.h"
#include "../systems/Systems.h"
#include "../../include/core/ServiceLocator.h"
#include "../../include/core/SFMLAudioPlayer.h"
#include "../utils/ObjectPool.h"
#include "../entities/Fire.h"
#include "../entities/StaticEntities.h"
#include <functional>
#include <map>

using CollisionCallback = std::function<void(Game*, Entity*)>;
extern std::map<EntityType, CollisionCallback> collisionCallbacks; // Add extern to avoid: Duplicate definition of global variables

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
	void placeEntityFromPool(std::shared_ptr<T> ent, const std::string& filename, int col, int row, std::shared_ptr<GraphicsComponent> gc);

	void bigArray(float elapsed);

	/*Achievement System£¨Observer£©*/
	void notifyPotionCollected();
	void notifyShout();

	void playSound(const std::string& soundName);

	ObjectPool<Fire>* getFirePool() { return firePool.get(); }

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

	/*Assignment1-D*/
	std::vector<std::shared_ptr<System>> systems;

	std::unique_ptr<ObjectPool<Fire>> firePool;
	std::unique_ptr<ObjectPool<Potion>> potionPool;
	std::unique_ptr<ObjectPool<Log>> logPool;
};

