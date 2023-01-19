// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace OsuDroid.Lib;

public class TimeoutTokenDictionary<Token, Value> where Token : notnull {
    private static readonly Mutex Mut = new();

    private Dictionary<Token, Holder> _dictionary;
    private int Counter;
    private readonly int CounterClearAt;
    private readonly TimeSpan LifeTime;

    public TimeoutTokenDictionary(TimeSpan lifeTime, int counterClearAt = 1000, int capacity = 0) {
        if (capacity < 0)
            throw new Exception("capacity < 0");
        if (counterClearAt <= 0)
            throw new Exception("counterClearAt <= 0");

        _dictionary = new Dictionary<Token, Holder>(capacity == 0 ? 8 : capacity);
        Counter = 0;
        LifeTime = lifeTime;
        CounterClearAt = counterClearAt;
    }

    public void CleanDeadTokens() {
        CleanDeadTokens(false);
    }

    public void Add(Token token, Value value) {
        _dictionary[token] = new Holder(DateTime.UtcNow, value);
    }

    public Response<Value> Pop(Token token) {
        Counter++;
        if (!_dictionary.TryGetValue(token, out var holder))
            return Response<Value>.Err;

        var now = DateTime.UtcNow;
        if (holder.HolderIsDead(ref now, LifeTime))
            return Response<Value>.Err;

        _dictionary[token] = new Holder(DateTime.MinValue, default!);
        return Response<Value>.Ok(holder.Value);
    }

    private void CleanDeadTokens(bool intern = true) {
        if (intern && _dictionary.Count < Counter)
            return;

        try {
            Mut.WaitOne();

            var newDic = new Dictionary<Token, Holder>(CounterClearAt);
            var now = DateTime.UtcNow;
            foreach (var (key, value) in _dictionary) {
                if (value.HolderIsDead(ref now, LifeTime))
                    continue;
                newDic[key] = value;
            }

            _dictionary = newDic;
        }
#if DEBUG
        catch (Exception e) {
            WriteLine(e);
            throw;
        }
#else
        catch (Exception) {
        }
#endif
        finally {
            Mut.ReleaseMutex();
        }
    }

    private readonly struct Holder {
        public readonly DateTime CreatTime;
        public readonly Value Value;

        public Holder(DateTime creatTime, Value value) {
            CreatTime = creatTime;
            Value = value;
        }

        public bool HolderIsDead(ref DateTime dateTime, TimeSpan lifeTime) {
            return CreatTime + lifeTime < dateTime;
        }
    }
}