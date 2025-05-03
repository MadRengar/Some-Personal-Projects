#include "../../include/components/PlayerStateComponent.h"
#include "../../include/entities/Fire.h"
#include "../../include/core/Game.h"
#include "../../include/components/GraphicsComponent.h"
#include "../../include/core/ServiceLocator.h"
#include "../../include/core/SFMLAudioPlayer.h"
#include <iostream>

void PlayerStateComponent::update(Entity* entity, Game* game, float elapsed)
{
    Player* player = dynamic_cast<Player*>(entity);
    if (!player) return;

    auto graphics = player->getGraphicsComponent();

    if (attacking) {
        graphics->setAnimation("Attack", true, false);
        if (!graphics->isAnimationPlaying()) {
            game->playSound("Attack.mp3");
            attacking = false;
        }
        return;
    }

    if (shouting) {
        graphics->setAnimation("Shout", true, false);

        if (graphics->isAnimationInAction() && wood >= shootingCost && shootCooldown <= 0) {
            game->notifyShout(); // Shout Achievement
            std::shared_ptr<Fire> fire = createFire(*player, graphics, game);
            //game->addEntity(fire); // Adding logic is moved to Game.cpp Update(), otherwise progress will be crash, cause vector container is modified at same time
            wood -= shootingCost;
            shootCooldown = shootCooldownTime;
        }

        if (!graphics->isAnimationPlaying()) {
            game->playSound("Fire.mp3");
            shouting = false;
        }
        return;
    }

    auto velocity = player->getVelocityComponent();
    if (velocity->getVel().x > 0) {
        graphics->setAnimation("Walk", true, true);
        graphics->setSpriteDirection(Direction::Right);
    }
    else if (velocity->getVel().x < 0) {
        graphics->setAnimation("Walk", true, true);
        graphics->setSpriteDirection(Direction::Left);
    }
    else if (velocity->getVel().y > 0 || velocity->getVel().y < 0) {
        graphics->setAnimation("Walk", true, true);
    }
    else {
        graphics->setAnimation("Idle", true, true);
    }

    if (shootCooldown > 0) {
        shootCooldown -= elapsed;
    }
}

void PlayerStateComponent::addWood(int w)
{
    wood += w;
    if (wood > maxWood) wood = maxWood;
    if (wood < 0) wood = 0;
}

std::shared_ptr<Fire> PlayerStateComponent::createFire(Player& player, std::shared_ptr<GraphicsComponent> graphics, Game* game) const
{
    //auto fireEntity = std::make_shared<Fire>();
    auto fireEntity = game->getFirePool()->acquire();
    if (!fireEntity) {
        std::cerr << "[Pool] No available Fire objects!\n";
        return nullptr;
    }
    Vector2f pos = player.getPosition() + Vector2f(player.getTextureSize().x * 0.5f, player.getTextureSize().y * 0.5f);
    Vector2f vel(fireSpeed, 0.f);

    //fireEntity->init("img/fire.png", std::make_shared<SpriteGraphicsComponent>(1.0f));
    fireEntity->setPosition(pos.x, pos.y);
    
    if (graphics->getSpriteDirection() == Direction::Left) {
        vel.x = -fireSpeed;
    }

    fireEntity->getVelocityComp()->setVel(vel.x, vel.y);
    fireEntity->setActive(true);
    return fireEntity;
}