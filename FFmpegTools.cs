using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ffmpeg_Converter
{
     class FFmpegTools
    {
        #region Properties
        // Define the source directory (where the original files and directories are).
        public string sourceDirectory;

        // Define the target directory (where the processed files will be saved).
        public string targetDirectory;

        public List<string> directoriesToSkip = new List<string>
        {
            @"$RECYCLE.BIN",
            @"System Volume Information"
        };

        public List<string> fileTypesToConvert = new List<string>
        {
            ".mp4",
            ".mkv",
            ".mp3"
        };
        public string standardOutputFile;
        public string errorOutputFile;
        public string resultFile;

        #endregion Properties

        #region Constructor
        public FFmpegTools(string sourceDirectory, string targetDirectory)
        {
            this.sourceDirectory = sourceDirectory;
            this.targetDirectory = targetDirectory + "\\ConvertedMedia";
            this.standardOutputFile = Path.Combine(targetDirectory, "ffmpeg_output.txt");
            this.errorOutputFile = Path.Combine(targetDirectory, "ffmpeg_error.txt");
            this.resultFile = Path.Combine(targetDirectory, "ffmpeg_result.txt");
        }
        #endregion Constructor


        public void runFFmpegConvert()
        {
            try
            {
                ProcessFilesWithFFmpeg(sourceDirectory, targetDirectory);
                Console.WriteLine("Processing complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while running FFmpeg Convert: {ex.Message}");
            }
        }

        // This method recursively processes the directory structure and runs FFmpeg on each file.
        private void ProcessFilesWithFFmpeg(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Get all files in the current directory.
            string[] files = Directory.GetFiles(sourceDir);
            foreach (var file in files)
            {
                string fileExtension = Path.GetExtension(file).ToLower();
                if (fileTypesToConvert.Contains(fileExtension))
                {
                    string newFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                    RunFFmpegCommand(file, newFilePath);
                }
                else
                {
                    Console.WriteLine();
                }
            }

            // Recursively process all subdirectories.
            string[] subDirs = Directory.GetDirectories(sourceDir);
            foreach (var subDir in subDirs)
            {
                string subDirName = Path.GetFileName(subDir);
                if (directoriesToSkip.Contains(subDirName))
                {
                    continue;
                }
                string newSubDirPath = Path.Combine(targetDir, subDirName);
                ProcessFilesWithFFmpeg(subDir, newSubDirPath);
            }
        }

        private void RunFFmpegCommand(string inputFile, string outputFile)
        {
            string ffmpegArgs = " -hwaccel cuda -i \"" + inputFile + "\" -c:v h264_nvenc -pix_fmt yuv420p -preset fast -profile:v high -level:v 4.2 -crf 18 -c:a copy  \"" + outputFile + "\"";
            Console.WriteLine($"Running cmd:{ffmpegArgs}");
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg", // Make sure FFmpeg is installed and available in the system PATH.
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                Process process = new Process { StartInfo = processStartInfo };
                process.Start();

                process.WaitForExit();
                File.WriteAllText(standardOutputFile, process.StandardOutput.ReadToEnd() + "\n\n");
                File.WriteAllText(errorOutputFile, process.StandardError.ReadToEnd() + "\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing file {inputFile}: {ex.Message}");
            }
        }

        public void runFFprobe()
        {
            try
            {
                CheckVideoCodec(sourceDirectory, targetDirectory);
                Console.WriteLine("Processing complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void CheckVideoCodec(string sourceDir, string targetDir)
        {
            // Get all files in the current directory.
            string[] files = Directory.GetFiles(sourceDir);
            foreach (var file in files)
            {
                string fileExtension = Path.GetExtension(file).ToLower();
                if (fileTypesToConvert.Contains(fileExtension))
                {
                    string newFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                    RunFFprobeCommand(file, newFilePath);
                }
            }

            string[] subDirs = Directory.GetDirectories(sourceDir);
            foreach (var subDir in subDirs)
            {
                string subDirName = Path.GetFileName(subDir);

                if (directoriesToSkip.Contains(subDirName))
                {
                    continue;
                }
                string newSubDirPath = Path.Combine(targetDir, subDirName);
                CheckVideoCodec(subDir, newSubDirPath);
            }
        }

        private bool RunFFprobeCommand(string inputFile, string outputFile)
        {
            string ffmpegArgs = " -v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"" + inputFile + "\"";
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "ffprobe", 
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                // Start the FFmpeg process.
                Process process = new Process { StartInfo = processStartInfo };
                process.Start();

                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                File.WriteAllText(standardOutputFile, output + "\n\n");
                File.WriteAllText(errorOutputFile, error + "\n\n");
                File.WriteAllLines(resultFile, new[] { output + "\t" + inputFile});
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking video codec file {inputFile}: {ex.Message}");
            }
            return false;
        }

        public void RunVideoConversionNumbers(string sourceDir, string targetDir)
        {
            try
            {
                CheckVideoConversionNumbers(sourceDir, targetDir);
                Console.WriteLine("Processing complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void CheckVideoConversionNumbers(string sourceDir, string targetDir)
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
            if (fileTypesToConvert == null)
            {
                Console.WriteLine("CheckVideoConversionNumbers: fileTypes is null");
                return;
            }

            // Get all files in the current directory.
            string[] srcFiles = Directory.GetFiles(sourceDir).Where(v => fileTypesToConvert.Contains(Path.GetExtension(v).ToLower())).ToArray();
            string[] targetFiles = Directory.GetFiles(targetDir).Where(v => fileTypesToConvert.Contains(Path.GetExtension(v).ToLower())).ToArray();

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
                CheckVideoConversionNumbers(subDir, newSubDirPath);
            }
        }

    }
}
