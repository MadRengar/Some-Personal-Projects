#pragma once
enum class ComponentID
{
	UNDEFINE = -1,
	INPUT = 0,
	POSITION = 1,
	VELOCITY = 2,
	COLLIDER = 3,
	HEALTH = 4,
	GRAPHICS = 5,
	PLAYERSTATE = 6,
	TTL = 7
};

class Component
{
public:
	virtual ~Component() = default;
	virtual ComponentID getID() const = 0;//'= 0'Pure Virtual Function means this function must be overrride
};