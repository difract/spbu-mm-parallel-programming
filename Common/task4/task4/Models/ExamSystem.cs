using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task4.Models
{
    public class ExamSystem<T>: IExamSystem<T>
    {
        private IExamSystem<T> Data;

        public ExamSystem(SetType type)
        {
            Data = type switch
            {
                SetType.LazySetSystem => new LazySetSystem<T>(),
                SetType.CoarseSetSystem => new CoarseSetSystem<T>(),
                _ => new LazySetSystem<T>()
            };
        }

        public bool Add(T newData)
        {
            return Data.Add(newData);
        }

        public bool Remove(T newData)
        {
            return Data.Remove(newData);
        }

        public bool Contains(T newData)
        {
            return Data.Contains(newData);
        }

        public int Count()
        {
            return Data.Count();
        }
    }
}
