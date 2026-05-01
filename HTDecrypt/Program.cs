using System.Text;
using HTCrypto;

namespace HTDecrypt;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintBanner();
                Console.WriteLine("Drag and drop one or more encrypted files onto this exe.");
                Console.WriteLine("Or run: HTDecrypt.exe <file1> [file2] ...");
                Console.WriteLine();
                Console.WriteLine($"Known keys: {string.Join(", ", KnownKeys.AutoDetectOrder)}");
                Pause();
                return 1;
            }

            PrintBanner();

            int errors = 0;
            foreach (var path in args)
            {
                try
                {
                    ProcessFile(path);
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

    private static void ProcessFile(string inputPath)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("File not found", inputPath);

        Console.WriteLine($"Processing: {inputPath}");

        var lines = File.ReadAllLines(inputPath);
        var result = HTLineProcessor.DecryptLines(lines);

        var fullInput = Path.GetFullPath(inputPath);
        var dir = Path.GetDirectoryName(fullInput) ?? ".";
        var outputPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(inputPath) + ".decrypted" + Path.GetExtension(inputPath));

        File.WriteAllText(outputPath, result.Text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var keyTag = result.KeysUsed.Count == 0 ? "no encrypted blocks" : "key=" + string.Join("+", result.KeysUsed);

        Console.WriteLine($"  Decrypted {result.DecryptedBlocks} block(s); kept {result.PassthroughLines} line(s) verbatim. ({keyTag})");
        Console.WriteLine($"  -> {outputPath}");
    }

    private static void PrintBanner()
    {
        Console.WriteLine("=== HTDecrypt -- NTE INI / log decryptor ===");
        Console.WriteLine();
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