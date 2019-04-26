using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Tortuga.Platform;

namespace Tortuga.AndroidPlatform
{
    public class AndroidInputTracker : IInputTracker
    {
        private Vector2 _previousPointerPosition;
        public Vector2 PointerPosition { get; private set; }
        public Vector2 PointerDelta { get => PointerPosition - _previousPointerPosition; }
        public bool PointerDown { get; private set; }
        public bool PointerPressed { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public Vector2 MouseDelta { get; private set; }

        private int? _currentPointerId;

        public bool ProcessMotionEvent(MotionEvent e)
        {
            PointerPressed = false;

            if (e.Action == MotionEventActions.Down && !_currentPointerId.HasValue)
            {
                _currentPointerId = e.GetPointerId(0);
                PointerPosition = new Vector2(e.GetX(0), e.GetY(0));
                _previousPointerPosition = PointerPosition;
                PointerDown = true;
                PointerPressed = true;
            }
            else if(e.Action == MotionEventActions.Up && _currentPointerId.HasValue)
            {
                _currentPointerId = null;
                PointerPosition = Vector2.Zero;
                _previousPointerPosition = Vector2.Zero;
                PointerDown = false;
            }
            else if(e.Action == MotionEventActions.Move && _currentPointerId.HasValue)
            {
                var index = e.FindPointerIndex(_currentPointerId.Value);
                _previousPointerPosition = PointerPosition;
                PointerPosition = new Vector2(e.GetX(index), e.GetY(index));
            }

            return true;
        }

        public bool GetKey(TKey key)
        {
            return false;
        }
        public bool GetKeyDown(TKey key)
        {
            return false;
        }
        public bool GetMouseButtonDown(TMouseButton button)
        {
            return false;
        }
        public bool GetMouseButtonPressed(TMouseButton button)
        {
            return false;
        }
    }
}