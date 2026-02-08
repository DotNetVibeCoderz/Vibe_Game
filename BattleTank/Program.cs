using System;

namespace BattleTank
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new BattleTankGame())
                game.Run();
        }
    }
}