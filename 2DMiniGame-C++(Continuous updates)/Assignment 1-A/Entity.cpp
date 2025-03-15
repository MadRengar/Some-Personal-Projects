#include "Entity.h"
#include<stdexcept>
#include<iostream>

Entity::Entity(const std::string& texturePath, Window* window, float speed, float ttl)
    : movementSpeed(speed), timeToLive(ttl), createTime(0.0f) {
    const float PI = 3.14159265f;

    if (!texture.loadFromFile(texturePath)) {
        throw std::runtime_error("Failed to load texture: " + texturePath);
    }
    sprite.setTexture(texture);

    if (window)
    {
        sf::Vector2u windowSize = window->getWindowSize();
        sf::Vector2u textureSize = texture.getSize();

        float randX = static_cast<float>(rand() % (windowSize.x - textureSize.x));
        float randY = static_cast<float>(rand() % (windowSize.y - textureSize.y));

        sprite.setPosition(randX, randY);

        float angle = static_cast<float>(rand() % 360) * PI / 180.0f;
        movementDirection.x = cos(angle) * speed;
        movementDirection.y = sin(angle) * speed;
    }
    else {
        throw std::runtime_error("window pointer is nullptr!");
    }    
}

Entity::~Entity(){}

void Entity::draw(Window& window)
{
    window.draw(sprite);
}

void Entity::update(float deltaTime)
{
    sprite.move(movementDirection * deltaTime);
    createTime += deltaTime;
}

void Entity::moveMushroom(Window& window)
{
    sf::Vector2u windowSize = window.getWindowSize();
    sf::Vector2u textureSize = texture.getSize();
    float xRightLimit = windowSize.x - textureSize.x;
    float yBottomLimit = windowSize.y - textureSize.y;

    sf::Vector2f mushroomPosition = sprite.getPosition();

    bool bouncesRight = (mushroomPosition.x > xRightLimit && movementDirection.x > 0);
    bool bouncesLeft = (mushroomPosition.x < 0 && movementDirection.x < 0);
    if (bouncesRight || bouncesLeft) {
        movementDirection.x = -movementDirection.x;
    }

    bool bouncesBottom = (mushroomPosition.y > yBottomLimit && movementDirection.y > 0);
    bool bouncesTop = (mushroomPosition.y < 0 && movementDirection.y < 0);
    if (bouncesBottom || bouncesTop) {
        movementDirection.y = -movementDirection.y;
    }

    sprite.setPosition(
        mushroomPosition.x + movementDirection.x, mushroomPosition.y + movementDirection.y
    );
}

const sf::Vector2f Entity::getPosition() const
{
    return sprite.getPosition();
}

float Entity::getCreateTime() const { return createTime; }
float Entity::getTimeToLive() const { return timeToLive; }
