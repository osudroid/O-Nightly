using LamLibAllOver;
using OsuDroidMediator.Domain.Model;

namespace OsuDroidMediator.Domain.Interface; 

public interface ITransaction<T> where T : IResponse {
    public EModelResult Result { get; }
    public Option<T> OptionResponse { get; }
    public ResultErr<string> Err { get; }
    public string UserErrorMessage { get; }
}