using System;
using System.IO;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using System.Collections.Immutable;

class Program
{
    // This method recursively processes the directory structure and runs FFmpeg on each file.
    static async void ProcessFilesWithFFmpeg(string sourceDir, string targetDir, List<string> loops)
    {
        // Create the target directory if it doesn't exist.
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // Get all files in the current directory.
        string[] files = Directory.GetFiles(sourceDir);
        foreach (var file in files)
        {
            // Get the file extension to check if it's a valid media file (for this example, video/audio).
            string fileExtension = Path.GetExtension(file).ToLower();

            // Check if the file extension is one that we want to process (e.g., .mp4, .mkv, .mp3).
            if (fileExtension == ".mp4" || fileExtension == ".mkv" || fileExtension == ".mp3")
            {
                // Construct the target file name (you can modify this if you want to change the file name or format).
                string newFilePath = Path.Combine(targetDir, Path.GetFileName(file));

                // Run FFmpeg command on the file.
                RunFFmpegCommand(file, newFilePath);
                //System.Threading.Thread.Sleep(4 * 60 * 1000);

                // Add ffmpeg loop

            }
        }

        //loops.Add("for %%a in (\"" + sourceDir + "\\*.mkv\") do ffmpeg -hwaccel cuda -i \"%%a\" -c:v h264_nvenc -pix_fmt yuv420p -preset fast -profile:v high -level:v 4.2 -crf 18 -c:a copy \"" + targetDir + "\\%%~na.mkv\"");


        // Recursively process all subdirectories.
        string[] subDirs = Directory.GetDirectories(sourceDir);
        foreach (var subDir in subDirs)
        {
            // Get the name of the subdirectory.
            string subDirName = Path.GetFileName(subDir);

            if (new string[] { "$RECYCLE.BIN" }.Contains(subDirName))
            {
                continue;
            }
            // Construct the path for the new subdirectory.
            string newSubDirPath = Path.Combine(targetDir, subDirName);

            // Recursively process files and directories.
            ProcessFilesWithFFmpeg(subDir, newSubDirPath, loops);
        }
    }



    // To fix the CS0120 error, you need to make the `RunFFmpegCommand` method static because it is being called from a static context.  
    // Update the method definition as follows:  

    static public void RunFFmpegCommand(string inputFile, string outputFile)
    {
        // FFmpeg command example: Convert a video to MP4 format.
        string ffmpegArgs = " -hwaccel cuda -i \"" + inputFile + "\" -c:v h264_nvenc -pix_fmt yuv420p -preset fast -profile:v high -level:v 4.2 -crf 18 -c:a copy  \"" + outputFile + "\"";
        Console.WriteLine($"Running cmd:{ffmpegArgs}");
        // Create the process to execute FFmpeg.
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg", // Make sure FFmpeg is installed and available in the system PATH.
            Arguments = ffmpegArgs,
            
        };

