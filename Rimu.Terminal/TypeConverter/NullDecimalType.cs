using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Rimu.Terminal.TypeConverter;

internal class NullDecimalType : TypeConverter<double?> {
    public override double? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) {
        if (text is null) return null;
        if (text == "NULL") return null;
        return double.Parse(text);
    }

    public override string? ConvertToString(double? value, IWriterRow row, MemberMapData memberMapData) {
        return value.HasValue? value.Value.ToString(): "NULL";
    }
}