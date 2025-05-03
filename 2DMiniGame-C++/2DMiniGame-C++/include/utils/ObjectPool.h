#pragma once
#include <vector>
#include <memory>

template<class T>
class ObjectPool
{
public:
	ObjectPool(size_t size);

	std::shared_ptr<T> acquire();
	void release(std::shared_ptr<T> obj);
	const std::vector<std::shared_ptr<T>>& getAll() const { return objects; }
private:
	std::vector<std::shared_ptr<T>> objects;
};

template<class T>
ObjectPool<T>::ObjectPool(size_t size)
{
	for (size_t i = 0; i < size; ++i)
	{
		auto obj = std::make_shared<T>();
		obj->setActive(false);
		objects.push_back(obj);
	}
}

template<class T>
std::shared_ptr<T> ObjectPool<T>::acquire()
{
	for (auto& obj : objects) {
		if (!obj->isActive()) {
			obj->setActive(true);
			return obj;
		}
	}
	return nullptr;
}

template<class T>
void ObjectPool<T>::release(std::shared_ptr<T> obj)
{
	obj->setActive(false);
}