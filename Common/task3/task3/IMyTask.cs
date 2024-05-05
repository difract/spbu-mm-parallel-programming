using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task3
{
    public interface IMyTask
    {
        void SetPool(ThreadPool pool);

        void Start();
    }
}
