using System.Text;
using HTCrypto;

namespace HTEncrypt;

internal static class Program
{
    private const KeyId DefaultKey = KeyId.Windows;

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintBanner();
                PrintUsage();
                Pause();
                return 1;
            }

            KeyId keyId = DefaultKey;
            var fileArgs = new List<string>(args.Length);
            foreach (var a in args)
            {
                if (a.StartsWith("--key=", StringComparison.OrdinalIgnoreCase))
                {
                    var v = a.Substring("--key=".Length).Trim();
                    if (!Enum.TryParse<KeyId>(v, ignoreCase: true, out keyId))
                    {
                        Console.Error.WriteLine($"[ERROR] Unknown key '{v}'. Valid: {string.Join(", ", Enum.GetNames<KeyId>())}");
                        Pause();
                        return 1;
                    }
                }
                else if (a == "--help" || a == "-h" || a == "/?")
                {
                    PrintBanner();
                    PrintUsage();
                    Pause();
                    return 0;
                }
                else
                {
                    fileArgs.Add(a);
                }
            }

            if (fileArgs.Count == 0)
            {
                PrintBanner();
                PrintUsage();
                Pause();
                return 1;
            }

            PrintBanner();
            Console.WriteLine($"Using key: {keyId}");
            Console.WriteLine();

            int errors = 0;
            foreach (var path in fileArgs)
            {
                try
                {
                    ProcessFile(path, keyId);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ERROR] {path}: {ex.Message}");
                    errors++;
                }

                Console.WriteLine();
            }

            Console.WriteLine(errors == 0 ? "Done." : $"Done with {errors} error(s).");
            Pause();
            return errors == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[FATAL] {ex}");
            Pause();
            return 2;
        }
    }

    private static void ProcessFile(string inputPath, KeyId keyId)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("File not found", inputPath);

        Console.WriteLine($"Processing: {inputPath}");

        var lines = File.ReadAllLines(inputPath);
        var result = HTLineProcessor.EncryptLines(lines, keyId);

        var fullInput = Path.GetFullPath(inputPath);
        var dir = Path.GetDirectoryName(fullInput) ?? ".";
        var outputPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(inputPath) + ".encrypted" + Path.GetExtension(inputPath));

        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        File.WriteAllText(outputPath, result.Text, encoding);

        Console.WriteLine($"  Encrypted {result.ContentLines} content line(s); {result.BlankLineMarkers} blank-line marker(s).");
        Console.WriteLine($"  -> {outputPath}");
    }

    private static void PrintBanner()
    {
        Console.WriteLine("=== HTEncrypt -- NTE INI / log encryptor ===");
        Console.WriteLine();
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Drag and drop plaintext files onto this exe (uses default Windows key).");
        Console.WriteLine();
        Console.WriteLine("CLI:  HTEncrypt.exe [--key=windows|ios] <file1> [file2] ...");
        Console.WriteLine();
        Console.WriteLine("Default key when none specified: " + DefaultKey);
        Console.WriteLine("Available keys: " + string.Join(", ", Enum.GetNames<KeyId>()));
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        try
        {
            Console.ReadKey(intercept: true);
        }
        catch
        {
            /* no console */
        }
    }
}