# Using Microsoft Playwright For Automated Collection in C# 

*A quick demo of how to do a looping, automated collection*

In my work as a software developer for a startup IT company, I use Puppeteer in almost all my work for collecting documents and data in looping scripts. I recently came upon Microsoft Playwright which promised that it eliminated certain difficulties which are a part of working in Puppeteer, namely 1) indetifying Iframes and shadow roots in the DOM in order to select and use CSS selectors that are children to them and 2) customizing timeouts for different situations involving waiting for items to download, selectors to be located, etc. I already had made a small demo project using Node.js in Visual Studio Code and wanted to see how Playwright would work in a C# and .NET environment.

I set up a project to download all of the archived shows in mp3 format from AllClassical's website, which is my local classical radio station. Because mp3s are larger files, I would normally have to set up a timeout of 30 to 60 seconds after clicking the download button before resuming the loop to collect the next show. This project tests the second claim about not needing timeouts. 

Playwright uses Webkit, Firefox, or Chromium browsers, I selected Chromium. I define page and browser with the option for headless and used the functio GotoAsync() to navigate to the url. Playwright uses the Locator() function similar to Puppeteer's QuerySelectorAsync() although no timeouts are necessary. Playwright differs from Puppeteer though in that if more than one of the selectors is found, it will throw an error instead of attaching to the first found. 

I created var showElements to loop through from the parent element that contained all the child elements with inner text for show names and descriptions and the href attribute that contains the download url. I created var showname and var description which use the TextContentAsync() function to scrape the inner text of the elements and save to string. I locate the id attritube to locate the element of the download link and use GetAttributeAsync("href") to save the href to a string. 

After defining the path and filename with finename and description variables, I use the custom function DownloadFileAsync() to directly download the file at the url. This function uses HttpClient from System.Net.Http to make a GET request. The response is written to file using Filestream from System.IO. Simulation of clicking is not needed and the download is asynchronous so the loop won't restart until the download is completed. 

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

The custom function SanitizeFileName() is called on the filename string to limit characters (some of the descriptions of shows are rather lengthy) and to eliminate illegal characters in Windows filenames. 

There were many other Playwright functions such as ClickAsync() that I wasn't able to use (I assumed there would be more pages to click through) but this was a good introduction to how Playwright works in C# for an automated collection and how I could start incorporating it into my daily work.
