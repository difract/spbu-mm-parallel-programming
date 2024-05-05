using System.Threading;

namespace task3
{
    public interface IMyThread
    {
        void Run(CancellationToken token);
    }
}
