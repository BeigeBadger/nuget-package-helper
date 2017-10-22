using ConsoleFormattingHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace NuGetPackageDeleter
{
	internal class Program
	{
		private static string _filePath;
		private static string _packageFeedUrl;
		private static string _apiKey;

		private static Uri _packageFeedUri => new Uri(_packageFeedUrl);
		private static string _packageFeedHostUrl => $"{_packageFeedUri.Scheme}://{_packageFeedUri.Host}";

		// Message constants
		private const string WelcomeMessageText = "Welcome to nuget-package-deleter!";

		private const string EnterFilePathPromptMessage = "Please enter the file path of the CSV file that contains the NuGet packages you would like to delete:";
		private const string EnterFeedSourceUrlPromptMessage = "Please enter the URL of the NuGet package feed that you would like to delete from:";
		private const string EnterApikKeyPromptMessage = "Please enter the API key to that authorises you to perform this action:";
		private const string UnableToContactServerMessageText = "Unable to contact the server using the URL provided";
		private const string SummaryMessageText = "Operation has completed, please see the log above for further information.";

		private const string BlurbMessageText =
			"This tool will allow you to specify NuGet packages, and their versions that you would like to remove from a\r\n" +
			"specified NuGet feed. It was built to support NuGet Server 2.8. If you have a different version it may not work\r\n" +
			"for you. It uses the `delete` command from the NuGet Command Line Interface.\r\n" +
			"See https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference#delete for more information.";

		private const string PackageListSourceBlurbMessageText = "You will need to enter the following pieces of information:\r\n" +
			"* The path to the CSV file that contains the names and versions of the NuGet packages you would like to remove.\r\n" +
			"* The URL of the NuGet feed that hosts the packages.\r\n" +
			"* The API key that authorises you to delete the packages.";

		// Message templates
		private const string InvalidFilePathMessageTemplate = "The provided file path \r\n'{0}'\r\nis not valid.\r\n\r\n" +
			"Press enter to continue.";

		// Command template
		private const string DeleteCommandTemplate = "/K nuget.exe delete {0} -Source {1} -ApiKey {2} -NonInteractive";

		private static void Main(string[] args)
		{
			ConsoleHelper.PrintInitialBlurbMessage(WelcomeMessageText, BlurbMessageText, PackageListSourceBlurbMessageText);

			GetFilePath();
			GetPackageFeedUrl();
			GetApiKey();

			List<string> itemsToDelete = GetPackagesFromFile();

			using (Process cmdProcess = new Process())
			{
				foreach (string package in itemsToDelete)
				{
					string argsString = string.Format(DeleteCommandTemplate, package, _packageFeedHostUrl, _apiKey);
					ProcessStartInfo startInfo = new ProcessStartInfo
					{
						WindowStyle = ProcessWindowStyle.Normal,
						FileName = "cmd.exe",
						UseShellExecute = false,
						Arguments = argsString
					};

					cmdProcess.StartInfo = startInfo;
					cmdProcess.Start();
					cmdProcess.WaitForExit();
				}
			}

			ConsoleHelper.PrintEmptyLine();
			ConsoleHelper.PrintTextSurroundedByHorizontalRules(SummaryMessageText);

			Console.ReadLine();
		}

		private static void GetFilePath()
		{
			ConsoleHelper.PromptUserForInput(EnterFilePathPromptMessage);

			_filePath = ConsoleHelper.WaitForUserInput();
			bool validPath = File.Exists(_filePath);

			PromptUserToFixBadInput(InvalidFilePathMessageTemplate, EnterFilePathPromptMessage, validPath, () => File.Exists(_filePath), _filePath);
		}

		private static void GetPackageFeedUrl()
		{
			ConsoleHelper.PromptUserForInput(EnterFeedSourceUrlPromptMessage);

			_packageFeedUrl = ConsoleHelper.WaitForUserInput();
			bool validFeed = Uri.IsWellFormedUriString(_packageFeedUrl, UriKind.Absolute);

			PromptUserToFixBadInput(ConsoleHelper.InvalidFeedUrlMessageTemplate, EnterFeedSourceUrlPromptMessage, validFeed, () => Uri.IsWellFormedUriString(_packageFeedUrl, UriKind.Absolute), _packageFeedUrl);

			ConsoleHelper.PrintText(ConsoleHelper.AttemptingToContactServerMessageTemplate, _packageFeedUrl);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_packageFeedUrl);
			request.AllowAutoRedirect = false;
			request.Method = WebRequestMethods.Http.Head;

			try
			{
				HttpWebResponse result = (HttpWebResponse)request.GetResponse();

				if (result.StatusCode != HttpStatusCode.NotFound)
					ConsoleHelper.PrintText(ConsoleHelper.SuccessfullyContactedServerMessageText);
				else
					ConsoleHelper.PrintErrorMessageThenHalt(UnableToContactServerMessageText);
			}
			catch (Exception ex)
			{
				ConsoleHelper.PrintErrorMessageThenHalt($"{ConsoleHelper.ExceptionMessageTemplate} {ConsoleHelper.RestartApplicationMessageText}", ex.GetType().Name, ex.Message, ex.StackTrace);
			}
		}

		private static void GetApiKey()
		{
			ConsoleHelper.PromptUserForInput(EnterApikKeyPromptMessage);
			_apiKey = ConsoleHelper.WaitForUserInput();
		}

		private static void PromptUserToFixBadInput(string errorMessageTemplate, string inputPromptMessage, bool validInput, Func<bool> validationFunc, string inputString)
		{
			while (!validInput)
			{
				ConsoleHelper.PrintErrorMessageThenHalt($"{errorMessageTemplate}", inputString);
				ConsoleHelper.PromptUserForInput(inputPromptMessage);

				inputString = ConsoleHelper.WaitForUserInput();
				validInput = validationFunc();
			}
		}

		private static List<string> GetPackagesFromFile()
		{
			List<string> itemsToDelete = new List<string>();

			using (StreamReader streamReader = new StreamReader(_filePath))
			{
				string currentLine;

				while ((currentLine = streamReader.ReadLine()) != null)
				{
					itemsToDelete.AddRange(currentLine.Split(','));
				}

				ConsoleHelper.PrintTextSurroundedByHorizontalRules(ConsoleHelper.NumberOfPackagesFoundMessageTemplate, itemsToDelete.Count.ToString());
			}

			return itemsToDelete;
		}
	}
}