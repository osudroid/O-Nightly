namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class PostSimpleToken : Api2.IValuesAreGood, Api2.ISingleString {
    public Guid Token { get; set; }

    public bool ValuesAreGood() {
        return Token != default;
    }

    public string ToSingleString() {
        return Token.ToString();
    }
}