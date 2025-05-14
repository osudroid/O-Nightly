namespace Rimu.Repository.Attachment.Adapter.Interface;

public interface IScopeGuid {
    public Guid Id { get; }

    public bool Equals(IScopeGuid other) => Id.Equals(other.Id);
}