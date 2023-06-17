using OsuDroid.Lib;

namespace OsuDroid.Post; 

public static class Api2 {
        public class PostApi2GroundNoHeader<T> : IValuesAreGood where T : IValuesAreGood, ISingleString {
        public T? Body { get; set; }

        public bool ValuesAreGood() {
            if (Body is null) return false;
            if (Body.ValuesAreGood() == false) return false;
            return true;
        }
    }

    public class PostApi2Ground<T> : IValuesAreGood where T : IValuesAreGood, ISingleString {
        public PostApi2GroundHeader? Header { get; set; }

        public T? Body { get; set; }

        public bool ValuesAreGood() {
            if (Header is null) return false;
            if (Body is null) return false;
            if (Header.ValuesAreGood() == false) return false;
            if (Body.ValuesAreGood() == false) return false;
            return true;
        }
    }

    public class PostApi2GroundWithHash<T> : IValuesAreGood, IPrintHashOrder
        where T : IValuesAreGood, ISingleString, IPrintHashOrder {
        public PostApi2GroundHeaderWithHash? Header { get; set; }

        public T? Body { get; set; }

        public string PrintHashOrder() {
            return Body is not null ? Body.PrintHashOrder() : "";
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
                Setting.RequestHash_Keyword!.Value);
        }
    }


    public sealed class PostApi2GroundHeader : IValuesAreGood {
        public Guid Token { get; set; }

        public bool ValuesAreGood() {
            return Token != Guid.Empty;
        }
    }

    public sealed class PostApi2GroundHeaderWithHash : IValuesAreGood {
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