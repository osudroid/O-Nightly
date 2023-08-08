namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class PostAvatarHashesByUserIds : Api2.IValuesAreGood, Api2.ISingleString {
    public int Size { get; set; }
    public long[]? UserIds { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new object[] {
            Size,
            Merge.ListToString(UserIds ?? Array.Empty<long>())
        });
    }

    public bool ValuesAreGood() {
        return !(
            Size > 1000
            || Size < 1
            || UserIds is null or { Length: > 1000 or 0 });
    }
}