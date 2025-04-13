#include "../../include/components/ColliderComponent.h"

void BoxColliderComponent::init(const Vector2f& sz)
{
	size = sz;
}

void BoxColliderComponent::update(const Entity& entity) {
    Vector2f entityPos = entity.getPosition();
    boundingBox.setTopLeft(entityPos);
    boundingBox.setBottomRight(entityPos + size);
}

bool BoxColliderComponent::interesects(BoxColliderComponent* cc) {
    return boundingBox.intersects(cc->getBoundingBox());
}

void BoxColliderComponent::draw(Window* window) {
    window->draw(boundingBox.getDrawableRect());
}

void BoxColliderComponent::setBoundingBox(const Vector2f& topLeft, const Vector2f& bottomRight) {
    boundingBox.setTopLeft(topLeft);
    boundingBox.setBottomRight(bottomRight);
}