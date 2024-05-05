using System;
using System.Threading;
using System.Collections.Generic;

namespace task3
{
    class Program
    {
        static Random Random;
        private int numThreads = 4;
        static void Main(string[] args)
        {
            Program prog = new Program();
            Random = new Random();
            Console.WriteLine("This program is implementing ThreadPool and putting it through some tests");
            Console.WriteLine("Thread pool is working with " + prog.numThreads + " threads.");
            bool mode = true;
            Console.WriteLine("Single task adding with WorkSharing test successful: " + prog.TestSingleTask(mode));
            Console.WriteLine("Multiple task adding with WorkSharing test successful: " + prog.TestMultipleTasks(mode));
            mode = false;
            Console.WriteLine("Single task adding with WorkStealing test successful: " + prog.TestSingleTask(mode));
            Console.WriteLine("Multiple task adding with WorkStealing test successful: " + prog.TestMultipleTasks(mode));
            Console.WriteLine("Thread amount test successful: " + prog.TestAmountOfThreads(mode));
            Console.WriteLine("Separate ContinueWith test successful: " + prog.TestSeparateContinuation(mode));
            Console.WriteLine("Pipeline ContinueWith test successful: " + prog.TestPipelineContinuation(mode));
            Console.WriteLine("Everything is done. Press any key to exit.");
            Console.ReadKey();
        }

        static public int Task()
        {
            int num = Random.Next(100);
            Thread.Sleep(num);
            return num;
        }

        public bool TestSingleTask(bool mode)
        {
            ThreadPool pool = new ThreadPool(numThreads, mode);
            int numTasks = 1;
            for (int i = 0; i < numTasks; i++)
            {
                MyTask<int> newTask = new MyTask<int>(Task);
                pool.Enqueue(newTask);
            }
            int numAcquired = 0;
            foreach (IDEQueue <IMyTask> TaskQueue in pool.TaskQueues.Values)
                numAcquired += TaskQueue.Count();
            pool.Dispose();
            return numAcquired == numTasks;
        }

        public bool TestMultipleTasks(bool mode)
        {
            ThreadPool pool = new ThreadPool(numThreads, mode);
            int numTasks = 250;
            for (int i = 0; i < numTasks; i++)
            {
                MyTask<int> newTask = new MyTask<int>(Task);
                pool.Enqueue(newTask);
            }
            int numAcquired = 0;
            foreach (IDEQueue<IMyTask> TaskQueue in pool.TaskQueues.Values)
                numAcquired += TaskQueue.Count();
            pool.Dispose();
            return numAcquired == numTasks;
        }

        public bool TestAmountOfThreads(bool mode)
        {
            ThreadPool pool = new ThreadPool(numThreads, mode);
            int numAcquired = 0;
            pool.Start();
            foreach (Thread trh in pool.Pool)
                numAcquired++;
            pool.Dispose();
            return numAcquired == numThreads;
        }

        public bool TestSeparateContinuation(bool mode)
        {
            ThreadPool pool = new ThreadPool(numThreads, mode);
            int initialNumber = 1;
            pool.Start();
            MyTask<int> task1 = new MyTask<int>(() => initialNumber);
            pool.Enqueue(task1);
            MyTask<int> task2 = task1.ContinueWith(x => x * 2);
            Thread.Sleep(100);
            pool.Dispose();
            return initialNumber * 2 == task2.GetResult();
        }
        public bool TestPipelineContinuation(bool mode)
        {
            ThreadPool pool = new ThreadPool(numThreads, mode);
            int initialNumber = 1;
            pool.Start();
            MyTask<int> task1 = new MyTask<int>(() => initialNumber);
            pool.Enqueue(task1);
            MyTask<int> task2 = task1.ContinueWith(x => x * 2).ContinueWith(x => x + 3);
            Thread.Sleep(100);
            pool.Dispose();
            return (initialNumber * 2) + 3 == task2.GetResult();
        }
    }
}
