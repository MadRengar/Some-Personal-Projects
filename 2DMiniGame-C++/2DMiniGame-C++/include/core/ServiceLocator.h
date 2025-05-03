#pragma once
#include "AudioService.h"

class ServiceLocator {
public:
	static AudioPlayer* GetAudio() { return audioService; }
	static void RegisterAudio(AudioPlayer* service) { audioService = service; }

private:
	static AudioPlayer* audioService;
};

