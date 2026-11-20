using System.Text.RegularExpressions;

namespace Apps.SFTP.Utils;

public static partial class PrivateKeyNormalizer
{
    private static readonly Regex Pattern = PemRegex();
    private const int PemLineLength = 64;   // https://www.rfc-editor.org/info/rfc7468/

    public static string Normalize(string key)
    {
        var match = Pattern.Match(key.Trim());
        if (!match.Success)
            return key;
        
        var body = Regex.Replace(match.Groups["body"].Value, @"\s+", string.Empty);
        var lines = body.Chunk(PemLineLength).Select(c => new string(c));

        return string.Join("\n", [match.Groups["header"].Value, .. lines, match.Groups["footer"].Value]) + "\n";
    }

    [GeneratedRegex(
        "(?<header>-----BEGIN [A-Z ]+-----)(?<body>.+?)(?<footer>-----END [A-Z ]+-----)", 
        RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex PemRegex();
}