using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using PlaywrightCsharp;
using System.Text.RegularExpressions;

//This program will use Playwright to navigate to AllClassical's website and download all the arhcived mp3s of their shows as well as
//name them based on show name and description and put them in a specified folder.
class Program
{
	static async Task Main(string[] args)
	{
		try
		{
			//use .NET 8.0
			//may have to install playwright and browsers in the terminal as well as through Nuget
			using var playwright = await Playwright.CreateAsync();
			//var firefox = playwright.Firefox;
			//var webkit = playwright.Webkit;
			var chromium = playwright.Chromium;

			//choose browser, choose headless
			var browser = await chromium.LaunchAsync(new() { Headless = false });
			var page = await browser.NewPageAsync();
			Console.WriteLine("browser launched");

			//go to webpage
			await page.GotoAsync("https://www.allclassical.org/programs/program-archive/");
			Console.WriteLine("page loaded");

			//List to store scraped data, see Shows.cs
			var shows = new List<Shows>();

			//select html elements in page that contain show name and description. Can use id's or css selectors
			var showElements = page.Locator("#archive-list > div");

			//loop through each element, am having to add breakpoint here or app will crash
			for (var index = 0; index < await showElements.CountAsync(); index++)
			{
				try
				{
					// get the current HTML element
					var showElement = showElements.Nth(index);

					// retrieve the name and description
					var showname = (await showElement.Locator("css=.font-circ-medium.uppercase").TextContentAsync())?.Trim();
					var description = (await showElement.Locator("css=.episode-description").TextContentAsync())?.Trim();

					var downloadButton = page.Locator($"#archive-list > div:nth-child({index + 1}) > div.p-6.episode-meat > div.playlist > div > a");
					//gets file name "X.mp3" through href linkn
					var html = await downloadButton.GetAttributeAsync("href");
					Console.WriteLine(html);

					//check if exists
					if (!string.IsNullOrEmpty(html))
					{
						var mp3Output = @"C:\Temp\Testing\Playwright Output\mp3s";
						var fileName = $"{showname}_{description}.mp3";
						fileName = SanitizeFileName(fileName);
						var filePath = Path.Combine(mp3Output, fileName);
						// Download the file asynchronously using HttpClient
						await DownloadFileAsync(html, filePath);
					}
					else
					{
						Console.WriteLine("No valid MP3 URL found");
					}

					//to click an element works same as Puppeteer, no need for Iframes or timeouts
					//var downloadButton = page.Locator($"#archive-list > div:nth-child({index+1}) > div.p-6.episode-meat > div.playlist > div > a");
					//await downloadButton.ClickAsync();
				}
				catch (Exception innerEx)
				{
					Console.WriteLine($"Error processing item {index}: {innerEx.Message}");
				}
			}
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
		}
	}
	//logic to download 
	private static async Task DownloadFileAsync(string url, string filePath)
	{
		using HttpClient client = new();
		var response = await client.GetAsync(url);

		if (response.IsSuccessStatusCode)
		{
			await using var fs = new FileStream(filePath, FileMode.Create);
			await response.Content.CopyToAsync(fs);
			Console.WriteLine($"File downloaded successfully: {filePath}");
		}
		else
		{
			Console.WriteLine($"Failed to download file. Status Code: {response.StatusCode}");
		}
	}
	//clean up filename
	private static string SanitizeFileName(string fileName, int maxLength = 100)
	{
		// Replace invalid filename characters with "_"
		fileName = Regex.Replace(fileName, @"[<>:""/\\|?*]", "_");

		// Trim the filename to the max length while keeping the file extension
		if (fileName.Length > maxLength)
		{
			string extension = Path.GetExtension(fileName);
			string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

			// Ensure the final filename (including extension) doesn't exceed the max length
			fileName = nameWithoutExtension.Substring(0, Math.Min(nameWithoutExtension.Length, maxLength - extension.Length)) + extension;
		}

		return fileName;
	}
}