        try
        {
            // Start the FFmpeg process.
            Process process = new Process { StartInfo = processStartInfo };
            process.Start();

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing file {inputFile}: {ex.Message}");
        }
    }

    static void Main(string[] args)
    {
        bool runFFmpegConvert = false;
        bool runVideoCodecCheck = false;
        bool runVideoConversionNumbers = true;

        // Define the source directory (where the original files and directories are).
        string sourceDirectory = @"F:\\";

        // Define the target directory (where the processed files will be saved).
        string targetDirectory = @"G:\\";

        List<string> directoriesToSkip = new List<string>
        {
            @"$RECYCLE.BIN",
            @"System Volume Information"
        };


        if (runFFmpegConvert)
        {
            //foreach (var subDir in Directory.GetDirectories(sourceDirectory))
            //{
            //    string subDirName = Path.GetFileName(subDir);
            //    if (new string[] { "$RECYCLE.BIN" }.Contains(subDirName))
            //    {
            //        continue;
            //    }

            //}

            List<string> loops = new List<string>();

            // Start processing files with FFmpeg.
            try
            {
                ProcessFilesWithFFmpeg(sourceDirectory, targetDirectory, loops);
                System.IO.File.WriteAllLines("D:\\Erros.txt", loops);
                Console.WriteLine("Processing complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        if (runVideoCodecCheck)
        {

            List<string> loops = new List<string>();
            List<string> videoFolderList = new List<string>();

            // Start processing files with FFmpeg.
            try
            {
                videoFolderList = CheckVideoCodec(sourceDirectory, targetDirectory, videoFolderList);
                //System.IO.File.WriteAllLines("D:\\Erros.txt", loops);
                Console.WriteLine("Processing complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            
            System.IO.File.WriteAllLines("D:\\Output.txt", videoFolderList.ToArray());
        }
        if (runVideoConversionNumbers)
        {
            List<string> fileTypes = [".mp4", ".mkv", ".mp3"];
            try
            {
                CheckVideoConversionNumbers(sourceDirectory, targetDirectory, fileTypes, directoriesToSkip);
                Console.WriteLine("Processing complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking video conversion numbers: {ex.Message}");
            }
        }
    }

    static List<string> CheckVideoCodec(string sourceDir, string targetDir, List<string> videoFolderList)
    {

        // Get all files in the current directory.
        string[] files = Directory.GetFiles(sourceDir);
        foreach (var file in files)
        {
            string fileExtension = Path.GetExtension(file).ToLower();


            if (fileExtension == ".mp4" || fileExtension == ".mkv" || fileExtension == ".mp3")
            {
                string newFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                if (RunFFprobeCommand(file, newFilePath))
                {
                    videoFolderList.Add(sourceDir);
                    break;
                }
            }
        }

        string[] subDirs = Directory.GetDirectories(sourceDir);
        foreach (var subDir in subDirs)
        {
            string subDirName = Path.GetFileName(subDir);

            if (new string[] { "$RECYCLE.BIN" }.Contains(subDirName))
            {
                continue;
            }
            string newSubDirPath = Path.Combine(targetDir, subDirName);
            videoFolderList = CheckVideoCodec(subDir, newSubDirPath, videoFolderList);
        }
        return videoFolderList;
    }

    static public bool RunFFprobeCommand(string inputFile, string outputFile)
    {
        string ffmpegArgs = " -v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"" + inputFile + "\"";
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = "ffprobe", // Make sure FFmpeg is installed and available in the system PATH.
            Arguments = ffmpegArgs,
            RedirectStandardOutput = true,
        };

        try
        {
            // Start the FFmpeg process.
            Process process = new Process { StartInfo = processStartInfo };
            process.Start();

            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            if (process.StandardOutput.ToString() != null && !process.StandardOutput.ToString().Contains("264"))
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing file {inputFile}: {ex.Message}");
        }
        return false;
    }

    static void CheckVideoConversionNumbers(string sourceDir, string targetDir, List<string> fileTypes, List<string> directoriesToSkip)
    {
        // Input Validation
        foreach (string s in directoriesToSkip)
        {
            if (sourceDir.Contains(s))
            {
                return;
            }
        }
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }
        if (fileTypes == null)
        {
            Console.WriteLine("CheckVideoConversionNumbers: fileTypes is null");
            return;
        }

        // Get all files in the current directory.
        string[] srcFiles = Directory.GetFiles(sourceDir).Where(v => fileTypes.Contains(Path.GetExtension(v).ToLower())).ToArray();
        string[] targetFiles = Directory.GetFiles(sourceDir).Where(v => fileTypes.Contains(Path.GetExtension(v).ToLower())).ToArray();
        
        if (srcFiles.Length != targetFiles.Length)
        {
            Console.WriteLine("CheckVideoConversionNumbers: Loss of video files in folder " + sourceDir);
            return;
        }
        else
        {
            Console.WriteLine("Good folder: " + sourceDir);
        }

            // Recursively process all subdirectories.
            string[] subDirs = Directory.GetDirectories(sourceDir);
        foreach (var subDir in subDirs)
        {
            // Get the name of the subdirectory.
            string subDirName = Path.GetFileName(subDir);

            // Construct the path for the new subdirectory.
            string newSubDirPath = Path.Combine(targetDir, subDirName);

            // Recursively process files and directories.
            CheckVideoConversionNumbers(subDir, newSubDirPath, fileTypes, directoriesToSkip);
        }
    }
}
