using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace task3
{
    class MySharingThread : IMyThread
    {
        Dictionary<int, IDEQueue<IMyTask>> Queue;
        Random Random;
        const int THRESHOLD = 42;
        public MySharingThread(Dictionary<int, IDEQueue<IMyTask>> myQueue)
        {
            Queue = myQueue;
            Random = new Random();
        }
        public void Run(CancellationToken token)
        {
            int me = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Hello from process " + me);
            while (!token.IsCancellationRequested)
            {
                IMyTask task;
                lock (Queue[me])
                {
                    task = Queue[me].PopTop();
                }
                if (task != null)
                    task.Start();
                int size = Queue[me].Count();
                if (Random.Next(size + 1) == size && size > 1)
                {
                    int victim = Queue.Keys.ToList()[Random.Next(Queue.Keys.Count)];
                    if (victim == me)
                        continue;
                    int min = (victim <= me) ? victim : me;
                    int max = (victim <= me) ? me : victim;
                    lock (Queue[min])
                    {
                        lock (Queue[max])
                        {
                            Console.WriteLine("Process " + min + " started balancing with " + max);
                            Console.WriteLine("Counts before " + Queue[min].Count() + " " + Queue[max].Count());
                            Balance(Queue[min], Queue[max]);
                        }
                    }
                }
                Thread.Sleep(30);
            }
        }
        private void Balance(IDEQueue<IMyTask> q0, IDEQueue<IMyTask> q1)
        {
            var qMin = (q0.Count() < q1.Count()) ? q0 : q1;
            var qMax = (q0.Count() < q1.Count()) ? q1 : q0;
            int diff = qMax.Count() - qMin.Count();
            if (diff > THRESHOLD)
                while (qMax.Count() > qMin.Count())
                    qMin.Enqueue(qMax.PopTop());
            Console.WriteLine("Counts after " + q0.Count() + " " + q1.Count());
        }
    }
}
