using System;

namespace Physicalia
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (PhysicaliaGame game = new PhysicaliaGame())
            {
                game.Run();
            }
        }
    }
}

