#pragma once
#include "../graphics/Window.h"
#include "../graphics/SpriteSheet.h"
#include "../utils/Rectangle.h"
#include "../components/ColliderComponent.h"
#include "../components/GraphicsComponent.h"
#include "../utils/Bitmask.h"
#include "../components/TTLComponent.h"
#include "../components/InputComponent.h"
#include "../components/VelocityComponent.h"
#include "../components/GraphicsComponent.h"
#include "../components/ColliderComponent.h"


using EntityID = unsigned int;
enum class EntityType
{
	UNDEFINED = -1,
	PLAYER = 0,
	POTION = 1,
	LOG = 2,
	FIRE = 3
};

class Game; //forward declaration
class PositionComponent;
class BoxColliderComponent;
class GraphicsComponent;
class VelocityComponent;
class LogicComponent;

class Entity
{
public:

	//Constructors and Desctrutors
	Entity();
	Entity(EntityType et);
	~Entity();

	//Init and update functions
	//virtual void init(const std::string& textureFile, float scale);
	virtual void init(const std::string& textureFile, std::shared_ptr<GraphicsComponent> gc);
	//void initSpriteSheet(const std::string& spriteSheetFile);
	virtual void update(Game* game, float elapsed = 1.0f);
	void draw(Window* window);

	//Getters and Setters
	void setID(EntityID entId) { id = entId; }
	EntityID getID() const { return id; }

	void setPosition(float x, float y);
	const Vector2f& getPosition() const;
	
	const sf::Vector2f& getSpriteScale() const;
	sf::Vector2i getTextureSize() const;
	EntityType getEntityType() const { return type; }
	//const SpriteSheet* getSpriteSheet() const { return &spriteSheet; }

	
	// X.C  Add two helper functions. One that returns the value of the deleted flag, another one that 
	//      "deletes" the entity by setting this flag to true. (Q: one of this functions should be "const", which one?).
	//bool isDeleted() const{ return deleted; }
	//void deleteEntity() { deleted = true; }

	/*Assignment1-D*/
	Bitmask getComponentSet() const { return componentSet; }

	void addComponent(std::shared_ptr<Component> comp);

	bool hasComponent(Bitmask mask) const { return componentSet.contains(mask); }

	virtual std::shared_ptr<TTLComponent> getTTLComponent() { return nullptr; }
	virtual std::shared_ptr<InputComponent> getInputComponent() { return nullptr; }
	std::shared_ptr<PositionComponent> getPositionComp() { return position; }
	std::shared_ptr<GraphicsComponent> getGraphicsComponent() { return graphics; }
	virtual std::shared_ptr<BoxColliderComponent> getColliderComponent() { return nullptr; }
	virtual std::shared_ptr<VelocityComponent> getVelocityComponent() { return nullptr; }
	virtual std::shared_ptr<LogicComponent> getLogicComponent() const { return nullptr; }

	void setActive(bool val) { active = val; }
	bool isActive() { return active; }
protected:

	EntityType type;
	EntityID id;

	//Position and velocity
	std::shared_ptr<PositionComponent> position;
	Vector2f velocity;
	float speed;

	//Collision
	std::shared_ptr<BoxColliderComponent> collider;

	//Graphics-related variables.
	std::shared_ptr<GraphicsComponent> graphics;

	// X.A Add a bool member variable "deleted" to this class.
	//bool deleted;

	Bitmask componentSet;

	//ObjectPool Entity Flag
	bool active = true;
};