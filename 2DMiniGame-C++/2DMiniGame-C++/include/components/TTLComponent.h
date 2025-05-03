#pragma once
#include "Components.h"

class TTLComponent : public Component
{
private:
	int ttl;
	int originalTTL;
public:
	explicit TTLComponent(int ttlValue) : ttl(ttlValue) { originalTTL = ttlValue; };

	int getTTL () const
	{
		return ttl;
	}

	virtual ComponentID getID() const override { return ComponentID::TTL; }

	void decreaseTTL() { ttl--; }

	void resetTTL() { ttl = originalTTL; }
};