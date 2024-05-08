using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace task3
{
    class MyStealingThread : IMyThread
    {
        Dictionary<int, IDEQueue<IMyTask>> Queue;
        Random Random;
        public MyStealingThread(Dictionary<int, IDEQueue<IMyTask>> myQueue)
        {
            Queue = myQueue;
            Random = new Random();
        }
        public void Run(CancellationToken token)
        {
            int me = Thread.CurrentThread.ManagedThreadId;
            IMyTask task = Queue[me].PopBottom();
            while (!token.IsCancellationRequested)
            {
                while (task != null)
                {
                    task.Start();
                    task = Queue[me].PopBottom();
                }
                if (task == null)
                {
                    Thread.Yield();
                    int victim = Queue.Keys.ToList()[Random.Next(Queue.Keys.Count)];
                    if (!Queue[victim].IsEmpty())
                    {
                        task = Queue[victim].PopTop();
                    }
                    if (!Queue[me].IsEmpty())
                    {
                        task = Queue[me].PopTop();
                    }
                }
            }
        }
        }
    }
