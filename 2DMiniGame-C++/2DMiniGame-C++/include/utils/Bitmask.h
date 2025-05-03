//Adapted from SFML Game Development By Example (Raimondas Pupius)
#pragma once
#include <stdint.h>

using Bitset = uint32_t;

class Bitmask
{
public:
	Bitmask() : bits(0) {}
	Bitmask(const Bitset& b) : bits(b) {}

	Bitset getMask() const { return bits; }
	void setMask(const Bitset& value) { bits = value; }

	bool match(const Bitmask& b, const Bitset& relevant = 0) const
	{
		if (relevant)
			return (b.getMask() & relevant) == (bits & relevant);
		else (b.getMask() == bits);
	}

	bool contains(const Bitmask& other) const
	{
		return other.getMask() == (bits & other.getMask()); // Determine whether the entity "has at least" all the components required by the system.
		// (Eg: 0011 & 0010 = 0010) if has all of components required by the system than return true
	}

	bool getBit(const unsigned int& pos) const
	{
		return ((bits & (1 << pos)) != 0);
	}

	void turnOnBit(const unsigned int& pos)
	{
		bits |= 1 << pos;
	}

	void turnOnBits(const Bitset& b)
	{
		bits |= b;
	}

	void clearBit(const unsigned int& pos)
	{
		bits &= ~(1 << pos);
	}

	void toggleBit(const unsigned int& pos)
	{
		bits ^= 1 << pos;
	}

	void clear() { bits = 0; }

private:
	Bitset bits;
};