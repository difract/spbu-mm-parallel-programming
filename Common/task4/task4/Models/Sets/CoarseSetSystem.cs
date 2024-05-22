using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace task4.Models
{
    public class CoarseSetSystem<T> : IExamSystem<T>
    {
        private Node<T> head;
        private object sync = new object();
        public CoarseSetSystem()
        {
            head = new Node<T>(int.MinValue);
            head.Next = new Node<T>(int.MaxValue);
        }

        public bool Add(T item)
        {
            Node<T> pred, curr;
            int key = item.GetHashCode();
            Monitor.Enter(sync);
            try
            {
                pred = head;
                curr = pred.Next;
                while (curr.Key < key)
                {
                    pred = curr;
                    curr = curr.Next;
                }
                if (key == curr.Key)
                {
                    return false;
                }
                else
                {
                    Node<T> node = new Node<T>(item);

                    node.Next = curr;
                    pred.Next = node;
                    return true;
                }
            }
            finally
            {
                Monitor.Exit(sync);
            }
        }

        public bool Remove(T item)
        {
            Node<T> pred, curr;
            int key = item.GetHashCode();
            Monitor.Enter(sync);
            try
            {
                pred = head;
                curr = pred.Next;
                while (curr.Key < key)
                {
                    pred = curr;
                    curr = curr.Next;
                }
                if (key == curr.Key)
                {
                    pred.Next = curr.Next;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                Monitor.Exit(sync);
            }
        }

        public bool Contains(T item)
        {
            Node<T> pred, curr;
            int key = item.GetHashCode();
            Monitor.Enter(sync);
            try
            {
                pred = head;
                curr = pred.Next;
                while (curr.Key < key)
                {
                    pred = curr;
                    curr = curr.Next;
                }
                return key == curr.Key;

            }
            finally
            {
                Monitor.Exit(sync);
            }
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
