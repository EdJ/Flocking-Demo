namespace Flocking
{
    using System;

#if WINDOWS || XBOX
    public static class Program
    {
        static void Main(string[] args)
        {
            using (MainWrapper game = new MainWrapper())
            {
                game.Run();
            }
        }
    }
#endif
}

