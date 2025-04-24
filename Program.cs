using System;
using System.IO;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using System.Collections.Immutable;
using ffmpeg_Converter;

class Program
{
    static bool runFFmpegConvert = false;
    static bool runVideoCodecCheck = true;
    static bool runVideoConversionNumbers = false;

    // Define the source directory (where the original files and directories are).
    static string sourceDirectory = @"F:\\";

    // Define the target directory (where the processed files will be saved).
    static string targetDirectory = @"G:\\";

    static List<string> directoriesToSkip = new List<string>
        {
            @"$RECYCLE.BIN",
            @"System Volume Information"
        };


    static void Main(string[] args)
    {

        // Create FFmpegTools instance
        FFmpegTools ffmpegTools = new FFmpegTools(sourceDirectory = @"F:\\", targetDirectory = @"G:\\");





    }


    
}
