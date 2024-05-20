using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task4.Models
{
    public interface IExamSystem<T>
    {
        bool Add(T item);
        bool Remove(T item);
        bool Contains(T item);
        int Count();
    }
}
