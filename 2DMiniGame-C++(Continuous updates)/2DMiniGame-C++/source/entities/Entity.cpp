#include "../../include/entities/Entity.h"
#include "../../include/graphics/Window.h"
#include "../../include/components/PositionComponent.h"
#include <iostream>


Entity::Entity() :
	velocity(0, 0),
	speed(1),
	isSpriteSheet(false),
	id(0),
	type(EntityType::UNDEFINED),
	// X.B (1/2) Add the initialization the deleted flag to false
	deleted(false)
{
	position = std::make_shared<PositionComponent>();
}

Entity::Entity(EntityType et) : 
	velocity(0, 0), 
	speed(1), 
	isSpriteSheet(false),
	id(0),
	type (et),
	// X.B (2/2) Add the initialization the deleted flag to false
	deleted(false)
{
	position = std::make_shared<PositionComponent>();
}

Entity::~Entity()
{
}

void Entity::update(Game* game, float elapsed)
{
	// Update the position 
	Vector2f newPosition = position->getPosition();
	newPosition.x += velocity.x * speed * elapsed;
	newPosition.y += velocity.y * speed * elapsed;
	position->setPosition(newPosition.x, newPosition.y);

	if (isSpriteSheet)
	{
		// Set the sprite position of the spritesheet to the position of this entity.
		spriteSheet.setSpritePosition(sf::Vector2f(newPosition.x, newPosition.y));

		// Call update passing delta
		spriteSheet.update(elapsed);
	}
	else
	{
		// If no spritesheet, update the sprite position.
		sprite.setPosition(sf::Vector2f(newPosition.x, newPosition.y));
	}


	// VIII.A  The bounding box of an entity has the same dimensions as the texture of the sprite
	//		   or spritesheet. This is calculated in the init() functions (see below in this file)
	//		   and the size is stored in the variable "bboxSize". 
	//		   The member variable boundingBox is a Rectangle where we'll hold this boundary box. 
	//		   Set the top left corner of this rectangle to the position of this entity.
	//		   Set the bottom right corner of this rectangle to the position+bboxSize coordinates.
	boundingBox.setTopLeft(newPosition);
	boundingBox.setBottomRight(newPosition + bboxSize);
}



void Entity::draw(Window* window)
{
	if (isSpriteSheet)
	{
		sf::Sprite* sp = &spriteSheet.getSprite();
		const sf::Vector2f pos = sp->getPosition();
		window->draw(spriteSheet.getSprite());
	}
	else
		window->draw(sprite); 

	// VIII.B Draw the bounding box by retrieving a drawable rect from the bounding box Rectangle.
	window->draw(boundingBox.getDrawableRect());

}

void Entity::init(const std::string& textureFile, float scale)
{
	texture.loadFromFile(textureFile);
	sprite.setTexture(texture);
	sprite.setScale(scale, scale);
	bboxSize = Vector2f(texture.getSize().x * sprite.getScale().x, texture.getSize().y * sprite.getScale().y);
}

void Entity::initSpriteSheet(const std::string& spriteSheetFile)
{
	spriteSheet.loadSheet(spriteSheetFile);
	isSpriteSheet = true;
	spriteSheet.setAnimation("Idle", true, true);
	bboxSize = Vector2f(spriteSheet.getSpriteSize().x * spriteSheet.getSpriteScale().x,
					  spriteSheet.getSpriteSize().y * spriteSheet.getSpriteScale().y);
}

const Vector2f& Entity::getPosition() const {
	return position->getPosition();
}

void Entity::setPosition(float x, float y) {
	position->setPosition(x, y);

	if (isSpriteSheet)
		spriteSheet.getSprite().setPosition(x, y);
	else
		sprite.setPosition(x, y);
}


const sf::Vector2f& Entity::getSpriteScale() const
{
	if (isSpriteSheet)
	{
		return spriteSheet.getSpriteScale();
	}

	return sprite.getScale();
}

sf::Vector2i Entity::getTextureSize() const
{
	if (isSpriteSheet)
	{
		return spriteSheet.getSpriteSize();
	}

	return { static_cast<int>(texture.getSize().x), static_cast<int>(texture.getSize().y) };
}
