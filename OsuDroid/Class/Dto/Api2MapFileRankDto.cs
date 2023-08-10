namespace OsuDroid.Class.Dto;

// ReSharper disable All
public class Api2MapFileRankDto : IDto {
    public required string Filename { get; init; }
    public required string FileHash { get; init; }
}