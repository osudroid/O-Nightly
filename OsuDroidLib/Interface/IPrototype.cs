#nullable enable

namespace OsuDroidLib.Interface; 

public interface IPrototype<out T>: ICloneable where T : notnull {
    public T CloneType();
    
    object ICloneable.Clone() {
        return CloneType();
    }
}