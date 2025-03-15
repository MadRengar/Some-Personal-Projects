#pragma once
#include <SFML/Graphics.hpp>
#include <string.h>

class Window
{
public:
	Window();

	Window(const std::string& title, const sf::Vector2u& size);

	~Window();

	void setup(const std::string& title, const sf::Vector2u& size);

	void beginDraw();

	void endDraw();

	void update();

	void toggleFullscreen();

	void draw(sf::Drawable& drawable);

	void redraw();

	/*Font*/
	sf::Text& getGUIText();
	void loadFont(const std::string& fontFile);
	void drawFps(sf::Drawable& drawable);

	// Flag-WindowClose
	bool isWindowDone() const { return isDone; }
	// Flag-FullWindow
	bool isWindowFullscreen() const { return isFullscreen; }
	// Set-WindowSize
	inline void setSize(const sf::Vector2u& size) { windowSize = size; }
	// Get-WindowSize
	const sf::Vector2u& getWindowSize() const { return windowSize; }
	// Set-WindowTitle
	inline void setTitle(const std::string& t) { windowTitle = t; }
	// Get-WindowTitle
	const std::string& getTitle() const { return windowTitle; }

private:
	void destroy();

	void create();

	sf::RenderWindow window;

	sf::Vector2u windowSize;

	std::string windowTitle;

	bool isDone;

	bool isFullscreen;

	/*Font attributes*/
	sf::Font font;
	sf::Text fpsText;
	const int FONT_SIZE = 30;
};

