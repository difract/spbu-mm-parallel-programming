using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task4.Models
{
    public class LazySetSystem<T>: IExamSystem<T>
    {
        private Node<T> tail = new Node<T>(int.MaxValue);
        private Node<T> head = new Node<T>(int.MinValue);

        public LazySetSystem()
        {
            head.Next = tail;
        }

        private bool Validate(Node<T> pred, Node<T> curr)
        {
            return !pred.Marked && !curr.Marked && pred.Next == curr;
        }

        public bool Add(T item)
        {
            int key = item.GetHashCode();
            while (true)
            {
                Node<T> pred = head;
                Node<T> curr = head.Next;
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
                        curr.Lock.ReleaseMutex();
                    }
                }
                finally
                {
                    pred.Lock.ReleaseMutex();
                }

            }
        }

        public bool Remove(T item)
        {
            int key = item.GetHashCode();
            while (true)
            {
                Node<T> pred = head;
                Node<T> curr = head.Next;
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
                            if (curr.Key != key)
                            {
                                return false;
                            }
                            else
                            {
                                curr.Marked = true;
                                pred.Next = curr.Next;
                                return true;
                            }
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


        public bool Contains(T item)
        {
            int key = item.GetHashCode();
            Node<T> curr = head;
            while (curr.Key < key)
                curr = curr.Next;
            return curr.Key == key && !curr.Marked;
        }

        public int Count()
        {
            int size = 0;
            Node<T> cur = head;
            while (cur.Next != null)
            {
                size++;
                cur = cur.Next;
            }
            return size - 1;
        }
    }
}
