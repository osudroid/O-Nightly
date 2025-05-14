using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Rimu.Terminal.TypeConverter;

internal class NullStringType : TypeConverter<string> {
    public override string? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) {
        if (text is null) return null;
        if (text == "NULL") return null;
        return text;
    }

    public override string? ConvertToString(string? value, IWriterRow row, MemberMapData memberMapData) {
        if (value is null) return null;
        if (value == "NULL") return null;
        return value;
    }
}