using System.Runtime.InteropServices;

namespace OsuDroid.Lib;

internal interface IServiceModeRun {
    public void Stop();
    public void Dispose();
}

internal sealed class ServiceModeRun<T> : IDisposable, IServiceModeRun {
    private readonly List<Func<T, ResultErr<string>>> _actions;
    private T? _state;
    private Task? _task;
    private TimeSpan _timeSpan;
    private bool FirstSleep = true;
    private Func<T>? FuncState;

    private ServiceModeRun() {
        _timeSpan = TimeSpan.FromDays(1);
        _actions = new List<Func<T, ResultErr<string>>>(4);
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
        static ResultErr<string> ForeActions(Span<Func<T, ResultErr<string>>> actions, T? state) {
            if (state is null)
                throw new NullReferenceException(nameof(state));
            foreach (var action in actions) {
                var resultErr = action(state);
                if (resultErr == EResult.Err)
                    return resultErr;
            }

            return ResultErr<string>.Ok();
        }

        try {
            if (FuncState is null)
                throw new NullReferenceException(nameof(FuncState));
            _state = FuncState();

            while (true) {
                var response = ResultErr<string>.Ok();
                if (FirstSleep) {
                    await Task.Delay(_timeSpan);
                    response = ForeActions(CollectionsMarshal.AsSpan(_actions), _state);
                    if (response == EResult.Err)
                        _state = FuncState();
                    continue;
                }

                response = ForeActions(CollectionsMarshal.AsSpan(_actions), _state);
                if (response == EResult.Err)
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

    public ServiceModeRun<T> AddFunction(Func<T, ResultErr<string>> action) {
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