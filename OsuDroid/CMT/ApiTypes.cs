using OsuDroid.Lib;

namespace OsuDroid.CMT;

public static class ApiTypes {
    public class ExistOrFoundInfo<T> {
        public bool ExistOrFound { get; set; }
        public T? Value { get; set; }

        public static ExistOrFoundInfo<T> Exist(T? value) {
            return new ExistOrFoundInfo<T> {
                ExistOrFound = true,
                Value = value
            };
        }

        public static ExistOrFoundInfo<T> NotExist() {
            return new ExistOrFoundInfo<T> {
                ExistOrFound = false,
                Value = default
            };
        }
    }

    public class Work {
        public static Work False = new() { HasWork = false };
        public static Work True = new() { HasWork = true };
        public bool HasWork { get; init; }
    }

    public class Api2GroundNoHeader<T> : IValuesAreGood where T : IValuesAreGood, ISingleString {
        public T? Body { get; set; }

        public bool ValuesAreGood() {
            if (Body is null) return false;
            if (Body.ValuesAreGood() == false) return false;
            return true;
        }
    }

    public class Api2Ground<T> : IValuesAreGood where T : IValuesAreGood, ISingleString {
        public Api2GroundHeader? Header { get; set; }

        public T? Body { get; set; }

        public bool ValuesAreGood() {
            if (Header is null) return false;
            if (Body is null) return false;
            if (Header.ValuesAreGood() == false) return false;
            if (Body.ValuesAreGood() == false) return false;
            return true;
        }
    }

    public class Api2GroundWithHash<T> : IValuesAreGood, IPrintHashOrder
        where T : IValuesAreGood, ISingleString, IPrintHashOrder {
        public Api2GroundHeaderWithHash? Header { get; set; }

        public T? Body { get; set; }

        public string PrintHashOrder() {
            return Body is not null ? PrintHashOrder() : "";
        }

        public bool ValuesAreGood() {
            if (Header is null) return false;
            if (Body is null) return false;
            if (Header.ValuesAreGood() == false) return false;
            if (Body.ValuesAreGood() == false) return false;
            return true;
        }

        public bool HashValidate() {
            if (Header is null) return false;
            if (Body is null) return false;

            return Security.GetSecurity().Api2HashValidate(
                Header.HashBodyData ?? "",
                Body.ToSingleString(),
                Env.Keyword ?? "");
        }
    }


    public sealed class Api2GroundHeader : IValuesAreGood {
        public Guid Token { get; set; }

        public bool ValuesAreGood() {
            return Token != Guid.Empty;
        }
    }

    public sealed class Api2GroundHeaderWithHash : IValuesAreGood {
        public Guid Token { get; set; }
        public string? HashBodyData { get; set; }

        public bool ValuesAreGood() {
            return Token != Guid.Empty && HashBodyData is not null;
        }
    }

    public interface IValuesAreGood {
        public bool ValuesAreGood();
    }

    public interface ISingleString {
        public string ToSingleString();
    }

    public interface IPrintHashOrder {
        public string PrintHashOrder();
    }
}