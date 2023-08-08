namespace OsuDroid.HttpGet; 

public struct GetTopPlaysByMarkPageing {
    public required long UserId { get; init; } 
    public required string? MarkString { get; init; } 
    public required int Page { get; init; }
}