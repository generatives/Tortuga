using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Tortuga.Platform
{
    public interface IInputTracker
    {
        Vector2 PointerPosition { get; }
        Vector2 PointerDelta { get; }
        bool PointerDown { get; }

        bool PointerPressed { get; }
        Vector2 MousePosition { get; }
        Vector2 MouseDelta { get; }

        bool GetKey(TKey key);
        bool GetKeyDown(TKey key);
        bool GetMouseButtonDown(TMouseButton button);
        bool GetMouseButtonPressed(TMouseButton button);
    }
}
