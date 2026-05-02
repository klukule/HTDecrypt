using System.Text;

namespace HTCrypto;

public static class KnownKeys
{
    /// <summary>
    /// Global client - Windows
    /// </summary>
    public static readonly byte[] Windows = Convert.FromBase64String("VVZiUDZwamp3NUtaaHZkZGllM3RmaGcxcFZra3ZlWTg=");
    /// <summary>
    /// Global client - iOS
    /// NOTE: Untested
    /// </summary>
    public static readonly byte[] IOS = Convert.FromBase64String("I2J3M0ZaFfHVO1Z8ZLIEcw==");
    
    /// <summary>
    /// Mainland China client - Windows
    /// </summary>
    public static readonly byte[] WindowsCN = Convert.FromBase64String("MXpoNklPbElvaHJSODhVTlBqaUxpc3JrV0FDVVFZdXo=");

    public static byte[] Get(KeyId id) => id switch
    {
        KeyId.Windows => Windows,
        KeyId.IOS => IOS,
        KeyId.WindowsCN => WindowsCN,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null),
    };

    public static readonly IReadOnlyList<KeyId> AutoDetectOrder =
    [
        KeyId.Windows,
        KeyId.WindowsCN,
        KeyId.IOS
    ];
}
