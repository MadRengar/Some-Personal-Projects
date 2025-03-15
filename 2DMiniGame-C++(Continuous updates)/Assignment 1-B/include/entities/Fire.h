#pragma once
#include "Entity.h"
#include "../components/TTLComponent.h"
#include "../components/VelocityComponent.h"

class Fire : public Entity
{
public:
	const int startTimeToLive = 150; //frames

	Fire();

	std::shared_ptr<VelocityComponent> getVelocityComp() { return velocity; }

	virtual void update(Game* game, float elapsed = 1.0f) override;

	int getTTL() const { return ttl->getTTL(); }

private:

	std::unique_ptr<TTLComponent> ttl;
	std::shared_ptr<VelocityComponent> velocity;
};

