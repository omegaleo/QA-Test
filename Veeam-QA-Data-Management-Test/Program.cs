using System;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: Veeam-QA-Data-Management-Test.exe --input-directory | -i [inputDir] --output-directory | -o [outputDir] --interval | -int [interval] --log-file | -l [logFile]");
            return;
        }

        var inputDirectory = "";
        var outputDirectory = "";
        var interval = 0;
        var logFile = "";

        if (!AssignArguments(args, ref inputDirectory, ref outputDirectory, ref interval, ref logFile)) return;

        HandleArguments(inputDirectory, outputDirectory, interval, logFile);
        
        // Let's create our timer to run periodically on the set interval
        Timer timer = new Timer(Sync, (inputDirectory, outputDirectory, logFile), TimeSpan.Zero, TimeSpan.FromSeconds(interval));
        
        Console.WriteLine($"Monitoring and processing at an interval of {interval} seconds. Press Enter to exit.");
        Console.ReadLine();
    }

    private static void Sync(object state)
    {
        Console.WriteLine($"Synchronization process started at: {DateTime.Now}");
        
        // Extract inputDirectory and outputDirectory from the state object.
        (string inputDir, string outputDir, string logFile) = ((string, string, string))state;

        // Get all the files in our input directory
        var filesInInput = Directory.GetFiles(inputDir);

        foreach (var inputFilePath in filesInInput)
        {
            // Get filename and extension from current file path
            var fileName = Path.GetFileName(inputFilePath);
            
            // Let's create our output file's path from the outputDir and also the filename and extension from the current filePath we're looking at.
            var outputFilePath = Path.Join(outputDir, fileName);

            if (File.Exists(outputFilePath))
            {
                WriteToLog(logFile, $"Deleted file at path: {outputFilePath} to avoid conflicts while synchronizing.");
                File.Delete(outputFilePath);
            }
            
            File.Copy(inputFilePath, outputFilePath);
            WriteToLog(logFile, $"Copied {fileName} to directory: {outputDir}");
        }
        
        Console.WriteLine($"Synchronization process finished at: {DateTime.Now}");
    }

    private static void WriteToLog(string path, string message)
    {
        if (!File.Exists(path))
        {
            File.CreateText(path);
        }

        File.AppendAllText(path, $"[{DateTime.Now}] {message}{Environment.NewLine}");
    }
    
    private static void HandleArguments(string? inputDirectory, string? outputDirectory, int interval, string? logFile)
    {
        // In case any of the arguments is invalid, let's return an error
        if (string.IsNullOrWhiteSpace(inputDirectory))
        {
            throw new ArgumentNullException(nameof(inputDirectory));
        }
        
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentNullException(nameof(outputDirectory));
        }
        
        if (interval <= 0)
        {
            throw new ArgumentNullException(nameof(interval));
        }
        
        if (string.IsNullOrWhiteSpace(logFile))
        {
            throw new ArgumentNullException(nameof(logFile));
        }

        // In case the input directory doesn't exist, let's return an error
        if (!Directory.Exists(inputDirectory))
        {
            Console.WriteLine("Input Directory does not exist!");
            return;
        }

        // In case the output directory doesn't exist, let's create it
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine("Output directory was created as it didn't exist.");
        }
    }

    private static bool AssignArguments(string[] args, ref string? inputDirectory, ref string? outputDirectory,
        ref int interval, ref string? logFile)
    {
        // let's assign our arguments to their respective variables
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--input-directory":
                case "-i":
                    inputDirectory = args[++i];
                    break;
                case "--output-directory":
                case "-o":
                    outputDirectory = args[++i];
                    break;
                case "--interval":
                case "-int":
                    if (!int.TryParse(args[++i], out interval))
                    {
                        Console.WriteLine("Interval must be a valid integer.");
                        return false;
                    }

                    break;
                case "--log-file":
                case "-l":
                    logFile = args[++i];
                    break;
            }
        }

        return true;
    }
}
