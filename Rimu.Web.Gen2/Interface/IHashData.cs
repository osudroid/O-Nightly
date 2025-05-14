namespace Rimu.Web.Gen2.Interface;

public interface IHashData<T> where T : class, ISingleString {
    public string? Hash { get; }
    public T? Data { get; set; }
}