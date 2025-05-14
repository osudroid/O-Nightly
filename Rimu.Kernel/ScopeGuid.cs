namespace Rimu.Kernel;

public sealed class ScopeGuid {
    private readonly Guid _guid;
    
    public ScopeGuid() => _guid = Guid.NewGuid();

    public void Deconstruct(out Guid guid) => guid = _guid;

    public static implicit operator Guid(ScopeGuid scopeGuid) => scopeGuid._guid;
}