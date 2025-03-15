#include "include/core/Command.h"
#include "include/entities/Player.h"
#include "include/entities/Entity.h"
#include "include/components/VelocityComponent.h"

void MoveRightCommand::execute(Player& player)
{
    auto velocityComp = player.getVelocityComp();
    if (velocityComp)
    {
        velocityComp->setVel(1.0f, velocityComp->getVel().y);
    }
}

void MoveDownCommand::execute(Player& player)
{
    auto velocityComp = player.getVelocityComp();
    if (velocityComp)
    {
        velocityComp->setVel(velocityComp->getVel().x, 1.0f);
    }
}

void MoveLeftCommand::execute(Player& player)
{
    auto velocityComp = player.getVelocityComp();
    if (velocityComp)
    {
        velocityComp->setVel(-1.0f, velocityComp->getVel().y);
    }
}

void MoveUpCommand::execute(Player& player)
{
    auto velocityComp = player.getVelocityComp();
    if (velocityComp)
    {
        velocityComp->setVel(velocityComp->getVel().x, -1.0f);
    }
}

void AttackCommand::execute(Player& player)
{
	if (!player.isAttacking())
	{
		player.setAttacking(true);
	}
}
void ShoutCommand::execute(Player& player)
{
	if (!player.isShouting())
	{
		player.setShouting(true);
	}
}
