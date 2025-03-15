#include "Window.h"


Window::Window() 
	: windowTitle("Default Title"), 
	  windowSize(640, 480), 
	  isDone(false), 
	  isFullscreen(false)
{ 
	setup(windowTitle, windowSize);
	create();
}

Window::Window(const std::string& title, const sf::Vector2u& size) 
	: windowTitle(title),
	  windowSize(size),
	  isDone(false),
	  isFullscreen(false)
{
	setup(title, size);
	create();
}

Window::~Window() 
{
	destroy();
}

/*Set window basic attributes*/
void Window::setup(const std::string& title, const sf::Vector2u& size)
{
	windowTitle = title;
	windowSize = size;
	isFullscreen = false;
	isDone = false;
	create();
	// Load font 
	loadFont("AmaticSC-Regular.ttf");
}

void Window::create()
{
	auto style = isFullscreen ? sf::Style::Fullscreen : sf::Style::Default;

	sf::VideoMode videoMode{ windowSize.x, windowSize.y };

	window.create(videoMode, windowTitle, style);
}

void Window::destroy()
{
	if (window.isOpen()) {
		window.close();
	}
}

void Window::redraw()
{
	destroy();
	create();
}

void Window::toggleFullscreen()
{
	isFullscreen = !isFullscreen;
	redraw();

}

void Window::update()
{
	sf::Event event;
	while (window.pollEvent(event))
	{
		if (event.type == sf::Event::Closed)
		{
			isDone = true;

		}
		else if (event.type == sf::Event::KeyPressed && event.key.code == sf::Keyboard::F5)
		{
			toggleFullscreen();
		}

	}
}

void Window::beginDraw() 
{ 
	window.clear(sf::Color::Black);
}

void Window::draw(sf::Drawable& drawable) 
{
	window.draw(drawable);
}

void Window::drawFps(sf::Drawable& drawable)
{
	window.draw(fpsText);
}

void Window::endDraw() 
{
	window.display();
}

sf::Text& Window::getGUIText()
{
	return fpsText;
}

void Window::loadFont(const std::string& fontFile)
{
	if (!font.loadFromFile(fontFile)) {
		throw std::runtime_error("Failed to load font: " + fontFile);
	}
	/*Set text attributes*/
	fpsText.setFont(font);
	fpsText.setFillColor(sf::Color::White);
	fpsText.setCharacterSize(FONT_SIZE);
	fpsText.setPosition(10, 10);
	fpsText.setString("FPS: 0");
}




