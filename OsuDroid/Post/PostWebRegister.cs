using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostWebRegister : Api2.IValuesAreGood, Api2.ISingleString {
    public string? Email { get; set; }
    public int MathRes { get; set; }
    public Guid MathToken { get; set; }
    public string? Region { get; set; }
    public string? Passwd { get; set; }
    public string? Username { get; set; }
    public bool ValuesAreGood() {
        throw new NotImplementedException();
    }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
            Email??"",
            MathRes.ToString(),
            MathToken.ToString(),
            Region??"",
            Passwd??"",
            Username??"",
        });
    }
}