using OsuDroidLib.Dto;

namespace OsuDroid.Class.Dto; 

public class TopPlaysByMarkPageingDto {
    public required long UserId { get; init; } 
    public required PlayScoreDto.EPlayScoreMark MarkString { get; init; } 
    public required int Page { get; init; }
}