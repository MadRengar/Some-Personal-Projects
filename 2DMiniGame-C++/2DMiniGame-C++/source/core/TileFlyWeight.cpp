#include "../../include/core/TileFlyweight.h"

TileFlyweight::TileFlyweight(const std::string& filename) {
    if (!texture.loadFromFile(filename)) {
        throw std::runtime_error("Failed to load texture: " + filename);
    }
}

const sf::Texture& TileFlyweight::getTexture() const {
    return texture;
}
