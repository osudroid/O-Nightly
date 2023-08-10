using LamLibAllOver;

namespace OsuDroidAttachment.Interface;

public interface IDbCreates<Db> where Db : IDb {
    public ValueTask<Result<Db, string>> Create();
}