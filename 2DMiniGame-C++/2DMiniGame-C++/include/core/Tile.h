#pragma once
#include "../utils/Vector2.h"
#include "../graphics/Window.h"
#include "../core/TileFlyWeight.h"

enum class TileType {
	CORRIDOR,
	WALL
};

class Tile
{

private:

	TileType type;
	sf::Vector2i position; // Position in the grid (not in screen pixels, for that sprite.getPosition())
	sf::Sprite sprite;
	std::shared_ptr<TileFlyweight> flyweight;


public:

	Tile(TileType type, std::shared_ptr<TileFlyweight> flyweight);
	void loadTile(int x, int yy, float sc);

	//void setPosition(int x, int y);
	inline int x() const { return position.x; }
	inline int y() const { return position.y; }
	inline const sf::Vector2f& getScale() const { return sprite.getScale(); }
	inline TileType getType() const { return type; }
	inline float getSpriteXpos() const { return sprite.getPosition().x; }
	inline float getSpriteYpos() const { return sprite.getPosition().y; }
	void draw(Window* window);
};
