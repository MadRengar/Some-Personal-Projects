#include <SFML/Graphics.hpp>
#include "Game.h"
#include <ctime>
#include <cstdlib>
#include <iostream>


// Check here APIs for the SFML classes to be used in this LAB (all in here: https://www.sfml-dev.org/documentation/2.5.1/annotated.php):
//   sf::RenderWindow -  https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1RenderWindow.php
//   sf::Vector2u - https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1Vector2.php
//   sf::Style - https://www.sfml-dev.org/documentation/1.6/namespacesf_1_1Style.php
//   sf::Drawable - https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1Drawable.php
//   sf::Texture - https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1Texture.php 
//   sf::Sprite - https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1Sprite.php


int simpleLoop(Game& game)
{
    sf::Time startTime = game.getElapsedTime();// Record the start time of the loop

    game.userInput();
    game.update(0);
    game.render();

    sf::Time endTime = game.getElapsedTime();// Record the end time of the loop

    // Calculate frame time (s)
    float elapsedSeconds = endTime.asSeconds() - startTime.asSeconds();
    int fps = (elapsedSeconds > 0) ? static_cast<int>(1.0f / elapsedSeconds) : 0;

    std::cout << "Frame Time: " << elapsedSeconds * 1000.0f << " ms, FPS: " << fps << std::endl;
    game.setFPS(fps);
    return fps;
}

int targetTimedLoop(Game& game, float targetElapsedTime)
{
    /*Calculate the time of one cycle*/
    sf::Clock frameClock;
    sf::Time startTime = frameClock.getElapsedTime();
    game.userInput();
    game.update(0);
    game.render();
    sf::Time endTime = frameClock.getElapsedTime();

    /*Calculate CPU sleep time*/
    float elapsedSeconds = endTime.asSeconds() - startTime.asSeconds();// Calculate frame time
    float sleepTime = targetElapsedTime - elapsedSeconds;// Calculating CPU requires sleep time
    if (sleepTime > 0) {
        sf::sleep(sf::seconds(sleepTime));
    }

    /*Calculate fps*/
    float totalFrameTime = elapsedSeconds + (sleepTime > 0 ? sleepTime : 0);
    int fps = (totalFrameTime > 0) ? static_cast<int>(1.0f / totalFrameTime) : 0;

    /*Output FPS information*/ 
    std::cout << "Frame Time: " << elapsedSeconds * 1000.0f << " ms, Sleep Time: " << sleepTime * 1000.0f
        << " ms, FPS: " << fps << std::endl;

    game.setFPS(fps); // Update window FPS text

    return fps;
}

int adaptiveLoop(Game& game, sf::Clock& globalClock, float& lastTime, float targetElapsedTime) {
    float currentTime = globalClock.getElapsedTime().asSeconds();
    float deltaTime = currentTime - lastTime; 

    game.userInput();
    game.update(deltaTime);
    game.render();

    float frameTime = globalClock.getElapsedTime().asSeconds() - currentTime;
    if (frameTime < targetElapsedTime) {
        sf::sleep(sf::seconds(targetElapsedTime - frameTime));
    }

    int fps = (deltaTime > 0) ? static_cast<int>(1.0f / deltaTime) : 0;
    game.setFPS(fps);

    lastTime = currentTime;

    return fps;
}

enum class GameLoopType {
    SimpleLoop,
    TargetTimedLoop,
    AdaptiveLoop
};

float runGameLoop(Game& game, GameLoopType loopType, float targetElapsedTime)
{
    sf::Clock globalClock;
    float lastTime = globalClock.getElapsedTime().asSeconds();
    int frameCount = 0;
    float totalElapsedTime = 0.0f;
    float averageFPS = 0.0f;
    float deltaTime = 0.0f;

    while (!game.getWindow()->isWindowDone())
    {
        int currentFPS = 0;

        sf::Time loopStartTime = globalClock.getElapsedTime();
        // Execute different logic based on Game Loop type
        switch (loopType) {
        case GameLoopType::SimpleLoop:
            currentFPS = simpleLoop(game);
            break;

        case GameLoopType::TargetTimedLoop:
            currentFPS = targetTimedLoop(game, targetElapsedTime);
            break;

        case GameLoopType::AdaptiveLoop:
            currentFPS = adaptiveLoop(game, globalClock, lastTime, targetElapsedTime);
            break;
        }
        sf::Time loopEndTime = globalClock.getElapsedTime();

        float deltaTime = loopEndTime.asSeconds() - loopStartTime.asSeconds();
        totalElapsedTime += deltaTime;

        // Choose the average FPS calculation method based on the Game Loop type
        if (loopType == GameLoopType::AdaptiveLoop) {
            averageFPS = frameCount / totalElapsedTime;
        }
        else {
            averageFPS = averageFPS + (currentFPS - averageFPS) / (frameCount + 1);
        }
        //std::cout << "Final Average FPS: " << static_cast<int>(averageFPS) << std::endl;
        frameCount++;
    }
    return averageFPS;
}

int main()
{
    srand(static_cast<unsigned int>(time(nullptr)));
    /*Basic game Info*/
    Game game(1, 1.0f);
    /*Choose loop type*/
    GameLoopType loopType = GameLoopType::AdaptiveLoop;//SimpleLoop TargetTimedLoop AdaptiveLoop
    float targetElapsedTime = 1.0f / 60.0f;
    float averageFPS = runGameLoop(game, loopType, targetElapsedTime);
    std::cout << "Final Average FPS: " << static_cast<int>(averageFPS) << std::endl;
    return 0;
}
