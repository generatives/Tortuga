using System;
using System.Collections.Generic;
using System.Text;

namespace FlyingTortuga.Game
{
    interface IScreen
    {
        void Started(Game game);
        void Stopped();
        void Tick(float time);
    }
}
