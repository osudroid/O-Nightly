namespace OsuDroid.CMT;

public interface IFactoryError<out T> where T : AbsTsSendType, IFactoryError<T> {
    public static abstract T FactoryError(string errorMsg);
}