using System;
using System.Threading.Tasks;

namespace CrossWordsNet.Services
{
    public static class SoundManager
    {
        public static void PlayClick()
        {
            Task.Run(() => 
            {
                try
                {
                    Console.Beep(1000, 100);
                }
                catch { }
            });
        }

        public static void PlaySuccess()
        {
            Task.Run(() => 
            {
                try
                {
                    Console.Beep(1000, 200);
                    Console.Beep(1500, 200);
                }
                catch { }
            });
        }

        public static void PlayFail()
        {
            Task.Run(() => 
            {
                try
                {
                    Console.Beep(500, 400);
                }
                catch { }
            });
        }
        
        public static void PlayWin()
        {
             Task.Run(() => 
            {
                try
                {
                    Console.Beep(1000, 200);
                    Console.Beep(1200, 200);
                    Console.Beep(1500, 200);
                    Console.Beep(2000, 400);
                }
                catch { }
            });
        }
    }
}