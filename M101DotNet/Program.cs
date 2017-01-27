using System;
using M101DotNet.Training;

namespace M101DotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            var training = new Training.Training();
            training.BulkWrite().GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("Press Enter");
            Console.ReadLine();
        }
    }
}
