using System.Text.Json;
using LamLibAllOver.ErrorHandling;

namespace Rimu.Web.Gen2.Share.Signup;

public sealed class TokenWithGroupDataDto {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";

    public static ResultOk<TokenWithGroupDataDto> FromJson(string json) {
        try {
            var dto = JsonSerializer.Deserialize<TokenWithGroupDataDto>(json);
            if (dto is null) {
                Logger.Error($"Can't deserialize {json}");
                return ResultOk<TokenWithGroupDataDto>.Err(); 
            }
            return ResultOk<TokenWithGroupDataDto>.Ok(dto);
        }
        catch (Exception e) {
            Logger.Trace(e, "Can't deserialize {json}", json);
            return ResultOk<TokenWithGroupDataDto>.Err();
        }
    }

    public string ToJson() => JsonSerializer.Serialize(this);
}