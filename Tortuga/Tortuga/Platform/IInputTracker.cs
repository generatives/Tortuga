using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Tortuga.Platform
{
    public interface IInputTracker
    {
        /// <summary>
        /// The position of the pointer (The mouse on Desktop, single finger on mobile)
        /// </summary>
        Vector2 PointerPosition { get; }
        /// <summary>
        /// How far the pointer moved this frame (The mouse on Desktop, single finger on mobile)
        /// </summary>
        Vector2 PointerDelta { get; }
        /// <summary>
        /// Was the pointer pressed this frame (Left mouse button on Desktop, single finger down on mobile)
        /// </summary>
        bool PointerDown { get; }
        /// <summary>
        /// Is the pointer pressed (Left mouse button on Desktop, single finger down on mobile)
        /// </summary>
        bool PointerPressed { get; }
        /// <summary>
        /// The mouse's current position
        /// </summary>
        Vector2 MousePosition { get; }
        /// <summary>
        /// How far the mouse moved this frme
        /// </summary>
        Vector2 MouseDelta { get; }
        /// <summary>
        /// Check if the key is pressed down
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool GetKey(TKey key);
        /// <summary>
        /// Check if the key was pressed this frame
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool GetKeyDown(TKey key);
        /// <summary>
        /// Check if the mouse button was pressed this frame
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool GetMouseButtonDown(TMouseButton button);
        /// <summary>
        /// Check if the mouse button is pressed down
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool GetMouseButtonPressed(TMouseButton button);
    }
}
