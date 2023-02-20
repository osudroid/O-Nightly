using System.Diagnostics;

namespace OsuDroid.Utils; 

public static class StackTrace {
    public static string WithMessage(string message) => $"{message}\n{new System.Diagnostics.StackTrace(1, true)}";
}