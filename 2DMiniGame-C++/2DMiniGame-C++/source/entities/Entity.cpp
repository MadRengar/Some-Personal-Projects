#include "../../include/entities/Entity.h"
#include "../../include/graphics/Window.h"
#include "../../include/components/PositionComponent.h"
#include <iostream>


Entity::Entity() :
	velocity(0, 0),
	speed(1),
	id(0),
	type(EntityType::UNDEFINED),
	// X.B (1/2) Add the initialization the deleted flag to false
	active(true)
{
	position = std::make_shared<PositionComponent>();
	collider = std::make_shared<BoxColliderComponent>();
}

Entity::Entity(EntityType et) : 
	velocity(0, 0), 
	speed(1), 
	id(0),
	type (et),
	// X.B (2/2) Add the initialization the deleted flag to false
	active(true)
{
	position = std::make_shared<PositionComponent>();
	collider = std::make_shared<BoxColliderComponent>();
}

Entity::~Entity()
{
}

void Entity::update(Game* game, float elapsed)
{
	if (graphics)
	{
		graphics->update(*this, elapsed);
	}
	// VIII.A  The bounding box of an entity has the same dimensions as the texture of the sprite
	//		   or spritesheet. This is calculated in the init() functions (see below in this file)
	//		   and the size is stored in the variable "bboxSize". 
	//		   The member variable boundingBox is a Rectangle where we'll hold this boundary box. 
	//		   Set the top left corner of this rectangle to the position of this entity.
	//		   Set the bottom right corner of this rectangle to the position+bboxSize coordinates.
	
	//if (collider) {
	//	collider->update(*this);
	//}
}

void Entity::draw(Window* window)
{
	if (graphics)
	{
		graphics->draw(window);
	}
	// VIII.B Draw the bounding box by retrieving a drawable rect from the bounding box Rectangle.
	//window->draw(boundingBox.getDrawableRect());
	collider->draw(window);
}

void Entity::init(const std::string& textureFile, std::shared_ptr<GraphicsComponent> gc)
{
	graphics = gc;
	graphics->init(textureFile);

	Vector2f bboxSize = Vector2f(graphics->getTextureSize().x * graphics->getSpriteScale().x,
		graphics->getTextureSize().y * graphics->getSpriteScale().y);
	collider->init(bboxSize);
}

const Vector2f& Entity::getPosition() const {
	return position->getPosition();
}

void Entity::setPosition(float x, float y) {
	position->setPosition(x, y);

	if (graphics) {
		graphics->update(*this, 0);
	}
}

const sf::Vector2f& Entity::getSpriteScale() const
{
	return graphics->getSpriteScale();
}

sf::Vector2i Entity::getTextureSize() const
{
	return graphics->getTextureSize();
}

void Entity::addComponent(std::shared_ptr<Component> comp)
{
	auto id = comp->getID();
	componentSet.turnOnBit(static_cast<unsigned int>(id));
}

