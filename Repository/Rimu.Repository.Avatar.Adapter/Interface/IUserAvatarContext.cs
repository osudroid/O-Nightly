using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Avatar.Adapter.Interface;

public interface IUserAvatarContext {
    public Task<ResultOk<View_UserAvatarNoBytes[]>> InsertOrOverwriteWithNewAvatarAsync(byte[] imageBytes);

    public Task<ResultOk<View_UserAvatarNoBytes[]>> FindByUserIdAsync();
    
    public Task<ResultOk<Option<View_UserAvatarNoBytes>>> FindByUserIdAndHashAsync(string hash);
    
    public Task<ResultOk<Option<UserAvatar>>> FindAvatarByUserIdAndHashAsync(string hash);
    
    public Task<ResultOk<Option<UserAvatar>>> FindAvatarLowByUserIdAsync();
    public Task<ResultOk<Option<UserAvatar>>> FindAvatarHighByUserIdAsync();
    public Task<ResultOk<Option<UserAvatar>>> FindAvatarOriginalByUserIdAsync();

    public Task<ResultOk<UserAvatar>> ToPngAsync(UserAvatar userAvatar);
}