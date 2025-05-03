#pragma once
#include "../utils/Vector2.h"
#include "../../include/entities/Entity.h"
#include "Components.h"

class VelocityComponent : public Component
{
private:
    Vector2f velocity;
    float speed;

public:
    // Constructor with default speed of 1.0
    VelocityComponent(float speed = 1.f) : speed(speed), velocity(0.f, 0.f) {}

    // Set velocity
    void setVel(float x, float y) { 
        velocity.x = x; 
        velocity.y = y; 
    }

    // Get velocity vector
    const Vector2f& getVel() const { return velocity; }
    float getSpeed() const { return speed; }

    // Update function
    void update(Entity& entity, float elapsed);

    virtual ComponentID getID() const override { return ComponentID::VELOCITY; }
};