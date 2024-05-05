using System.Collections.Generic;
using System.Linq;

namespace task3
{
    public class BDEQueue : IDEQueue<IMyTask>
    {
        private List<IMyTask> Queue;
        public object Locker;
        public BDEQueue()
        {
            Queue = new List<IMyTask>();
            Locker = new object();
        }

        public int Count()
        {
            lock (Locker)
            {
                return Queue.Count;
            }
        }

        public bool IsEmpty()
        {
            lock (Locker)
            {
                return Queue.Count == 0;
            }
        }

        public void Enqueue(IMyTask value)
        {
            lock (Locker)
            {
                Queue.Insert(0, value);
            }
        }

        public IMyTask PopBottom()
        {
            lock (Locker)
            {
                if (Queue.Count < 1 || Queue.Last() == null)
                    return null;
                IMyTask task = Queue.Last();
                Queue.RemoveAt(Queue.Count - 1);
                return task;
            }
        }

        public IMyTask PopTop()
        {
            lock (Locker)
            {
                if (Queue.Count < 1 || Queue.First() == null)
                    return null;
                IMyTask task = Queue.First();
                Queue.RemoveAt(0);
                return task;
            }
        }
    }
}
