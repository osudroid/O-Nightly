using BCrypt.Net;

namespace OsuDroidLib.Lib;

public static class HashParser {
    private static readonly HashFormatDescriptor
        OldFormatDescriptor = new(1);

    private static readonly HashFormatDescriptor
        NewFormatDescriptor = new(2);

    public static HashInformation GetHashInformation(string hash) {
        HashFormatDescriptor format;
        if (!IsValidHash(hash, out format))
            ThrowInvalidHashFormat();
        return new HashInformation(hash.Substring(0, format.SettingLength), hash.Substring(1, format.VersionLength),
            hash.Substring(format.WorkfactorOffset, 2), hash.Substring(format.HashOffset)
        );
    }

    public static int GetWorkFactor(string hash) {
        HashFormatDescriptor format;
        if (!IsValidHash(hash, out format))
            ThrowInvalidHashFormat();
        var workfactorOffset = format.WorkfactorOffset;
        return 10 * (hash[workfactorOffset] - 48) + (hash[workfactorOffset + 1] - 48);
    }

    public static bool IsValidHash(string hash) {
        return IsValidHash(hash, out var _);
    }

    private static bool IsValidHash(string hash, out HashFormatDescriptor format) {
        if (hash == null)
            throw new ArgumentNullException(nameof(hash));
        if (hash.Length != 59 && hash.Length != 60) {
            format = null;
            return false;
        }

        if (!hash.StartsWith("$2")) {
            format = null;
            return false;
        }

        var index1 = 2;
        if (IsValidBCryptVersionChar(hash[index1])) {
            ++index1;
            format = NewFormatDescriptor;
        }
        else {
            format = OldFormatDescriptor;
        }

        var str1 = hash;
        var index2 = index1;
        var num1 = index2 + 1;
        if (str1[index2] != '$') {
            format = null;
            return false;
        }

        var str2 = hash;
        var index3 = num1;
        var num2 = index3 + 1;
        if (IsAsciiNumeric(str2[index3])) {
            var str3 = hash;
            var index4 = num2;
            var num3 = index4 + 1;
            if (IsAsciiNumeric(str3[index4])) {
                var str4 = hash;
                var index5 = num3;
                var num4 = index5 + 1;
                if (str4[index5] != '$') {
                    format = null;
                    return false;
                }

                for (var index6 = num4; index6 < hash.Length; ++index6)
                    if (!IsValidBCryptBase64Char(hash[index6])) {
                        format = null;
                        return false;
                    }

                return true;
            }
        }

        format = null;
        return false;
    }

    private static bool IsValidBCryptVersionChar(char value) {
        return value == 'a' || value == 'b' || value == 'x' || value == 'y';
    }

    private static bool IsValidBCryptBase64Char(char value) {
        if (value == '.' || value == '/' || (value >= '0' && value <= '9') || (value >= 'A' && value <= 'Z'))
            return true;
        return value >= 'a' && value <= 'z';
    }

    private static bool IsAsciiNumeric(char value) {
        return value >= '0' && value <= '9';
    }

    private static void ThrowInvalidHashFormat() {
        throw new SaltParseException("Invalid Hash Format");
    }

    private class HashFormatDescriptor {
        public HashFormatDescriptor(int versionLength) {
            VersionLength = versionLength;
            WorkfactorOffset = 1 + VersionLength + 1;
            SettingLength = WorkfactorOffset + 2;
            HashOffset = SettingLength + 1;
        }

        public int VersionLength { get; }

        public int WorkfactorOffset { get; }

        public int SettingLength { get; }

        public int HashOffset { get; }
    }

    public sealed class HashInformation {
        /// <summary>Constructor. </summary>
        /// <param name="settings">The message.</param>
        /// <param name="version">The message.</param>
        /// <param name="workFactor">The message.</param>
        /// <param name="rawHash">The message.</param>
        internal HashInformation(string settings, string version, string workFactor, string rawHash) {
            Settings = settings;
            Version = version;
            WorkFactor = workFactor;
            RawHash = rawHash;
        }

        /// <summary>Settings string</summary>
        public string Settings { get; private set; }

        /// <summary>Hash Version</summary>
        public string Version { get; private set; }

        /// <summary>log rounds used / workfactor</summary>
        public string WorkFactor { get; private set; }

        /// <summary>Raw Hash</summary>
        public string RawHash { get; private set; }
    }
}