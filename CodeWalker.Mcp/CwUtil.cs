using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeWalker.GameFiles;
using SharpDX;

namespace CodeWalker.Mcp;

public static class CwUtil
{
    public static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    public static string ToJson(object o) => JsonSerializer.Serialize(o, Json);

    /// <summary>Accepts a name ("prop_bench_01"), a decimal hash, or a hex hash ("0x1234ABCD" / "hash_1234ABCD").</summary>
    public static uint ParseHash(string s)
    {
        s = s.Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
            uint.TryParse(s.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hx)) return hx;
        if (s.StartsWith("hash_", StringComparison.OrdinalIgnoreCase) &&
            uint.TryParse(s.AsSpan(5), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hh)) return hh;
        if (s.Length > 0 && char.IsDigit(s[0]) && uint.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out uint d)) return d;
        string l = s.ToLowerInvariant();
        JenkIndex.Ensure(l);
        return JenkHash.GenHash(l);
    }

    public static string HashName(uint hash)
    {
        if (hash == 0) return "";
        string s = JenkIndex.TryGetString(hash);
        return string.IsNullOrEmpty(s) ? "hash_" + hash.ToString("X8") : s;
    }

    /// <summary>Same as the RPF Explorer does for files opened from disk.</summary>
    public static RpfFileEntry CreateLooseEntry(string name, string path, ref byte[] data)
    {
        RpfFileEntry e;
        uint rsc7 = data.Length > 4 ? BitConverter.ToUInt32(data, 0) : 0;
        if (rsc7 == 0x37435352)
        {
            e = RpfFile.CreateResourceFileEntry(ref data, 0);
            data = ResourceBuilder.Decompress(data);
        }
        else
        {
            var be = new RpfBinaryFileEntry { FileSize = (uint)data.Length };
            be.FileUncompressedSize = be.FileSize;
            e = be;
        }
        e.Name = name;
        e.NameLower = name.ToLowerInvariant();
        e.NameHash = JenkHash.GenHash(e.NameLower);
        e.ShortNameHash = JenkHash.GenHash(Path.GetFileNameWithoutExtension(e.NameLower));
        e.Path = path;
        return e;
    }

    /// <summary>Turns extracted (decompressed) data back into a file that can sit on disk.</summary>
    public static byte[] ToLooseFileBytes(RpfFileEntry entry, byte[] data)
    {
        if (entry is RpfResourceFileEntry rre)
        {
            var c = ResourceBuilder.Compress(data);
            return ResourceBuilder.AddResourceHeader(rre, c);
        }
        return data;
    }

    // ----------------------------------------------------------- rotations
    // GTA is Z-up. Rotation is given as [x, y, z] degrees: pitch about X, roll about Y,
    // heading/yaw about Z (positive = turn left / counter-clockwise from above).
    // Applied in X, then Y, then Z order (the usual "ZYX" convention used by game scripts).

    public static Quaternion EulerToQuat(float xDeg, float yDeg, float zDeg)
    {
        var m = Matrix.RotationX(MathUtil.DegreesToRadians(xDeg))
              * Matrix.RotationY(MathUtil.DegreesToRadians(yDeg))
              * Matrix.RotationZ(MathUtil.DegreesToRadians(zDeg));
        return Quaternion.Normalize(Quaternion.RotationMatrix(m));
    }

    public static Vector3 QuatToEuler(Quaternion q)
    {
        var m = Matrix.RotationQuaternion(Quaternion.Normalize(q));
        float roll = (float)Math.Asin(Math.Clamp(-m.M13, -1f, 1f));
        float pitch, yaw;
        if (Math.Abs(m.M13) < 0.9999f)
        {
            pitch = (float)Math.Atan2(m.M23, m.M33);
            yaw = (float)Math.Atan2(m.M12, m.M11);
        }
        else
        {
            pitch = (float)Math.Atan2(-m.M32, m.M22);
            yaw = 0;
        }
        return new Vector3(Deg(pitch), Deg(roll), Deg(yaw));
    }

    private static float Deg(float r) => (float)Math.Round(MathUtil.RadiansToDegrees(r), 3);

    public static float[] Arr(Vector3 v) => new[] { R(v.X), R(v.Y), R(v.Z) };
    public static float[] Arr(Quaternion q) => new[] { R(q.X, 6), R(q.Y, 6), R(q.Z, 6), R(q.W, 6) };
    public static float R(float f, int digits = 3) => (float)Math.Round(f, digits);

    public static Vector3 Vec(float[]? a, Vector3 fallback)
    {
        if (a == null || a.Length == 0) return fallback;
        if (a.Length != 3) throw new CwException("Expected an [x, y, z] array.");
        return new Vector3(a[0], a[1], a[2]);
    }

    /// <summary>Resolves an output path and makes sure its folder exists.</summary>
    public static string PrepareOutput(string path, bool overwrite)
    {
        string full = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
        if (File.Exists(full) && !overwrite)
            throw new CwException($"{full} already exists. Pass overwrite=true to replace it.");
        string? dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        return full;
    }

    public static string Truncate(string s, int max)
        => s.Length <= max ? s : s.Substring(0, max) + $"\n\n... [truncated: {s.Length - max:N0} more characters. Pass output_file to save the full text.]";
}
