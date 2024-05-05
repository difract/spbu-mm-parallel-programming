
namespace task3
{
    public interface IDEQueue<T>
    {
        void Enqueue(T value);

        T PopTop();
        T PopBottom();

        bool IsEmpty();

        int Count();
    }
}
