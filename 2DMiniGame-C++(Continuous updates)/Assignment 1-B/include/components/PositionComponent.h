#pragma once
#include "../../include/utils/Vector2.h"

class PositionComponent {
public:
    // Constructor with default values
    PositionComponent(float x = 0.0f, float y = 0.0f) : position(x, y) {}

    // Getter for position (returns a constant reference)
    const Vector2f& getPosition() const { return position; }

    // Setter for position
    void setPosition(float x, float y) {
        position.x = x;
        position.y = y;
    }

private:
    Vector2f position;  // Stores the x, y coordinates
};