
namespace OsuDroid.Utils; 

public static class Mode {
    /// <exception cref="Exception"></exception>
    public static string[] ModeAsSingleStringToModeArray(ReadOnlySpan<Char> mode) {
        var res = new List<string>(4);

        var hasPipe = false;
        var pipePosi = 0;
        for (var i = 0; i < mode.Length; i++) {
            var c = mode[i];
            if (c == '|') {
                hasPipe = true;
                pipePosi = i + 1;
                break;
            }
                    
            res.Add(c.ToString());
        }

        if (!hasPipe || mode.Slice(pipePosi).Length == 0) 
            return res.ToArray();
        
        var slice = mode.Slice(pipePosi);
        
        if (slice[0] != 'x')
            throw new Exception("Must be start with 'x'");
        res.Add(new String(slice));

        return res.ToArray();
    }

    /// <summary> Return Empty If Error </summary>
    public static Option<string> ModeArrayToModeAsSingleString(IEnumerable<string>? modes) {
        if (modes is null)
            return Option<string>.With("|");
        
        StringBuilder builder = new StringBuilder(2);
        string end = "";

        foreach (var mode in modes) {
            if (mode.Length == 0)
                continue;
            
            if (mode.Length == 1) {
                builder.Append(mode);
                continue;
            }

            if (end.Length == 0)
                return Option<string>.Empty;
            
            if (mode[0] != 'x')
                return Option<string>.Empty;

            end = mode;
        }

        builder.Append('|');
        builder.Append(end);
        
        return Option<string>.With(builder.ToString());
    }
}