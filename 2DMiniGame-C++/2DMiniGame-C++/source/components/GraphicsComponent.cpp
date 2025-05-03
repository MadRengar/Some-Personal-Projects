#include "../../include/components/GraphicsComponent.h"

// ------------------ SpriteGraphicsComponent ------------------

void SpriteGraphicsComponent::init(const std::string& file) {
    if (!texture.loadFromFile(file)) {
        throw std::runtime_error("Failed to load sprite texture: " + file);
    }
    textureSize = sf::Vector2i(texture.getSize());
    sprite.setTexture(texture);
    sprite.setScale(scale, scale);
}

void SpriteGraphicsComponent::update(const Entity& entity, float elapsed) {
    sprite.setPosition(entity.getPosition().x, entity.getPosition().y);
}

void SpriteGraphicsComponent::draw(Window* window) {
    window->draw(sprite);
}

const sf::Vector2f& SpriteGraphicsComponent::getSpriteScale() const {
    return sprite.getScale();
}

const sf::Vector2i& SpriteGraphicsComponent::getTextureSize() const {
    return textureSize;
}

void SpriteGraphicsComponent::setAnimation(const std::string&, bool, bool) {
    throw std::logic_error("SpriteGraphicsComponent does not support animations.");
}

bool SpriteGraphicsComponent::isCurrentAnimation(const std::string&) const {
    return false;
}

const Direction SpriteGraphicsComponent::getSpriteDirection() {
    return Direction::Right;
}

void SpriteGraphicsComponent::setSpriteDirection(Direction) {}

bool SpriteGraphicsComponent::isAnimationPlaying() const {
    return false;
}

bool SpriteGraphicsComponent::isAnimationInAction() const {
    return false;
}

SpriteSheet* SpriteGraphicsComponent::getSpriteSheet() {
    return nullptr;
}

// ------------------ SpriteSheetGraphicsComponent ------------------

void SpriteSheetGraphicsComponent::init(const std::string& file) {
    if (!spriteSheet.loadSheet(file)) {
        throw std::runtime_error("Failed to load sprite sheet: " + file);
    }
    spriteSheet.setAnimation("Idle", true, true);
}

void SpriteSheetGraphicsComponent::update(const Entity& entity, float elapsed) {
    spriteSheet.setSpritePosition(sf::Vector2f(entity.getPosition().x, entity.getPosition().y));
    spriteSheet.update(elapsed);
}

void SpriteSheetGraphicsComponent::draw(Window* window) {
    window->draw(spriteSheet.getSprite());
}

const sf::Vector2f& SpriteSheetGraphicsComponent::getSpriteScale() const {
    return spriteSheet.getSpriteScale();
}

const sf::Vector2i& SpriteSheetGraphicsComponent::getTextureSize() const {
    return spriteSheet.getSpriteSize();
}

void SpriteSheetGraphicsComponent::setAnimation(const std::string& animationName, bool play, bool loop) {
    spriteSheet.setAnimation(animationName, play, loop);
}

bool SpriteSheetGraphicsComponent::isCurrentAnimation(const std::string& animationName) const {
    return spriteSheet.getCurrentAnim()->getName() == animationName;
}

const Direction SpriteSheetGraphicsComponent::getSpriteDirection() {
    return spriteSheet.getSpriteDirection();
}

void SpriteSheetGraphicsComponent::setSpriteDirection(Direction d) {
    spriteSheet.setSpriteDirection(d);
}

bool SpriteSheetGraphicsComponent::isAnimationPlaying() const {
    return spriteSheet.getCurrentAnim()->isPlaying();
}

bool SpriteSheetGraphicsComponent::isAnimationInAction() const {
    return spriteSheet.getCurrentAnim()->isInAction();
}

SpriteSheet* SpriteSheetGraphicsComponent::getSpriteSheet() {
    return &spriteSheet;
}
