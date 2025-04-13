#pragma once
#include "../entities/Entity.h"
#include "../graphics/Window.h"
#include "../graphics/SpriteSheet.h"

class GraphicsComponent
{
public:
	~GraphicsComponent() {}

	virtual void init(const std::string& file) = 0;					
	virtual void update(const Entity& entity, float elapsed) = 0;	
	virtual void draw(Window* window) = 0;

	virtual void setAnimation(const std::string& animationName, bool play, bool loop) = 0;
	virtual bool isCurrentAnimation(const std::string& animationName) const = 0;
	virtual void setSpriteDirection(Direction d) = 0;
	virtual const Direction getSpriteDirection() = 0;
	virtual bool isAnimationPlaying() const = 0;
	virtual bool isAnimationInAction() const = 0;
	virtual SpriteSheet* getSpriteSheet() = 0;

	virtual const sf::Vector2f& getSpriteScale() const = 0;
	virtual const sf::Vector2i& getTextureSize() const = 0;
};

class SpriteGraphicsComponent : public GraphicsComponent
{
public:
	SpriteGraphicsComponent(float spriteScale) : scale(spriteScale) {}
	~SpriteGraphicsComponent() {}

	void init(const std::string& file) override;				//load textures, set texture and scale to sprite
	void update(const Entity& entity, float elapsed) override;	//sets sprite position
	void draw(Window* window) override;							//draws sprite

	const sf::Vector2f& getSpriteScale() const override;		//returns the scale of the sprite
	const sf::Vector2i& getTextureSize() const override;		//returns the size of the texture

	// Animation functions - do nothing in this case, but should throw an exception as these should never be called.
	void setAnimation(const std::string& animationName, bool play, bool loop) override;	
	bool isCurrentAnimation(const std::string& animationName) const override;
	const Direction getSpriteDirection() override;
	void setSpriteDirection(Direction d) override;
	bool isAnimationPlaying() const override;
	bool isAnimationInAction() const override;
	SpriteSheet* getSpriteSheet() override;

protected:
	// Variables that belong to sprite-only graphics
	sf::Texture texture;
	sf::Sprite sprite;
	sf::Vector2i textureSize;
	float scale;
};


class SpriteSheetGraphicsComponent : public GraphicsComponent
{
public:
	SpriteSheetGraphicsComponent() {}
	~SpriteSheetGraphicsComponent() {}

	void init(const std::string& file) override;				//load the sheet and set the starting animation (idle)
	void update(const Entity& entity, float elapsed) override;	//sets the position of the sprite (via spritesheet) and updates the spritesheet
	void draw(Window* window) override;							//draws the sprite (taken from spritesheet.getSprite()).

	const sf::Vector2f& getSpriteScale() const override;		//gets the scale of the sprite (via spritesheet)
	const sf::Vector2i& getTextureSize() const override;		//gets the size of the texture (spritesize via spritesheet)

	void setAnimation(const std::string& animationName, bool play, bool loop) override; //sets the animation by name in spritesheet
	bool isCurrentAnimation(const std::string& animationName) const override;	//returns true if animation in spritesheet mathcnes animationName
	const Direction getSpriteDirection() override;								//returns sprite direction in spritesheet
	virtual void setSpriteDirection(Direction d) override;						//sets sprite direction in spritesheet
	bool isAnimationPlaying() const override;									//returns true if the current animation is playing.
	bool isAnimationInAction() const override;									//returns true if the current animation is "in action".
	SpriteSheet* getSpriteSheet() override;										//returns the spritesheet

protected:
	//SpriteSheetGraphics only needs this data:
	SpriteSheet spriteSheet;
};