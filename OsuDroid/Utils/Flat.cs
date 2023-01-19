namespace OsuDroid.Utils;

public static class Flat {
    public static E? FlatToSingle<T, E>(IEnumerable<T> list, Func<T, E?, E> func) where E : class {
        E? res = null;
        foreach (var value in list) res = func(value, res);

        return res;
    }

    public static E? FlatToSingle<T, E>(Span<T> list, Func<T, E?, E> func) where E : class {
        E? res = null;
        foreach (var value in list) res = func(value, res);

        return res;
    }
}