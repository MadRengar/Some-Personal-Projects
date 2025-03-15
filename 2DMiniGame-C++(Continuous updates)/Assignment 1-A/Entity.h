#pragma once
#include<SFML/Graphics.hpp>
#include"Window.h"

class Entity
{
public:
	Entity(const std::string& texturePath, Window* window, float speed, float ttl);
	~Entity();

	void draw(Window& window);
	void update(float deltaTime);
	void moveMushroom(Window& window);
	const sf::Vector2f getPosition() const;
	float getCreateTime() const;
	float getTimeToLive() const;
private:
	/*
	Texture is responsible for storing texture data (images), 
	which is not directly rendered but provided to sf:: Sprite is used to draw objects on the window
	*/
	sf::Texture texture;
	/*
	* Sprite is responsible for displaying textures on the window and can control attributes such as position, scaling, and rotation
	*/
	sf::Sprite sprite;

	sf::Vector2f movementDirection;

	float movementSpeed;

	float timeToLive;
	float createTime;
};

