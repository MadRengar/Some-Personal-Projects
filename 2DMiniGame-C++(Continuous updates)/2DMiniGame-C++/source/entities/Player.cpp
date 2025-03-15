#include "../../include/entities/Player.h"
#include "../../include/graphics/AnimBase.h"
#include "../../include/entities/Fire.h"
#include "../../include/core/Game.h"
#include "../../include/components/InputComponent.h"
#include "../../include/components/PositionComponent.h"
#include <iostream>

Player::Player() : Entity(EntityType::PLAYER), attacking(false), shouting(false), wood(0), shootCooldown(0)
{

	// VI.B: Create the unique pointer to the PlayerInputHandler object
	//playerInputHandler = std::make_unique<PlayerInputHandler>();  //had been deleted in 1-C 5. Adding the Input Component
	health = std::make_shared<HealthComponent>(startingHealth, maxHealth);
	playerInputComponent = std::make_unique<PlayerInputComponent>();
	velocity = std::make_shared<VelocityComponent>(playerSpeed);
}

Player::~Player() {}

void Player::update(Game* game, float elapsed)
{
	Entity::update(game, elapsed);

	velocity->update(*this, elapsed);
		// VI.G Modify the code below to add the functionality to play the appropriate animations 
		//      and set the appropriate directions for movement depending on the  value of the
		//      velocity vector for moving up, down and left.


		// VI.F (1/2) If the X component of the velocity vector is positive, we're moving to the right.
		//            Set the animation of the spritesheet to "Walk". Mind the parameters required for the
		//			  animation: if it should start playing and if it should loop.
		//			  Additionally, you must also set the sprite direction (to Direction::Right) of the spritesheet.

		// VI.F (2/2) If the player is not moving, we must get back to playing the "Idle" animation.
	if (attacking) {
		spriteSheet.setAnimation("Attack", true, false);
		if (!spriteSheet.getCurrentAnim()->isPlaying()) {
			attacking = false;
		}
		return;
	}
	if (shouting) {
		spriteSheet.setAnimation("Shout", true, false);

		/*
		If the logic of shouting fireball is implemented at the position of annotation XI, 
		the shooting flag will be reset too early! Causing the shooting flag always be false in every if judgment.
		*/
		if (shouting && getSpriteSheet()->getCurrentAnim()->isInAction() && wood >= shootingCost && shootCooldown <= 0)
		{
			// Create Fire
			std::cout << "£¡" << std::endl;
			std::shared_ptr<Entity> fire = createFire();
			game->addEntity(fire);

			// Deducting woodNum
			wood -= shootingCost;

			//Reset shootCooldown
			shootCooldown = shootCooldownTime;
		}

		if (!spriteSheet.getCurrentAnim()->isPlaying()) {
			shouting = false;
		}
		return;
	}
	if (getVelocityComp()->getVel().x > 0)
	{
		spriteSheet.setAnimation("Walk", true, true);
		spriteSheet.setSpriteDirection(Direction::Right);
	}
	else if (getVelocityComp()->getVel().x < 0)
	{
		spriteSheet.setAnimation("Walk", true, true);
		spriteSheet.setSpriteDirection(Direction::Left);
	}
	else if (getVelocityComp()->getVel().y > 0)
	{
		spriteSheet.setAnimation("Walk", true, true);
	}
	else if (getVelocityComp()->getVel().y < 0)
	{
		spriteSheet.setAnimation("Walk", true, true);
	}
	else // get back to playing the "Idle" animation
	{
		spriteSheet.setAnimation("Idle", true, true);
	}


	
	// IV.D (1/2) Call the function update in the base class to do the general update stuff that is common to all entities.

	// XI.B (2/2):  Reduce the shoot cooldown counter by the elapsed time at every frame. 
	//              Only do this if shoot cooldown is > 0 (can you guess why?)
	if (shootCooldown > 0)
	{
		shootCooldown -= elapsed;
	}
	// XI.A: Create an Fire entity object (using Player::createFire()) and add it to the game (using Game::addEntity).
	//       Then, remove the shooting cost (Player::shootingCost) from the wood member variable of this class
	//       Finally, wrap the functionality below in an IF statement, so we only spawn fire when:
	//            1) We are playing the shouting animation
	//			  2) The animation is in one of the "in action" frames.
	//			  3) We have enough wood "ammunition" (variable wood and shootingCost)

		// XI.B (1/2): Set the variable shootCooldown to the cooldown time (defined in shootCooldownTime).
		//        Add another condition to the shooting IF statement that only allows shoowing if shootCooldown <= 0.
	
	
	// VII.B: If we are attacking but the current animation is no longer playing, set the attacking flag to false.
	//        The same needs to be done for "shouting".

}


void Player::handleInput(Game& game)
{
	playerInputComponent->update(game);
	// VI.E Set the velocity of this player to (0, 0)
	//setVelocity({ 0.0f, 0.0f });
	// VI.C: Call the fucntion that handles the input for the player and retrieve the command returned in a variable.
	//       Then, call the "execute" method of the returned object to run this command.
	//if (auto command = playerInputHandler->handleInput()) {
	//	command->execute(*this);
	//}
	// VII.A Modify the code ABOVE so, instead of calling "execute" in a command pointer, iterates through
	//       the vector of commands and executes them all.
	//auto& commands = playerInputHandler->handleInput();
	//for (auto& command : commands)
	//{
	//	command->execute(*this);
	//}
}

std::shared_ptr<Fire> Player::createFire() const
{
	auto fireEntity = std::make_shared<Fire>();		

	Vector2f pos = position->getPosition() + Vector2f(getTextureSize().x * 0.5f, getTextureSize().y * 0.5f );
	fireEntity->init("img/fire.png", 1.0f);
	fireEntity->setPosition(pos.x, pos.y);
	Vector2f vel(fireSpeed, 0.f);
	if (spriteSheet.getSpriteDirection() == Direction::Left)
	{
		vel.x = -fireSpeed;
	}

	fireEntity->getVelocityComp()->setVel(vel.x, vel.y);

	return fireEntity;
}

/*Deleted addHealth()*/

void Player::addWood(int w)
{
	wood += w;
	if (wood > maxWood) wood = maxWood;
	if (wood < 0) wood = 0;
}


void Player::positionSprite(int row, int col, int spriteWH, float tileScale)
{
	sf::Vector2f scaleV2f = getSpriteScale();
	sf::Vector2i textureSize = getTextureSize();

	float x = col * spriteWH * tileScale;
	float y = (row)*spriteWH * tileScale;
	float spriteSizeY = scaleV2f.y * textureSize.y;
	float cntrFactorY = ((spriteWH * tileScale) - spriteSizeY);	// to align to lower side of the tile.
	float cntrFactorX = cntrFactorY * 0.5f;						//to center horizontally

	setPosition(x + cntrFactorX, y + cntrFactorY);
	sprite.setPosition(getPosition().x, getPosition().y);
}
