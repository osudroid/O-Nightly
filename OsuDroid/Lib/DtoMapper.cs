using OsuDroid.Class.Dto;
using OsuDroid.Post;
using OsuDroid.View;
using OsuDroidMediator.Domain.Interface;
using OsuDroidMediator.Domain.Model.Dto;
using Riok.Mapperly.Abstractions;
using IDto = OsuDroid.Class.Dto.IDto;
using UpdateUsernameDto = OsuDroid.Class.Dto.UpdateUsernameDto;
using WebLoginWithUsernameDto = OsuDroid.Class.Dto.WebLoginWithUsernameDto;

namespace OsuDroid.Lib;

[Mapper]
public static partial class DtoMapper {
    public static IDto ViewToDto(PostApi.IValuesAreGood post) {
        
    }
    public static partial Api2MapFileRankDto Api2MapFileRankToDto(PostApi2MapFileRank post);
    public static partial Api2PlayByIdDto Api2PlayByIdToDto(PostApi2PlayById post);
    public static partial Api2UploadReplayFileDto Api2UploadReplayFileToDto(PostApi2UploadReplayFile post);

    public static partial Api2UploadReplayFilePropAsFormWrapperDto Api2UploadReplayFilePropAsFormWrapperToDto(
        PostApi2UploadReplayFilePropAsFormWrapper post);

    public static partial AvatarHashesByUserIdsDto AvatarHashesByUserIdsToDto(PostAvatarHashesByUserIds post);
    public static partial CreateApi2TokenDto CreateApi2TokenToDto(PostCreateApi2Token post);
    public static partial CreateDropAccountTokenDto CreateDropAccountTokenToDto(PostCreateDropAccountToken post);
    public static partial LeaderBoardDto LeaderBoardToDto(PostLeaderBoard post);
    public static partial LeaderBoardSearchUserDto LeaderBoardSearchUserToDto(PostLeaderBoardSearchUser post);
    public static partial LeaderBoardUserDto LeaderBoardUserToDto(PostLeaderBoardUser post);
    public static partial PushPlayDto PushPlayToDto(PostPushPlay post);
    public static partial PushPlayStartDto PushPlayStartToDto(PostPushPlayStart post);
    public static partial RecentPlaysDto RecentPlaysToDto(PostRecentPlays post);
    public static partial ResetPasswdAndSendEmailDto ResetPasswdAndSendEmailToDto(PostResetPasswdAndSendEmail post);
    public static partial SetNewPasswdDto SetNewPasswdToDto(PostSetNewPasswd post);
    public static partial SimpleTokenDto SimpleTokenToDto(PostSimpleToken post);
    public static partial UpdateAvatarDto UpdateAvatarToDto(PostUpdateAvatar post);
    public static partial UpdateEmailDto UpdateEmailToDto(PostUpdateEmail post);
    public static partial UpdatePasswdDto UpdatePasswdToDto(PostUpdatePasswd post);
    public static partial UpdatePatreonEmailDto UpdatePatreonEmailToDto(PostUpdatePatreonEmail post);
    public static partial UpdateUsernameDto UpdateUsernameToDto(PostUpdateUsername post);
    public static partial WebLoginDto WebLoginToDto(PostWebLogin post);
    public static partial WebLoginWithUsernameDto WebLoginWithUsernameToDto(PostWebLoginWithUsername post);
    public static partial WebRegisterDto WebRegisterToDto(PostWebRegister post);

    public static partial Api2MapFileRankDto Api2MapFileRankToPost(Api2MapFileRankDto dto);
    public static partial Api2PlayByIdDto Api2PlayByIdToPost(Api2PlayByIdDto dto);
    public static partial Api2UploadReplayFileDto Api2UploadReplayFileToPost(Api2UploadReplayFileDto dto);

    public static partial Api2UploadReplayFilePropAsFormWrapperDto Api2UploadReplayFilePropAsFormWrapperToPost(
        Api2UploadReplayFilePropAsFormWrapperDto dto);

    public static partial AvatarHashesByUserIdsDto AvatarHashesByUserIdsToPost(AvatarHashesByUserIdsDto dto);
    public static partial CreateApi2TokenDto CreateApi2TokenToPost(CreateApi2TokenDto dto);
    public static partial CreateDropAccountTokenDto CreateDropAccountTokenToPost(CreateDropAccountTokenDto dto);
    public static partial LeaderBoardDto LeaderBoardToPost(LeaderBoardDto dto);
    public static partial LeaderBoardSearchUserDto LeaderBoardSearchUserToPost(LeaderBoardSearchUserDto dto);
    public static partial LeaderBoardUserDto LeaderBoardUserToPost(LeaderBoardUserDto dto);
    public static partial PushPlayDto PushPlayToPost(PushPlayDto dto);
    public static partial PushPlayStartDto PushPlayStartToPost(PushPlayStartDto dto);
    public static partial RecentPlaysDto RecentPlaysToPost(RecentPlaysDto dto);
    public static partial ResetPasswdAndSendEmailDto ResetPasswdAndSendEmailToPost(ResetPasswdAndSendEmailDto dto);
    public static partial SetNewPasswdDto SetNewPasswdToPost(SetNewPasswdDto dto);
    public static partial SimpleTokenDto SimpleTokenToPost(SimpleTokenDto dto);
    public static partial UpdateAvatarDto UpdateAvatarToPost(UpdateAvatarDto dto);
    public static partial UpdateEmailDto UpdateEmailToPost(UpdateEmailDto dto);
    public static partial UpdatePasswdDto UpdatePasswdToPost(UpdatePasswdDto dto);
    public static partial UpdatePatreonEmailDto UpdatePatreonEmailToPost(UpdatePatreonEmailDto dto);
    public static partial UpdateUsernameDto UpdateUsernameToPost(UpdateUsernameDto dto);
    public static partial WebLoginDto WebLoginToPost(WebLoginDto dto);
    public static partial WebLoginWithUsernameDto WebLoginWithUsernameToPost(WebLoginWithUsernameDto dto);
    public static partial WebRegisterDto WebRegisterToPost(WebRegisterDto dto);

    public static partial ViewWebLogin ToViewWebLogin(WebLoginResponseDto dto);
    
    public static partial ViewUserInfo ToViewUserInfo(ICookieUserInfo dto);
}