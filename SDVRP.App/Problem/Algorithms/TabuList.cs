using System.Collections;
using CSharpFunctionalExtensions;

namespace SDVRP.App.Problem.Algorithms;

internal class TabuList<T> : IEnumerable<T>
{
    private readonly int _tabuListSize;

    private readonly Queue<T> _queue;

    public TabuList(int tabuListSize)
    {
        if (tabuListSize < 1) throw new ArgumentOutOfRangeException(nameof(tabuListSize), "Tabu list must have positive size.");
        
        _tabuListSize = tabuListSize;
        _queue = new Queue<T>(_tabuListSize);
    }

    public Result<T> Add(T item)
    {
        _queue.Enqueue(item);
        if (_queue.Count > _tabuListSize)
        {
            _queue.Dequeue();
        }

        return item;
    }

    public Result Clear()
    {
        _queue.Clear();
        return Result.Success();
    }

    public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_queue).GetEnumerator();
}
