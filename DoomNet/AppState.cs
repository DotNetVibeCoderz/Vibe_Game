using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomNet
{
    public class ExitArgs: EventArgs
    {
        public ExitArgs() { }
    }

    public class AppConstants
    {
        public static AppState State { get; private set; } = new();
    }
    public class AppState
    {
        public EventHandler<EventArgs> OnExit;

        public void Exit()
        {
            OnExit?.Invoke(this, new ExitArgs());
        }
    }
}
