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
        private const int manufacturerCount = 3;
        private const int consumerCount = 3;
        private const int pause = 500;
        public void ManufacturerThreadFunc(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                mutexObj.WaitOne();

                buffer.Add(randGen.Next(0, 10000));

                mutexObj.ReleaseMutex();

                Console.WriteLine("Thread " + Environment.CurrentManagedThreadId + " put an item into the buffer, now it has " + buffer.Count + " elements");

                Thread.Sleep(pause);
            }
        }

        public void ConsumerThreadFunc(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
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
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            List<Thread> manufacturers = new List<Thread>();
            List<Thread> consumers = new List<Thread>();

            for (int i = 0; i < manufacturerCount; i++)
            {
                manufacturers.Add(new Thread(() => program.ManufacturerThreadFunc(tokenSource.Token)));
            }
            for (int i = 0; i < consumerCount; i++)
            {
                consumers.Add(new Thread(() => program.ConsumerThreadFunc(tokenSource.Token)));
            }

            Console.WriteLine("Hello, this program will now start " + manufacturerCount + " manufacturer and " + consumerCount + " consumer threads.");
            Console.WriteLine("They will operate unified buffer using mutex object. Press any key to start, then any key to stop.");
            Console.ReadKey();

            manufacturers.AsParallel().ForAll(elem => elem.Start());
            consumers.AsParallel().ForAll(elem => elem.Start());

            Console.ReadKey();

            tokenSource.Cancel();
            manufacturers.AsParallel().ForAll(elem => elem.Join());
            consumers.AsParallel().ForAll(elem => elem.Join());
            tokenSource.Dispose();
        }
    }
}
