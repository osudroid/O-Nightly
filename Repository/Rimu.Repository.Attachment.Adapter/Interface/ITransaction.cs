using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Attachment.Adapter.Enum;

namespace Rimu.Repository.Attachment.Adapter.Interface;

public interface ITransaction<T> {
    public EModelResult Result { get; }
    public Option<T> OptionResponse { get; }
    public SResultErr Err { get; }
    public string UserErrorMessage { get; }
}