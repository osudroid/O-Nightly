namespace OsuDroidLib.Interface;

public interface IPrototype<out T> : ICloneable where T : notnull {
    object ICloneable.Clone() {
        return CloneType();
    }

    public T CloneType();
}