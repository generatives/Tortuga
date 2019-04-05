using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Drawing.Resources;
using Tortuga.Graphics.Resources;
using Veldrid;

namespace Tortuga.Graphics.Text
{
    public class Font
    {
        private GridSpriteSheet _spriteSheet;
        private Dictionary<char, int> _characterIndices;

        public int CharacterWidth { get; private set; }
        public int CharacterHeight { get; private set; }

        public SubTexture this[char c]
        {
            get
            {
                return _spriteSheet[_characterIndices[c]];
            }
        }

        public Font(Texture texture, int characterWidth, int characterHeight, string characters)
        {
            CharacterWidth = characterWidth;
            CharacterHeight = characterHeight;
            _spriteSheet = new GridSpriteSheet(texture, CharacterWidth, CharacterHeight);
            _characterIndices = new Dictionary<char, int>();
            for(int i = 0; i < characters.Length; i++)
            {
                _characterIndices[characters[i]] = i;
            }
        }
    }
}
