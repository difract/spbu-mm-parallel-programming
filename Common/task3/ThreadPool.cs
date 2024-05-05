using System;
using System.Collections.Generic;
using System.Threading;

namespace task3
{
    public class ThreadPool : IDisposable
    {
        private volatile int ThreadCount;
        public List<Thread> Pool;
        public Dictionary<int, IDEQueue<IMyTask>> TaskQueues;
        CancellationTokenSource TokenSource;
        public ThreadPool(int numThreads, bool sharing)
        {
            ThreadCount = numThreads;
            TokenSource = new CancellationTokenSource();
            TaskQueues = new Dictionary<int, IDEQueue<IMyTask>>();
            Pool = new List<Thread>();
            for (int i = 0; i < ThreadCount; i++)
            {
                IMyThread thr;
                if (sharing)
                    thr = new MySharingThread(TaskQueues);
                else
                    thr = new MyStealingThread(TaskQueues);
                Thread poolMember = new Thread(() => thr.Run(TokenSource.Token));
                TaskQueues.Add(poolMember.ManagedThreadId, new BDEQueue());
                Pool.Add(poolMember);
            }
        }

        public void Start()
        {
            foreach(Thread poolMember in Pool)
            {
                poolMember.Start();
            }
        }

        public void Enqueue<T> (MyTask<T> task)
        {
            int minKey = 0;
            int minLen = int.MaxValue;
            foreach(int key in TaskQueues.Keys)
            {
                if(TaskQueues[key].Count() < minLen)
                {
                    minKey = key;
                    minLen = TaskQueues[key].Count();
                }
            }
            task.SetPool(this);
            lock (TaskQueues[minKey])
            {
                TaskQueues[minKey].Enqueue(task);
            }
        }

        private void Stop()
        {
            TokenSource.Cancel();
            foreach (Thread thr in Pool)
                if(thr.IsAlive)
                    thr.Join();
        }

        public void Dispose()
        {
            Stop();
            TokenSource.Dispose();
        }
    }
}
