using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Rimu.Terminal.TypeConverter;

internal class NullLongType : TypeConverter<long?> {
    public override long? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) {
        if (text is null) return null;
        if (text == "NULL") return null;
        return long.Parse(text);
    }

    public override string? ConvertToString(long? value, IWriterRow row, MemberMapData memberMapData) {
        return value.HasValue? value.Value.ToString(): "NULL";
    }
}