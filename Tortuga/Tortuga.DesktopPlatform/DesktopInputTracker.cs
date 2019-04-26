using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Platform;
using Veldrid;

namespace Tortuga.DesktopPlatform
{
    public class DesktopInputTracker : IInputTracker
    {
        private HashSet<TKey> _currentlyPressedKeys = new HashSet<TKey>();
        private HashSet<TKey> _newKeysThisFrame = new HashSet<TKey>();

        private HashSet<TMouseButton> _currentlyPressedMouseButtons = new HashSet<TMouseButton>();
        private HashSet<TMouseButton> _newMouseButtonsThisFrame = new HashSet<TMouseButton>();

        public Vector2 PointerPosition { get => MousePosition; }
        public Vector2 PointerDelta { get => MouseDelta; }
        public bool PointerDown { get => GetMouseButtonDown(TMouseButton.Left); }
        public bool PointerPressed { get => GetMouseButtonPressed(TMouseButton.Left); }
        public Vector2 MousePosition { get; private set; }
        public Vector2 MouseDelta { get; private set; }

        public bool GetKey(TKey key)
        {
            return _currentlyPressedKeys.Contains(key);
        }

        public bool GetKeyDown(TKey key)
        {
            return _newKeysThisFrame.Contains(key);
        }

        public bool GetMouseButtonDown(TMouseButton button)
        {
            return _currentlyPressedMouseButtons.Contains(button);
        }

        public bool GetMouseButtonPressed(TMouseButton button)
        {
            return _newMouseButtonsThisFrame.Contains(button);
        }

        public void UpdateFrameInput(InputSnapshot snapshot)
        {
            _newKeysThisFrame.Clear();
            _newMouseButtonsThisFrame.Clear();

            MouseDelta = snapshot.MousePosition - MousePosition;
            MousePosition = snapshot.MousePosition;

            for (int i = 0; i < snapshot.KeyEvents.Count; i++)
            {
                KeyEvent ke = snapshot.KeyEvents[i];
                if (ke.Down)
                {
                    KeyDown((TKey)ke.Key);
                }
                else
                {
                    KeyUp((TKey)ke.Key);
                }
            }
            for (int i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                MouseEvent me = snapshot.MouseEvents[i];
                if (me.Down)
                {
                    MouseDown((TMouseButton)me.MouseButton);
                }
                else
                {
                    MouseUp((TMouseButton)me.MouseButton);
                }
            }
        }

        private void MouseUp(TMouseButton mouseButton)
        {
            _currentlyPressedMouseButtons.Remove(mouseButton);
            _newMouseButtonsThisFrame.Remove(mouseButton);
        }

        private void MouseDown(TMouseButton mouseButton)
        {
            if (_currentlyPressedMouseButtons.Add(mouseButton))
            {
                _newMouseButtonsThisFrame.Add(mouseButton);
            }
        }

        private void KeyUp(TKey key)
        {
            _currentlyPressedKeys.Remove(key);
            _newKeysThisFrame.Remove(key);
        }

        private void KeyDown(TKey key)
        {
            if (_currentlyPressedKeys.Add(key))
            {
                _newKeysThisFrame.Add(key);
            }
        }
    }
}
