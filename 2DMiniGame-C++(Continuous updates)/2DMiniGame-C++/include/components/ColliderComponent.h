#pragma once
#include "../utils/Rectangle.h"
#include "../entities/Entity.h"
#include "../graphics/Window.h"

class BoxColliderComponent 
{
public:

	BoxColliderComponent() {}
	~BoxColliderComponent() {}

	void init(const Vector2f& sz);		//Assigns the size of the box.
	void update(const Entity& entity);	//sets the bounding box position to the current position of the entity.
	void draw(Window* window);			//Draws the bouinding box on the window

	bool interesects(BoxColliderComponent* cc);	//Checks the intersection between "this" and cc box collider components.

	void setBoundingBox(const Vector2f& topLeft, const Vector2f& bottomRight);	//Sets the bounding box (top left and bottom right positions).
	Rectangle& getBoundingBox() { return boundingBox; };

	const Vector2f& getSize() const { return size; }

private:
	//Variables needed for this collider component.
	Rectangle boundingBox;
	Vector2f size;

};
