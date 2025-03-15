#pragma once
class TTLComponent
{
private:
	int ttl;
public:
	explicit TTLComponent(int ttlValue) : ttl(ttlValue) {};
	void update()
	{
		if (ttl > 0)
		{
			ttl -= 1;
		}	
	}
	int getTTL () const
	{
		return ttl;
	}
};