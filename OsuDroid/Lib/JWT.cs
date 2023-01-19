#nullable enable
// using JWT.Algorithms;
// using JWT.Builder;

namespace OsuDroid.Lib;

// public static class Jwt {
// public static string? BuildToken(JwtCookie jwtCookie) {
//     return JwtBuilder.Create()
//         .WithAlgorithm(new HMACSHA256Algorithm())
//         .WithSecret(Env.JwtSecret)
//         .AddClaim("hash", jwtCookie.Hash)
//         .AddClaim("userId", jwtCookie.UserId)
//         .AddClaim("createTime", Time.ToScyllaString(jwtCookie.CreateTime))
//         .Encode();
// }
//
// public static JwtCookie? BuildJwtCookie(string token) {
//     var json = JwtBuilder.Create()
//         .WithAlgorithm(new HMACSHA256Algorithm())
//         .WithSecret(Env.JwtSecret)
//         .Decode(token);
//     if (json is null) return null;
//     try {
//         return JsonConvert.DeserializeObject<JwtCookie>(json);
//     }
//     catch (Exception) {
//         return null;
//     }
// }
//
// public record struct JwtCookie(string Hash, string UserId, DateTime CreateTime) {
//     public enum EHealth {
//         Top,
//         Dead
//     }
//
//     private static readonly TimeSpan LifeTime = new(TimeSpan.TicksPerDay * 30);
//
//     public EHealth IsHealthOk() {
//         return DateTime.UtcNow > CreateTime.Add(LifeTime)
//             ? EHealth.Dead
//             : EHealth.Top;
//     }
//
//     public static JwtCookie Factory(string userUuid) {
//         return new(Env.JwtHash, userUuid, DateTime.UtcNow);
//     }
//
//     public JwtCookie WithNewCreateTime() {
//         return this with {CreateTime = DateTime.UtcNow};
//     }
// }
// }