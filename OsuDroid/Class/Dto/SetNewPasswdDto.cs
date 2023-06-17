using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;

namespace OsuDroid.Class.Dto;

public sealed class SetNewPasswdDto {
    public required string NewPasswd { get; init; }
    public required string Token { get; init; }
    public required long UserId { get; init; }
}