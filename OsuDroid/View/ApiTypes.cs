using OsuDroid.Lib;

namespace OsuDroid.Class;

public static class ApiTypes {
    public class ViewExistOrFoundInfo<T> {
        public bool ExistOrFound { get; set; }
        public T? Value { get; set; }

        public static ViewExistOrFoundInfo<T> Exist(T? value) {
            return new ViewExistOrFoundInfo<T> {
                ExistOrFound = true,
                Value = value
            };
        }

        public static ViewExistOrFoundInfo<T> NotExist() {
            return new ViewExistOrFoundInfo<T> {
                ExistOrFound = false,
                Value = default
            };
        }
    }

    public class ViewWork {
        public static ViewWork False = new() { HasWork = false };
        public static ViewWork True = new() { HasWork = true };
        public bool HasWork { get; init; }
    }
}