using System;
using System.Threading;

public class ThreadSafeResettableLazy<T>
{
    private readonly Func<T> _valueFactory;
    private readonly LazyThreadSafetyMode _mode;
    private Lazy<T> _lazy;
    private readonly object _lock = new object();

    public ThreadSafeResettableLazy(Func<T> valueFactory, LazyThreadSafetyMode mode = LazyThreadSafetyMode.ExecutionAndPublication)
    {
        _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        _mode = mode;
        _lazy = new Lazy<T>(_valueFactory, _mode);
    }

    public T Value
    {
        get
        {
            // Thread-safe read operations happen inside the Lazy<T> instance itself
            return _lazy.Value;
        }
    }

    public bool IsValueCreated => _lazy.IsValueCreated;

    public void Reset()
    {
        lock (_lock)
        {
            _lazy = new Lazy<T>(_valueFactory, _mode);
        }
    }
}
