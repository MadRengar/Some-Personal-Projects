#include <SFML/Graphics.hpp>
#include <iostream>
#include <fstream>
#include "include/core/Game.h"

void adaptiveLoop(Game& game, float& lastTime, float updateTarget = 0)
{
    float current = game.getElapsed().asSeconds();
    float elapsedSeconds = current - lastTime;
    //三个核心函数在每次循环中被调用：handleInput, update and render.
    game.handleInput();
    game.update(elapsedSeconds);
    game.render(elapsedSeconds);

    //计算相邻两帧之间的时间差
    float frameTime = game.getElapsed().asSeconds() - current;
    if (frameTime < updateTarget) {
        sf::sleep(sf::seconds(updateTarget - frameTime));// 通过sleep来控制两帧之间的间隔时间->实现锁帧
    }

    int currentFPS = (elapsedSeconds > 0) ? static_cast<int>(1.0f / elapsedSeconds) : 0;
    game.setFPS(currentFPS);
    lastTime = current; // 更新上一帧的时间
}

int main(int argc, char** argv[])
{
    // Try to load the level:
    std::ifstream levelRead{ "levels/lvl0.txt" };
    if (!levelRead)
    {
        throw std::exception("File not found\n");
    }

    // Convert the read file into a vector of strings, one per line:
    std::vector<std::string> lines;
    while (levelRead)
    {
        std::string strInput;
        std::getline(levelRead, strInput);
        lines.emplace_back(strInput);
    }


    // Create and initalize the game.
    Game game;
    game.init(lines);
    // GAME LOOP (with an update target time at 60FPS)
    float updateTarget = 0.016f; //FPS: 60
    float lastTime = game.getElapsed().asSeconds();

    while (!game.getWindow()->isWindowDone())
    {
        //Adaptive loop to elapsed time, with a target update.
        adaptiveLoop(game, lastTime, updateTarget);
    }

    return 0;
}