using Rimu.Repository.Attachment.Adapter.Interface;

namespace Rimu.Repository.Attachment.Adapter.Class;

public sealed class ScopeGuid: IScopeGuid {
    private readonly Guid _guid;

    public Guid Id => _guid;
    
    public ScopeGuid() => _guid = Guid.NewGuid();
    
    public void Deconstruct(out Guid guid) => guid = _guid;
    
    public static implicit operator Guid(ScopeGuid scopeGuid) => scopeGuid._guid;

    public static bool operator ==(ScopeGuid a, ScopeGuid b) => a._guid == b._guid;

    public static bool operator !=(ScopeGuid a, ScopeGuid b) => a._guid != b._guid;

    public bool Equals(ScopeGuid other) => this == other;
    
    public override bool Equals(object? obj) {
        if (obj is ScopeGuid scopeGuid) {
            return this == scopeGuid;
        }

        throw new Exception("Object is not a ScopeGuid");
    }
    
    public override int GetHashCode() => _guid.GetHashCode();

    public override string ToString() => _guid.ToString();
}