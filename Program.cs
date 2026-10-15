using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("______ ЗАДАЧА 1: 3 файла ______=\n");

        string[] paths =
        {
            @"C:\Users\Public\Файлы для 3 ДЗ\file_1.txt",
            @"C:\Users\Public\Файлы для 3 ДЗ\file_2.txt",
            @"C:\Users\Public\Файлы для 3 ДЗ\file_3.txt"
        };

        // --- Параллельно ---
        var sw = Stopwatch.StartNew();
        FileResult[] parallelResults = await Task.WhenAll(
            paths.Select(p => CountSpacesInFileAsync(p)));
        sw.Stop();
        long parallelMs = sw.ElapsedMilliseconds;

        PrintResults(parallelResults);
        Console.WriteLine($"[Параллельно] Время: {parallelMs} мс\n");

        Console.WriteLine("______ ЗАДАЧА 2: все файлы из папки ______\n");

        string folder = @"C:\Users\Public\Файлы для 3 ДЗ";

        sw.Restart();
        List<FileResult> folderResults = await CountSpacesInFolderAsync(folder);
        sw.Stop();
        long folderMs = sw.ElapsedMilliseconds;

        PrintResults(folderResults);
        Console.WriteLine($"Файлов: {folderResults.Count}, " +
                          $"пробелов всего: {folderResults.Where(r => r.IsSuccess).Sum(r => r.SpaceCount)}");
        Console.WriteLine($"[Папка, параллельно] Время: {folderMs} мс");
    }

    // ----- Функция для папки -----
    static async Task<List<FileResult>> CountSpacesInFolderAsync(string folderPath)
    {
        var results = new List<FileResult>();

        if (!Directory.Exists(folderPath))
        {
            results.Add(new FileResult(folderPath, 0, "Папка не найдена"));
            return results;
        }

        string[] files;
        try
        {
            files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
        }
        catch (Exception ex)
        {
            results.Add(new FileResult(folderPath, 0, $"Ошибка перечисления: {ex.Message}"));
            return results;
        }

        Task<FileResult>[] tasks = files
            .Select(file => CountSpacesInFileAsync(file))
            .ToArray();

        FileResult[] taskResults = await Task.WhenAll(tasks);
        results.AddRange(taskResults);
        return results;
    }

    // ----- Чтение одного файла -----
    static async Task<FileResult> CountSpacesInFileAsync(string path)
    {
        try
        {
            string content = await File.ReadAllTextAsync(path);
            int spaces = content.Count(c => c == ' ');
            return new FileResult(path, spaces, null);
        }
        catch (Exception ex)
        {
            return new FileResult(path, 0, ex.Message);
        }
    }

    // ----- Вывод -----
    static void PrintResults(IEnumerable<FileResult> results)
    {
        foreach (var r in results)
        {
            if (r.IsSuccess)
                Console.WriteLine($"  {r.Path}: {r.SpaceCount} пробелов");
            else
                Console.WriteLine($"  {r.Path}: ОШИБКА — {r.Error}");
        }
    }
}

class FileResult
{
    public string Path { get; }
    public int SpaceCount { get; }
    public string Error { get; }

    public FileResult(string path, int spaceCount, string error)
    {
        Path = path;
        SpaceCount = spaceCount;
        Error = error;
    }

    public bool IsSuccess => Error == null;
}