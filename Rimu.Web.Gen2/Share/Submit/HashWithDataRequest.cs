using Rimu.Web.Gen2.Interface;

namespace Rimu.Web.Gen2.Share.Submit;

public sealed class HashWithDataRequest<T>: IHashData<T> where T : class, ISingleString, new() {
    public string Hash { get; set; } = "";
    public T? Data { get; set; }
}