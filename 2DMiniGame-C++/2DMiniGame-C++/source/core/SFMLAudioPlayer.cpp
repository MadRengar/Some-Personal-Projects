#include "../../include/core/SFMLAudioPlayer.h"
#include <iostream>

void SFMLAudioPlayer::loadSound(const std::string& soundName) {
	sf::SoundBuffer buffer;
	if (buffer.loadFromFile("sounds/" + soundName)) {
		soundBuffers[soundName] = buffer;
		sf::Sound sound;
		sound.setBuffer(soundBuffers[soundName]);
		sounds[soundName] = sound;
	}
	else {
		std::cerr << "Failed to load sound: " << soundName << "\n";
	}
}

void SFMLAudioPlayer::play(const std::string& soundName) {
	if (sounds.find(soundName) == sounds.end()) {
		loadSound(soundName);
	}
	if (sounds.count(soundName)) {
		sounds[soundName].play();
	}
}
