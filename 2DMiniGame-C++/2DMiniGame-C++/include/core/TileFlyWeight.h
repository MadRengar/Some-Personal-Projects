#pragma once
#include <SFML/Graphics.hpp>

class TileFlyweight {
public:
    TileFlyweight(const std::string& filename);

    const sf::Texture& getTexture() const;

private:
    sf::Texture texture;
};

