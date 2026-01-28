using Avalonia.Input;
using System.Collections.Generic;

namespace LiteGame2D.Engine
{
    public static class Input
    {
        private static HashSet<Key> _pressedKeys = new HashSet<Key>();

        public static void OnKeyDown(Key key)
        {
            if (!_pressedKeys.Contains(key))
                _pressedKeys.Add(key);
        }

        public static void OnKeyUp(Key key)
        {
            if (_pressedKeys.Contains(key))
                _pressedKeys.Remove(key);
        }

        public static bool IsKeyDown(Key key)
        {
            return _pressedKeys.Contains(key);
        }
        
        public static void Clear()
        {
            _pressedKeys.Clear();
        }
    }
}