using System;

namespace task3
{
    public class MyTask<T> : IMyTask
    {
        private Func<T> Job;
        private ThreadPool MyPool;
        public bool IsCompleted;
        public T Result;

        public MyTask(Func<T> newJob)
        {
            IsCompleted = true;
            Result = default(T);
            Job = newJob;
        }

        public void SetPool(ThreadPool pool)
        {
            MyPool = pool;
        }

        public void Start()
        {
            IsCompleted = false;
            try
            {
                Result = Job();
                IsCompleted = true;
            }
            catch (Exception E)
            {
                IsCompleted = false;
                throw new AggregateException(E);
            }
        }

        public T GetResult()
        {
            return Result;
        }
        
        public MyTask<TNew> ContinueWith<TNew>(MyTask<T> oldTask, Func<T, TNew> func)
        {
            {
                MyTask<TNew> newTask = new MyTask<TNew>(() => func(oldTask.GetResult()));

                MyPool.Enqueue(newTask);
                /*
                Action doContinuation = () =>
                {
                    newTask.Func = () => func(Result);
                    MyPool.Enqueue(newTask);
                };
                Action doFail = () =>
                {
                    newTask.exceptions.AddRange(exceptions);
                    newTask.exceptions.Add(new ParentTaskFailException(this));
                    lock (newTask.Locker)
                    {
                        newTask.IsFailed = true;
                    }
                    newTask.OnFailed?.Invoke();
                };

                lock (Locker)
                {
                    if (IsCompleted)
                        doContinuation();
                    else if (IsFailed)
                        doFail();
                    else
                    {
                        OnCompleted += doContinuation;
                        OnFailed += doFail;
                    }
                }
                */
                return newTask;
            }
        }
    }
}
