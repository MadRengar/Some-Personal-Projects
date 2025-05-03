#pragma once
#include "Entity.h"
#include "../components/TTLComponent.h"
#include "../components/VelocityComponent.h"
#include "../components/ColliderComponent.h"
#include "../components/PositionComponent.h"

class Fire : public Entity
{
public:
	const int startTimeToLive = 150; //frames

	Fire();

	std::shared_ptr<VelocityComponent> getVelocityComp() { return velocity; }

	virtual void update(Game* game, float elapsed = 1.0f) override;

	int getTTL() const { return ttl->getTTL(); }
	
	std::shared_ptr<BoxColliderComponent> getColliderComponent() override { return collider; }// Fix bug which can't get fireEneity boxcollider after replacing colliderComponent

	std::shared_ptr<TTLComponent> getTTLComponent() override;

	std::shared_ptr<VelocityComponent> getVelocityComponent() override;

private:
	std::shared_ptr<TTLComponent> ttl;
	std::shared_ptr<VelocityComponent> velocity;
};

