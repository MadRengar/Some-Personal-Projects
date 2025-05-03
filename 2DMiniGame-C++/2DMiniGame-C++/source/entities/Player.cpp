#include "../../include/entities/Player.h"
#include "../../include/graphics/AnimBase.h"
#include "../../include/entities/Fire.h"
#include "../../include/core/Game.h"
#include <iostream>

//had been deleted in 1-C 7. When adding the PlayerStateComponent
Player::Player() : Entity(EntityType::PLAYER)/*, attacking(false), shouting(false), wood(0), shootCooldown(0)*/
{

	// VI.B: Create the unique pointer to the PlayerInputHandler object
	//playerInputHandler = std::make_unique<PlayerInputHandler>();  //had been deleted in 1-C 5. Adding the Input Component
	input = std::make_shared<PlayerInputComponent>();
	addComponent(input);
	health = std::make_shared<HealthComponent>(startingHealth, maxHealth);
	addComponent(health);
	velocity = std::make_shared<VelocityComponent>(playerSpeed);
	addComponent(velocity);
	collider = std::make_shared<BoxColliderComponent>();
	addComponent(collider);
	graphics = std::make_shared<SpriteSheetGraphicsComponent>();
	addComponent(graphics);
	state = std::make_shared<PlayerStateComponent>();
	addComponent(state);
	position = std::make_shared<PositionComponent>();
	addComponent(position);

	/*
	* Since the separation of inputSyetem has not been completed yet, 
	  an initial speed is set to test whether the MovementSystem is successfully separated
	*/
	//velocity->setVel(1.0f, 0.0f); 
}

Player::~Player() {}

void Player::update(Game* game, float elapsed)
{
	Entity::update(game, elapsed);
	//state->update(this, game, elapsed);
	//velocity->update(*this, elapsed);
}


void Player::positionSprite(int row, int col, int spriteWH, float tileScale)
{
	sf::Vector2f scaleV2f = graphics->getSpriteScale();
	sf::Vector2i textureSize = graphics->getTextureSize();

	float x = col * spriteWH * tileScale;
	float y = (row)*spriteWH * tileScale;
	float spriteSizeY = scaleV2f.y * textureSize.y;
	float cntrFactorY = ((spriteWH * tileScale) - spriteSizeY);	
	float cntrFactorX = cntrFactorY * 0.5f;						

	setPosition(x + cntrFactorX, y + cntrFactorY);
}

void Player::onPotionCollision(Game* game, Entity* other) {
	if (!other->isActive()) return;

	Potion* potion = dynamic_cast<Potion*>(other);
	if (!potion) return;

	int restoreValue = potion->getHealth();
	getHealthComponent()->changeHealth(restoreValue);
	std::cout << "Collide with potion: Restored " << restoreValue << "health. "
		<< " Player health now: " << getHealthComponent()->getHealth() << std::endl;


	potion->setActive(false);
	game->notifyPotionCollected();
	game->playSound("potion.mp3");
}

void Player::onLogCollision(Game* game, Entity* other) {
	if (!other->isActive()) return;

	if (getPlayerStateComponent()->isAttacking() &&
		getGraphicsComponent()->getSpriteSheet()->getCurrentAnim()->isInAction())
	{
		Log* log = dynamic_cast<Log*>(other);
		if (log)
		{
			int woodValue = log->getWood(); // retrieve the wood value
			getPlayerStateComponent()->addWood(woodValue); // add wood to the player

			std::cout << "Chopped log: Collected " << woodValue << std::endl;

			//log->deleteEntity();
			log->setActive(false);
		}
	}
}
