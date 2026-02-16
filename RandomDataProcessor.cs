using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class RandomDataProcessor(ILogger logger, int capacity = 100)
{
    private readonly BlockingCollection<DataItem> _queue = new(capacity);
    private readonly Random _random = new();
    private readonly ILogger _logger = logger;
    private readonly List<Task> _tasks = [];
    private readonly CancellationTokenSource _cts = new();

    public void Start(int generatorCount, int workerCount)
    {
        for (int i = 0; i < generatorCount; i++)
        {
            int id = i;
            _tasks.Add(Task.Run(() => GeneratorLoopAsync(id, _cts.Token)));
        }

        for (int i = 0; i < workerCount; i++)
        {
            int id = i;
            _tasks.Add(Task.Run(() => WorkerLoopAsync(id, _cts.Token)));
        }
    }

    public async Task Stop()
    {
        _cts.Cancel();
        _queue.CompleteAdding();

        try
        {
            await Task.WhenAll(_tasks);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task GeneratorLoopAsync(int generatorId, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int value;
            lock (_random)
            {
                value = _random.Next(0, 10000);
            }

            var item = new DataItem(value);
            _queue.Add(item, token);
        }
    }

    private async Task WorkerLoopAsync(int workerId, CancellationToken token)
    {
        try
        {
            foreach (var item in _queue.GetConsumingEnumerable(token))
            {
                if (item.Value == 1)
                {
                    _logger.LogInformation($"[WORKER {workerId}] Value found : {item.Value} ({item.CreatedAt:HH:mm:ss})");
                }
            }
            await Task.Delay(500, token);
        }
        catch (OperationCanceledException)
        {
        }
    }
}