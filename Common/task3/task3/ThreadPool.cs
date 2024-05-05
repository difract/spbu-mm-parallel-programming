using System;
using System.Collections.Generic;
using System.Threading;

namespace task3
{
    public class ThreadPool : IDisposable
    {
        private volatile int ThreadCount;
        private List<Thread> Pool;
        Dictionary<int, IDEQueue<IMyTask>> TaskQueues;
        CancellationTokenSource TokenSource;
        public ThreadPool(int numThreads, bool mode)
        {
            ThreadCount = numThreads;
            TokenSource = new CancellationTokenSource();
            TaskQueues = new Dictionary<int, IDEQueue<IMyTask>>();
            Pool = new List<Thread>();
            Start(mode);
        }

        private void Start(bool sharing)
        {
            for(int i = 0; i < ThreadCount; i++)
            {
                IMyThread thr;
                if (sharing)
                    thr = new MySharingThread(TaskQueues);
                else
                    thr = new MyStealingThread(TaskQueues);
                Thread poolMember = new Thread(() => thr.Run(TokenSource.Token));
                TaskQueues.Add(poolMember.ManagedThreadId, new BDEQueue());
                Pool.Add(poolMember);
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

        public void Stop()
        {
            TokenSource.Cancel();
            Thread.Sleep(100);
        }

        public void Dispose()
        {
            Stop();
            TokenSource.Dispose();
        }
    }
}
