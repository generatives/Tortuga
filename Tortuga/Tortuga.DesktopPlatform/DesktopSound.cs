using Tortuga.Audio;
using SharpAudio;
using SharpAudio.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tortuga.DesktopPlatform
{
    public class DesktopSound : ISound
    {
        private SoundStream _soundStream;

        public  DesktopSound(Stream stream, AudioEngine engine)
        {
            _soundStream = new SoundStream(stream, engine);
        }

        public void Play()
        {
            _soundStream.Play();
        }
    }
}
