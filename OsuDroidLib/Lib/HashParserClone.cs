using BCrypt.Net;

namespace OsuDroidLib.Lib;

public static class HashParser {
    private static readonly HashParser.HashFormatDescriptor
        OldFormatDescriptor = new HashParser.HashFormatDescriptor(1);

    private static readonly HashParser.HashFormatDescriptor
        NewFormatDescriptor = new HashParser.HashFormatDescriptor(2);

    public static HashInformation GetHashInformation(string hash) {
        HashParser.HashFormatDescriptor format;
        if (!HashParser.IsValidHash(hash, out format))
            HashParser.ThrowInvalidHashFormat();
        return new HashInformation(hash.Substring(0, format.SettingLength), hash.Substring(1, format.VersionLength),
            hash.Substring(format.WorkfactorOffset, 2), hash.Substring(format.HashOffset));
    }

    public static int GetWorkFactor(string hash) {
        HashParser.HashFormatDescriptor format;
        if (!HashParser.IsValidHash(hash, out format))
            HashParser.ThrowInvalidHashFormat();
        int workfactorOffset = format.WorkfactorOffset;
        return 10 * ((int)hash[workfactorOffset] - 48) + ((int)hash[workfactorOffset + 1] - 48);
    }

    public static bool IsValidHash(string hash) {
        return IsValidHash(hash, out var _);
    }

    private static bool IsValidHash(string hash, out HashParser.HashFormatDescriptor format) {
        if (hash == null)
            throw new ArgumentNullException(nameof(hash));
        if (hash.Length != 59 && hash.Length != 60) {
            format = (HashParser.HashFormatDescriptor)null;
            return false;
        }

        if (!hash.StartsWith("$2")) {
            format = (HashParser.HashFormatDescriptor)null;
            return false;
        }

        int index1 = 2;
        if (HashParser.IsValidBCryptVersionChar(hash[index1])) {
            ++index1;
            format = HashParser.NewFormatDescriptor;
        }
        else
            format = HashParser.OldFormatDescriptor;

        string str1 = hash;
        int index2 = index1;
        int num1 = index2 + 1;
        if (str1[index2] != '$') {
            format = (HashParser.HashFormatDescriptor)null;
            return false;
        }

        string str2 = hash;
        int index3 = num1;
        int num2 = index3 + 1;
        if (HashParser.IsAsciiNumeric(str2[index3])) {
            string str3 = hash;
            int index4 = num2;
            int num3 = index4 + 1;
            if (HashParser.IsAsciiNumeric(str3[index4])) {
                string str4 = hash;
                int index5 = num3;
                int num4 = index5 + 1;
                if (str4[index5] != '$') {
                    format = (HashParser.HashFormatDescriptor)null;
                    return false;
                }

                for (int index6 = num4; index6 < hash.Length; ++index6) {
                    if (!HashParser.IsValidBCryptBase64Char(hash[index6])) {
                        format = (HashParser.HashFormatDescriptor)null;
                        return false;
                    }
                }

                return true;
            }
        }

        format = (HashParser.HashFormatDescriptor)null;
        return false;
    }

    private static bool IsValidBCryptVersionChar(char value) =>
        value == 'a' || value == 'b' || value == 'x' || value == 'y';

    private static bool IsValidBCryptBase64Char(char value) {
        if (value == '.' || value == '/' || value >= '0' && value <= '9' || value >= 'A' && value <= 'Z')
            return true;
        return value >= 'a' && value <= 'z';
    }

    private static bool IsAsciiNumeric(char value) => value >= '0' && value <= '9';

    private static void ThrowInvalidHashFormat() => throw new SaltParseException("Invalid Hash Format");

    private class HashFormatDescriptor {
        public HashFormatDescriptor(int versionLength) {
            this.VersionLength = versionLength;
            this.WorkfactorOffset = 1 + this.VersionLength + 1;
            this.SettingLength = this.WorkfactorOffset + 2;
            this.HashOffset = this.SettingLength + 1;
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
            this.Settings = settings;
            this.Version = version;
            this.WorkFactor = workFactor;
            this.RawHash = rawHash;
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