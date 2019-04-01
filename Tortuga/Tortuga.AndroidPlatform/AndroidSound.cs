using Android.Media;
using Tortuga.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tortuga.AndroidPlatform
{
    public class AndroidSound : ISound
    {
        private MediaPlayer _mediaPlayer;

        public AndroidSound(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
        }

        public void Play()
        {
            _mediaPlayer.Start();
        }
    }
}
