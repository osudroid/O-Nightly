using System.Runtime.InteropServices;

namespace OsuDroidLib;

public interface IServiceModeRun {
    public void Stop();
    public void Dispose();
}

public class ServiceManager<T> : IDisposable, IServiceModeRun {
    private readonly List<Func<T, Result<T, string>>> _actions;
    private T? _state;
    private Task? _task;
    private TimeSpan _timeSpan;
    private bool FirstSleep = true;
    private Func<T>? FuncState;

    private ServiceManager() {
        _timeSpan = TimeSpan.FromDays(1);
        _actions = new List<Func<T, Result<T, string>>>(4);
    }

    public void Dispose() {
        _task?.Dispose();
    }

    public void Stop() {
        Dispose();
    }

    public static ServiceManager<T> DefaultStetting() {
        return new();
    }

    private async Task Loop() {
        static ResultErr<string> ForeActions(Span<Func<T, Result<T, string>>> actions, T? state) {
            if (state is null)
                throw new NullReferenceException(nameof(state));
            foreach (var action in actions) {
                var resp = action(state);
                if (resp == EResult.Err)
                    return resp;
                state = resp.Ok();
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

    public ServiceManager<T> SetFirstLoop(bool sleepFirst) {
        FirstSleep = sleepFirst;
        return this;
    }

    public ServiceManager<T> SetStateBuilder(Func<T> funcState) {
        FuncState = funcState;
        return this;
    }

    public ServiceManager<T> AddFunction(Func<T, Result<T, string>> action) {
        _actions.Add(action);
        return this;
    }

    public ServiceManager<T> ExecuteFunctionAfter(TimeSpan time) {
        _timeSpan = time;
        return this;
    }

    public ServiceManager<T> RunInNewTask() {
        RunInNewTask(out _);
        return this;
    }

    public async Task Run() {
        await Loop();
    }

    public ServiceManager<T> RunInNewTask(out Task task) {
        _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        task = _task;
        return this;
    }
}