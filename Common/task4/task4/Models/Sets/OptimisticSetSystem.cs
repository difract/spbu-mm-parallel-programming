using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace task4.Models
{
    public class OptimisticSetSystem<T> : IExamSystem<T>
    {
        private Node<T> Tail = new Node<T>(int.MaxValue);
        private Node<T> Head = new Node<T>(int.MinValue);

        public OptimisticSetSystem()
        {
            Head.Next = Tail;
        }

        private bool Validate(Node<T> pred, Node<T> curr)
        {
            Node<T> node = Head;
            while (node.Key <= pred.Key)
            {
                if (node == pred)
                    return pred.Next == curr;
                node = node.Next;
            }
            return false;
        }

        public bool Add(T item)
        {
            int key = item.GetHashCode();
            while (true)
            {
                Node<T> pred = Head;
                Node<T> curr = Head.Next;
                while (curr.Key < key)
                {
                    pred = curr;
                    curr = curr.Next;
                }
                pred.Lock.WaitOne();
                try
                {
                    curr.Lock.WaitOne();
                    try
                    {
                        if (Validate(pred, curr))
                        {
                            if (curr.Key == key)
                            {
                                return false;
                            }
                            else
                            {
                                Node<T> node = new Node<T>(item) { Next = curr };
                                pred.Next = node;
                                return true;
                            }
                        }
                    }

                    finally
                    {
                        pred.Lock.ReleaseMutex();
                    }
                }
                finally
                {
                    curr.Lock.ReleaseMutex();
                }

            }

        }

        public bool Remove(T item)
        {
            int key = item.GetHashCode();
            while (true)
            {
                Node<T> pred = Head;
                Node<T> curr = Head.Next;
                while (curr.Key < key)
                {
                    pred = curr;
                    curr = curr.Next;
                }
                pred.Lock.WaitOne();
                curr.Lock.WaitOne();
                try
                {
                    if (Validate(pred, curr))
                    {
                        if (curr.Key != key)
                        {
                            return false;
                        }
                        else
                        {
                            pred.Next = curr.Next;
                            return true;
                        }
                    }
                }
                finally
                {
                    curr.Lock.ReleaseMutex();
                    pred.Lock.ReleaseMutex();
                }
            }
        }

        public bool Contains(T item)
        {
            int key = item.GetHashCode();
            while (true)
            {
                Node<T> pred = Head;
                Node<T> curr = Head.Next;
                while (curr.Key < key)
                {
                    pred = curr;
                    curr = curr.Next;
                }
                pred.Lock.WaitOne();
                try
                {
                    curr.Lock.WaitOne();
                    try
                    {
                        if (Validate(pred, curr))
                        {
                            return curr.Key == key;
                        }
                    }
                    finally
                    {
                        curr.Lock.ReleaseMutex();
                    }
                }
                finally
                {
                    pred.Lock.ReleaseMutex();
                }
            }
        }

        public int Count()
        {
            int size = 0;
            Node<T> cur = Head;
            while (cur.Next != null)
            {
                size++;
                cur = cur.Next;
            }
            return size - 1;
        }
    }
}
