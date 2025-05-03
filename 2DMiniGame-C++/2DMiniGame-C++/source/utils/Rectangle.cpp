#include "../../include/utils/Rectangle.h"


bool Rectangle::inside(float x, float y) const
{
    // IX.A Implement this function, that returns true if the point <x,y> is inside this rectangle.
    //return false; // you can delete this once IX.A is complete.
    return(x >= topLeft.x && y >= topLeft.y &&
           x <= bottomRight.x && y <= bottomRight.y);
}

bool Rectangle::intersects(const Rectangle& rect) const
{
    // IX.B Implement this function, that returns true if the rectangle "rect" overlaps with this rectangle.
    //return false; // you can delete this once IX.B is complete.
    return !(bottomRight.x < rect.topLeft.x ||  // Completely on the left side
             topLeft.x > rect.bottomRight.x ||  // Completely on the right side
             bottomRight.y < rect.topLeft.y ||  // Completely on the top side
             topLeft.y > rect.bottomRight.y);   // Completely on the bottom side
}

