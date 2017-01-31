using System;
using M101DotNet.Training;
using M101DotNet.Homework.Schema_Design;

namespace M101DotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            //var training = new Training.Training();
            //training.BulkWrite().GetAwaiter().GetResult();

            new DeleteHomeworkArray().Execute();
            Console.WriteLine();
            Console.WriteLine("Press Enter");
            Console.ReadLine();
        }
    }
}
