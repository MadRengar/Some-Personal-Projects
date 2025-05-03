#include "../../include/core/Tile.h"
#include <sstream>

Tile::Tile(TileType type, std::shared_ptr<TileFlyweight> flyweight)
	: type(type), flyweight(flyweight) {
	sprite.setTexture(flyweight->getTexture());
}

void Tile::loadTile(int x, int y, float sc) {
	position = { x, y };

	sprite.setScale(sc, sc);
	sf::Vector2u textSize = flyweight->getTexture().getSize();
	float pixels_x = static_cast<float>(x * (textSize.x * sc));
	float pixels_y = static_cast<float>(y * (textSize.y * sc));
	sprite.setPosition(pixels_x, pixels_y);
}

void Tile::draw(Window* window)
{
	window->draw(sprite);
}
