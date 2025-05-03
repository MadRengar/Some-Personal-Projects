#pragma once
#include "AudioService.h"
#include <SFML/Audio.hpp>
#include <map>

class SFMLAudioPlayer : public AudioPlayer {
public:
	void play(const std::string& soundName) override;

private:
	std::map<std::string, sf::SoundBuffer> soundBuffers;
	std::map<std::string, sf::Sound> sounds;
	void loadSound(const std::string& soundName);
};

