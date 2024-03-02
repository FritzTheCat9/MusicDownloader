using System.Diagnostics;

public class YoutubeDownloader
{
    public static void DownloadAudio(string videoUrl, string outputPath)
    {
        string arguments = $"-x --audio-format mp3 --audio-quality 0 --output \"{outputPath}/%(title)s.%(ext)s\" \"{videoUrl}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "yt-dlp",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) =>
            {
                Console.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                Console.WriteLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}
