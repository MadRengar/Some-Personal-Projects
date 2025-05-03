#include "include/core/Command.h"
#include "include/core/Game.h"
#include "include/entities/Player.h"
#include "include/entities/Entity.h"
#include "include/components/VelocityComponent.h"

void MoveRightCommand::execute(Game& game)
{
    auto playerPtr = game.getPlayer();
    if (!playerPtr) return;
    auto velocityComp = playerPtr->getVelocityComponent();
    if (velocityComp) {
        velocityComp->setVel(1.0f, velocityComp->getVel().y);
    }
}

void MoveDownCommand::execute(Game& game)
{
    auto playerPtr = game.getPlayer();
    if (!playerPtr) return;
    auto velocityComp = playerPtr->getVelocityComponent();
    if (velocityComp) {
        velocityComp->setVel(velocityComp->getVel().x, 1.0f);
    }
}

void MoveLeftCommand::execute(Game& game)
{
    auto playerPtr = game.getPlayer();
    if (!playerPtr) return;
    auto velocityComp = playerPtr->getVelocityComponent();
    if (velocityComp) {
        velocityComp->setVel(-1.0f, velocityComp->getVel().y);
    }
}

void MoveUpCommand::execute(Game& game)
{
    auto playerPtr = game.getPlayer();
    if (!playerPtr) return;
    auto velocityComp = playerPtr->getVelocityComponent();
    if (velocityComp) {
        velocityComp->setVel(velocityComp->getVel().x, -1.0f);
    }
}

void AttackCommand::execute(Game& game)
{
    auto playerPtr = game.getPlayer();
    if (!playerPtr) return;

    auto stateComp = playerPtr->getPlayerStateComponent();
    if (stateComp && !stateComp->isAttacking()) {
        stateComp->setAttacking(true);
    }

}
void ShoutCommand::execute(Game& game)
{
    auto playerPtr = game.getPlayer();
    if (!playerPtr) return;

    auto stateComp = playerPtr->getPlayerStateComponent();
    if (stateComp && !stateComp->isShouting()) {
        stateComp->setShouting(true);
    }
}
