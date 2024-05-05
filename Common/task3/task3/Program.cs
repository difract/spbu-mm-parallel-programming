using System;
using System.Threading;

namespace task3
{
    class Program
    {
        static Random Random;
        static void Main(string[] args)
        {
            int numThreads = 4;
            int numTasks = 100;
            Random = new Random();
            Console.WriteLine("Thread pool is working with " + numThreads + " threads and they're doing " + numTasks + " tasks.");
            Console.WriteLine("Enter '1', if you want to use WorkSharing strategy, else WorkStealing will be used.");
            Console.WriteLine("Then press any key to stop.");
            bool mode = Console.ReadKey().KeyChar == '1';
            ThreadPool pool = new ThreadPool(numThreads, mode);
            for (int i = 0; i < numTasks; i++)
            {
                MyTask<int> newTask = new MyTask<int>(Task);
                pool.Enqueue(newTask);
            }
            Console.ReadKey();
            pool.Dispose();
            Console.WriteLine("\nEverything is done. Press any key to exit.");
            Console.ReadKey();
        }

        static public int Task()
        {
            int num = Random.Next(100);
            Thread.Sleep(num);
            Console.WriteLine(num + " is counted by thread " + Thread.CurrentThread.ManagedThreadId);
            return num;
        }
    }
}
