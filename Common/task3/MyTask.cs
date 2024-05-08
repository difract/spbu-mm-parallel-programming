using System;
using System.Threading;

namespace task3
{
    public class MyTask<T> : IMyTask
    {
        private Func<T> Job;
        private ThreadPool MyPool;
        public AutoResetEvent IsCompleted;
        private T Result;

        public MyTask(Func<T> newJob)
        {
            IsCompleted = new AutoResetEvent(false);
            Result = default(T);
            Job = newJob;
        }

        public void SetPool(ThreadPool pool)
        {
            MyPool = pool;
        }

        public void Start()
        {
            try
            {
                Result = Job();
                IsCompleted.Set();
            }
            catch (Exception E)
            {
                throw new AggregateException(E);
            }
        }

        public T GetResult()
        {
            lock (Result)
            {
                return Result;
            }
        }
        
        public MyTask<TNew> ContinueWith<TNew>(Func<T, TNew> func)
        {
            {
                IsCompleted.WaitOne();
                MyTask<TNew> newTask = new MyTask<TNew>(() => func(GetResult()));
                MyPool?.Enqueue(newTask);
                return newTask;
            }
        }
    }
}
