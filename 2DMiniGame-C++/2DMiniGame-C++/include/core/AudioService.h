#pragma once
#include <string>

class AudioPlayer {
public:
	virtual ~AudioPlayer() = default;
	virtual void play(const std::string& soundName) = 0;
};

