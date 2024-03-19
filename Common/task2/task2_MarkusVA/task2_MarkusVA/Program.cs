using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace task2_MarkusVA
{
    class Program
    {
        private List<int> buffer = new List<int>();
        private Mutex mutexObj = new Mutex();
        private Random randGen = new Random();
        private bool activityFlag = true;
        private const int manufacturerCount = 3;
        private const int consumerCount = 3;
        private const int pause = 500;
        public void manufacturerThreadFunc()
        {
            while (activityFlag)
            {
                mutexObj.WaitOne();

                buffer.Add(randGen.Next(0, 10000));

                mutexObj.ReleaseMutex();

                Console.WriteLine("Thread " + Environment.CurrentManagedThreadId + " put an item into the buffer, now it has " + buffer.Count + " elements");

                Thread.Sleep(pause);
            }
        }

        public void consumerThreadFunc()
        {
            while (activityFlag)
            {
                mutexObj.WaitOne();

                if (buffer.Count > 0)
                    buffer.RemoveAt(buffer.Count - 1);

                mutexObj.ReleaseMutex();

                Console.WriteLine("Thread " + Environment.CurrentManagedThreadId + " took an item from the buffer, now it has " + buffer.Count + " elements");

                Thread.Sleep(pause);
            }
        }
        static void Main(string[] args)
        {
            var program = new Program();
            List<Thread> manufacturers = new List<Thread>();
            List<Thread> consumers = new List<Thread>();

            for (int i = 0; i < manufacturerCount; i++)
            {
                manufacturers.Add(new Thread(program.manufacturerThreadFunc));
            }
            for (int i = 0; i < consumerCount; i++)
            {
                consumers.Add(new Thread(program.consumerThreadFunc));
            }

            Console.WriteLine("Hello, this program will now start " + manufacturerCount + " manufacturer and " + consumerCount + " consumer threads.");
            Console.WriteLine("They will operate unified buffer using mutex object. Press any key to start, then any key to stop.");
            Console.ReadKey();

            manufacturers.AsParallel().ForAll(elem => elem.Start());
            consumers.AsParallel().ForAll(elem => elem.Start());

            Console.ReadKey();

            program.activityFlag = false;
            manufacturers.AsParallel().ForAll(elem => elem.Join());
            consumers.AsParallel().ForAll(elem => elem.Join());
        }
    }
}
