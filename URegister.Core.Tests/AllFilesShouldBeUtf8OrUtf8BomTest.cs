using System.Text;

namespace URegister.Core.Tests;

public class EncodingTests
{
    [Test]
    public void AllFiles_ShouldBeUtf8OrUtf8BomTest()
    {
        // Get the solution directory
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

        // Define the file types to check (you can modify this as needed)
        var fileExtensions = new[] { ".cs", ".txt", ".json", ".xml", ".html", ".css", ".js", ".cshtml" };

        // Get all relevant files in the project directory
        var files = Directory.GetFiles(projectDirectory, "*.*", SearchOption.AllDirectories)
            .Where(f => fileExtensions.Contains(Path.GetExtension(f).ToLower()) &&
                        !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) &&
                        !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                        !f.Contains(Path.DirectorySeparatorChar + "git" + Path.DirectorySeparatorChar) &&
                        !f.Contains(Path.DirectorySeparatorChar + ".vs" + Path.DirectorySeparatorChar))
                            .ToList();

        var invalidFiles = files.Where(file =>
        {
            byte[] fileBytes = File.ReadAllBytes(file);

            if (fileBytes.Length == 0)
                return false; // Skip empty files

            return !IsUtf(fileBytes);
        }).ToList();

        Assert.IsEmpty(invalidFiles, $"The following files are not UTF-8 or UTF-8 BOM encoded:\n{string.Join("\n", invalidFiles)}");
    }

    static bool IsUtf(byte[] buffer)
    {
        // Check for UTF-8 BOM
        if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            return true;
        }

        // Check if it's UTF-8 without BOM
        if (IsUtf8WithoutBom(buffer))
        {
            return true;
        }

        // If it's not UTF-8, assume ANSI (Windows-1251 or Windows-1252)
        return false;
    }

    static bool IsUtf8WithoutBom(byte[] buffer)
    {
        int i = 0;
        while (i < buffer.Length)
        {
            if (buffer[i] < 0x80) // ASCII range (0-127)
            {
                i++;
                continue;
            }

            // Check for valid UTF-8 multi-byte sequences
            if (buffer[i] >= 0xC2 && buffer[i] <= 0xF4)
            {
                int expectedBytes = 0;

                if (buffer[i] >= 0xC2 && buffer[i] <= 0xDF) expectedBytes = 1;
                else if (buffer[i] >= 0xE0 && buffer[i] <= 0xEF) expectedBytes = 2;
                else if (buffer[i] >= 0xF0 && buffer[i] <= 0xF4) expectedBytes = 3;

                if (i + expectedBytes >= buffer.Length) return false; // Incomplete sequence

                for (int j = 1; j <= expectedBytes; j++)
                {
                    if (buffer[i + j] < 0x80 || buffer[i + j] > 0xBF)
                        return false;  // Invalid UTF-8 sequence
                }

                i += expectedBytes + 1;
            }
            else
            {
                return false;  // Invalid UTF-8 byte found
            }
        }

        return true;  // All checks passed, it's UTF-8 without BOM
    }
}