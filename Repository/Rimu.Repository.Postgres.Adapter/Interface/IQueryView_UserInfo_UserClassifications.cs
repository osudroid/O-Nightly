using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IQueryView_UserInfo_UserClassifications {
    public Task<ResultOk<View_UserInfo_UserClassifications[]>> GetAllWithSingleActiveClassificationsAndPublicShowAsync();
}