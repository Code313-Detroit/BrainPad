using Games.Light;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace LightSprintGame
{
    class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        static void Main()
        {
            
            var game = new LightSprint(160);
            game.StartUp();

            _quitEvent.WaitOne();
        }
    }
}
