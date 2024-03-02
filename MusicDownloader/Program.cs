using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;

YoutubeDownloader.DownloadAudio("https://www.youtube.com/watch?v=MdF72GqEJkI", "../../../Downloaded");

Console.ReadKey();

void DownloadLinksWithWebsite()
{
    var website = "https://ytmp3.nu/29M3/";
    var inputId = "url";
    var searchButtonSelector = "/html/body/form/div[2]/input[2]";
    var downloadButtonSelector = "/html/body/form/div[2]/a[1]";

    var downloadsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";

    Log.Logger = new LoggerConfiguration()
        .WriteTo.File("../../../logs.txt")
        .WriteTo.Console()
        .MinimumLevel.Information()
        .CreateLogger();

    Log.Information("Music Downloader");

    var links = GetLinksFromFile($"links.txt");
    var notDownloadedLinks = new List<string>();
    var downloadedSongsFolder = "../../../Downloaded";
    var fullDriverPath = Path.GetFullPath("../../../chromedriver.exe");
    var errorLinksFile = "errorLinks.txt";

    var options = new ChromeOptions();
    options.AddArgument("--headless");

    using (var driver = new ChromeDriver(fullDriverPath))
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        foreach (var link in links)
        {
            try
            {
                driver.Navigate()
                      .GoToUrl(website);

                var textField = driver.FindElement(By.Id(inputId));
                if (textField.Displayed)
                    textField.SendKeys(link);

                var searchButton = driver.FindElement(By.XPath(searchButtonSelector));
                if (searchButton.Displayed)
                    searchButton.Click();

                var downloadButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(downloadButtonSelector)));
                if (downloadButton.Displayed)
                    downloadButton.Click();

                Thread.Sleep(15000);

                var targetDirectory = new DirectoryInfo(downloadsFolderPath);
                var files = targetDirectory.GetFiles();

                files = files.Where(file => !file.Attributes.HasFlag(FileAttributes.Directory)).ToArray();

                if (files.Length > 0)
                {
                    var newestFile = files.OrderByDescending(file => file.CreationTime)
                                          .FirstOrDefault();

                    if (newestFile is not null && newestFile.CreationTime >= DateTime.UtcNow.AddMinutes(-1))
                    {
                        Log.Information($"Copying downloaded file: {newestFile.Name}");
                        newestFile.CopyTo($"{downloadedSongsFolder}/{newestFile.Name}", true);

                        Log.Information($"Deleting downloaded file: {newestFile.Name}");
                        newestFile.Delete();

                        Log.Information($"Successfully deleted file: {newestFile.Name}");
                    }
                    else
                    {
                        Log.Error($"No song downloaded");
                        notDownloadedLinks.Add(link);
                        continue;
                    }
                }
                else
                {
                    Log.Error($"No song downloaded");
                    notDownloadedLinks.Add(link);
                    continue;
                }

                Log.Information($"Successfully downloaded: {link}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to download: {link}");
                Log.Error($"Exception: {ex.Message}");
                notDownloadedLinks.Add(link);
            }
        }

        try
        {
            File.WriteAllLines($"../../../{errorLinksFile}", notDownloadedLinks);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while writing to file: {ex.Message}");
        }

        Console.ReadKey();
    }
}

static List<string> GetLinksFromFile(string file)
{
    Log.Information($"Reading file: {file}");

    var links = new List<string>();

    try
    {
        var lines = File.ReadAllLines($"../../../{file}");
        links.AddRange(lines);
    }
    catch (Exception ex)
    {
        Log.Error($"An error occurred while reading the file: {ex.Message}");
    }

    Log.Information($"Links readed from file:");

    foreach (var link in links)
    {
        Log.Information(link);
    }

    return links;
}