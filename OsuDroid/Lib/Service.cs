using System.Runtime.InteropServices;

namespace OsuDroid.Lib;

internal interface IServiceModeRun {
    public void Stop();
    public void Dispose();
}

internal sealed class ServiceModeRun<T> : IDisposable, IServiceModeRun {
    private readonly List<Func<T, Response>> _actions;
    private T? _state;
    private Task? _task;
    private TimeSpan _timeSpan;
    private bool FirstSleep = true;
    private Func<T>? FuncState;

    private ServiceModeRun() {
        _timeSpan = TimeSpan.FromDays(1);
        _actions = new List<Func<T, Response>>(4);
    }

    public void Dispose() {
        _task?.Dispose();
    }

    public void Stop() {
        Dispose();
    }

    public static ServiceModeRun<T> DefaultStetting() {
        return new();
    }

    private async Task Loop() {
        static Response ForeActions(Span<Func<T, Response>> actions, T? state) {
            if (state is null)
                throw new NullReferenceException(nameof(state));
            foreach (var action in actions)
                if (action(state) == EResponse.Err)
                    return Response.Err();
            return Response.Ok();
        }

        try {
            if (FuncState is null)
                throw new NullReferenceException(nameof(FuncState));
            _state = FuncState();

            while (true) {
                var response = Response.Empty;
                if (FirstSleep) {
                    await Task.Delay(_timeSpan);
                    response = ForeActions(CollectionsMarshal.AsSpan(_actions), _state);
                    if (response == EResponse.Err)
                        _state = FuncState();
                    continue;
                }

                response = ForeActions(CollectionsMarshal.AsSpan(_actions), _state);
                if (response == EResponse.Err)
                    _state = FuncState();
                await Task.Delay(_timeSpan);
            }
        }
        catch (Exception e) {
            WriteLine(e);
            throw;
        }
    }

    public ServiceModeRun<T> SetFirstLoop(bool sleepFirst) {
        FirstSleep = sleepFirst;
        return this;
    }

    public ServiceModeRun<T> SetStateBuilder(Func<T> funcState) {
        FuncState = funcState;
        return this;
    }

    public ServiceModeRun<T> AddFunction(Func<T, Response> action) {
        _actions.Add(action);
        return this;
    }

    public ServiceModeRun<T> ExecuteFunctionAfter(TimeSpan time) {
        _timeSpan = time;
        return this;
    }

    public ServiceModeRun<T> RunInNewTask() {
        RunInNewTask(out _);
        return this;
    }

    private async Task Run() {
        await Loop();
    }

    public ServiceModeRun<T> RunInNewTask(out Task task) {
        _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        task = _task;
        return this;
    }
}