using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuGetPackageLister
{
	internal class Program
	{
		// AllVersions flag is automatically set by the FindPackages method
		private const bool IncludePreReleasePackages = true;

		private const bool IncludeDelistedPackages = true;

		// Message constants
		private const string WelcomeText = "Welcome to nuget-package-lister!";

		private const string HorizontalRule = "====================================================================================================================";
		private const string ListItemDecorator = "\r\n * ";
		private const string PackageFeedSourceInstructionsText = "Please enter the URL of the NuGet package feed that you would like to access:";
		private const string NoPackageIdEnteredText = "No package id has been entered, all packages will be returned.";
		private const string SuccessfullyContactedServerMessageText = "Successfully contacted the server using the URL provided";
		private const string AttemptingToFindPackagesMessageText = "Attempting to find packages...";

		private const string SourceDescriptionBlurb =
			"The URL that you enter should be in the form of: 'https://<domain>.<gTLD>/nuget/' or\r\n'https://nuget.<domain>.<gTLD>/nuget/' without the quotes. Your URL\r\n" +
			"feed may have something different on the end like '/api/packages/'. to find the URL for your package feed visit the\r\n" +
			"base URL (without /nuget/) and it should be listed under the Repository URLs section";

		private const string RestartApplicationMessageText = "Please restart the application and try again.";

		private const string PackageFilterInstructionsText =
			"If you only wish to return results for a specific package, please enter the package id now. Otherwise, press enter.";

		// Message templates
		private const string NumberOfPackagesFoundMessateTemplate = "{0} package/s were found.";

		private const string ExceptionMessageTemplate = "A {0} was thrown\r\nThe message was: {1}\r\nStacktrace: {2}";
		private const string AttemptingToContactServerMessageTemplate = "Attempting to contact the server via '{0}'...";
		private const string InvalidFeedUrlMessageTemplate = "The provided package feed URL \r\n'{0}'\r\nis not valid.";
		private const string PackageIdSpecifiedMessageTemplate = "Only packages with an id that matches '{0}' will be returned.";
		private const string NoPackagesFoundAtFeedUrlMessageTemplate = "No packages were found at the following feed url:\r\n'{0}'";
		private const string OutputLogSummaryMessageTemplate = "A text and a csv file containing the results have been outputted to:\r\n'{0}'.";

		// Statics
		private static readonly List<string> _flagsToUse = new List<string> { "-AllVersions", "-IncludeDelisted", "-PreRelease" };

		private static readonly SemanticVersion minSemVer = new SemanticVersion("0.0.0");
		private static readonly VersionSpec versionSpec = new VersionSpec { MinVersion = minSemVer, IsMinInclusive = true };

		private static readonly string BlurbTextTemplate =
			"This tool will allow you to specify a NuGet package feed to view the packages for. It was built to support NuGet\r\n" +
			"Server 2.8\r\n\r\n" +
			"If you have a different version it may not work for you. It uses the `list` argument from \r\n" +
			"https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference#list with the following flags set: " +
			$"{ListItemDecorator}{string.Join(ListItemDecorator, _flagsToUse)}";

		public static void Main(string[] args)
		{
			PrintInitialBlurbMessage();
			PromptUserForFeedUrl();

			string nugetFeedUrl = WaitForUserPackageFeedUrlInput();

			Uri uriResult;
			bool validUrl = Uri.TryCreate(nugetFeedUrl, UriKind.Absolute, out uriResult);

			if (!validUrl)
			{
				PrintInvalidFeedUrlMessage(nugetFeedUrl);

				return;
			}

			// Update the value for the feed url to be all nice and proper
			nugetFeedUrl = uriResult.AbsoluteUri;

			try
			{
				PrintAttemptingToContactServerMessage(nugetFeedUrl);

				IPackageRepository packageRepo = PackageRepositoryFactory.Default.CreateRepository(nugetFeedUrl);

				PrintSuccessfullyContactedServerMessage();
				PromptUserForPackageIdFilter();

				string nugetPackageId = WaitForUserPackageIdInput();
				bool filterOnPackageId = !string.IsNullOrWhiteSpace(nugetPackageId);

				PrintPackageIdFilteringMessage(nugetPackageId);

				List<IPackage> foundPackages = filterOnPackageId
					? packageRepo.FindPackages(nugetPackageId, versionSpec, IncludePreReleasePackages, IncludeDelistedPackages).ToList()
					: packageRepo.GetPackages().ToList();

				if (!foundPackages.Any())
				{
					PrintNoFoundPackagesMessages(nugetFeedUrl);

					return;
				}

				List<string> foundPackageNamesWithVersionAppended = foundPackages.Select(p => $"{p.Id} {p.Version}").ToList();

				PrintFoundPackagesList(foundPackageNamesWithVersionAppended);
				PrintNumberOfPackagesFound(foundPackages);

				string currentDir = Directory.GetCurrentDirectory();
				string fileName = $"packages-list-at-{DateTime.UtcNow.ToString("yyyy-mm-ddTHH-MM-ssZ")}";
				string textFilePath = Path.Combine(currentDir, $"{fileName}.txt");
				string csvFilePath = Path.Combine(currentDir, $"{fileName}.csv");

				File.WriteAllText(textFilePath, string.Join("\r\n", foundPackageNamesWithVersionAppended));
				File.WriteAllText(csvFilePath, string.Join(",", foundPackageNamesWithVersionAppended));

				PrintLogOutputMessage(currentDir);
			}
			catch (Exception ex)
			{
				PrintExceptionDetailsMessage(ex);

				return;
			}

			Console.ReadLine();
		}

		private static void PrintEmptyLine()
		{
			Console.WriteLine();
		}

		private static void PrintHorizontalRule()
		{
			Console.WriteLine(HorizontalRule);
		}

		private static void PrintPaddedText(bool padBefore = true, bool padAfter = true, string textToPrint = HorizontalRule, PaddlingElement paddingElement = PaddlingElement.BlankLine)
		{
			if (padBefore && paddingElement == PaddlingElement.BlankLine)
				PrintEmptyLine();
			else if (padBefore && paddingElement == PaddlingElement.HorizontalRule)
				PrintHorizontalRule();

			Console.WriteLine(textToPrint);

			if (padAfter && paddingElement == PaddlingElement.BlankLine)
				PrintEmptyLine();
			else if (padAfter && paddingElement == PaddlingElement.HorizontalRule)
				PrintHorizontalRule();
		}

		private static void PrintTextFlankedByHorizontalRules(string textToPrint)
		{
			PrintHorizontalRule();
			Console.WriteLine(textToPrint);
			PrintHorizontalRule();
		}

		private static void PrintInitialBlurbMessage()
		{
			PrintPaddedText(textToPrint: WelcomeText, paddingElement: PaddlingElement.HorizontalRule);
			PrintPaddedText(textToPrint: BlurbTextTemplate, paddingElement: PaddlingElement.BlankLine);

			PrintHorizontalRule();
			PrintPaddedText(textToPrint: SourceDescriptionBlurb, paddingElement: PaddlingElement.BlankLine);
			PrintHorizontalRule();
		}

		private static void PromptUserForFeedUrl()
		{
			PrintEmptyLine();
			Console.WriteLine(PackageFeedSourceInstructionsText);
		}

		private static string WaitForUserPackageFeedUrlInput()
		{
			string nugetFeedUrl;

			do
			{
				nugetFeedUrl = Console.ReadLine().Trim();
			}
			while (string.IsNullOrWhiteSpace(nugetFeedUrl));

			PrintEmptyLine();

			return nugetFeedUrl;
		}

		private static void PrintInvalidFeedUrlMessage(string nugetFeedUrl)
		{
			Console.WriteLine($"{InvalidFeedUrlMessageTemplate} {RestartApplicationMessageText}", nugetFeedUrl);
			Console.ReadLine();
		}

		private static void PrintAttemptingToContactServerMessage(string nugetFeedUrl)
		{
			Console.WriteLine(AttemptingToContactServerMessageTemplate, nugetFeedUrl);
		}

		private static void PrintSuccessfullyContactedServerMessage()
		{
			Console.WriteLine(SuccessfullyContactedServerMessageText);
			PrintEmptyLine();
		}

		private static void PromptUserForPackageIdFilter()
		{
			Console.WriteLine(PackageFilterInstructionsText);
		}

		private static string WaitForUserPackageIdInput()
		{
			string nugetPackageId;

			do
			{
				nugetPackageId = Console.ReadLine().Trim();
			}
			while (Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Enter);

			return nugetPackageId;
		}

		private static void PrintPackageIdFilteringMessage(string nugetPackageId)
		{
			if (string.IsNullOrWhiteSpace(nugetPackageId))
			{
				Console.WriteLine(NoPackageIdEnteredText);
			}
			else
			{
				PrintEmptyLine();
				Console.WriteLine(PackageIdSpecifiedMessageTemplate, nugetPackageId);
			}

			PrintPaddedText();
			Console.WriteLine(AttemptingToFindPackagesMessageText);
		}

		private static void PrintNoFoundPackagesMessages(string nugetFeedUrl)
		{
			Console.WriteLine(NoPackagesFoundAtFeedUrlMessageTemplate, nugetFeedUrl);
			Console.ReadLine();
		}

		private static void PrintFoundPackagesList(List<string> foundPackageNamesWithVersionAppended)
		{
			Console.WriteLine($"{ListItemDecorator}{string.Join(ListItemDecorator, foundPackageNamesWithVersionAppended)}");
		}

		private static void PrintNumberOfPackagesFound(List<IPackage> foundPackages)
		{
			PrintPaddedText(textToPrint: foundPackages.Count.ToString(), paddingElement: PaddlingElement.HorizontalRule);
		}

		private static void PrintExceptionDetailsMessage(Exception ex)
		{
			PrintPaddedText();
			Console.WriteLine(ExceptionMessageTemplate, ex.GetType().Name, ex.Message, ex.StackTrace);
			Console.WriteLine(RestartApplicationMessageText);
			PrintPaddedText();
			Console.ReadLine();
		}

		private static void PrintLogOutputMessage(string currentDir)
		{
			Console.WriteLine(OutputLogSummaryMessageTemplate, currentDir);
			PrintHorizontalRule();
		}
	}

	public enum PaddlingElement
	{
		BlankLine = 0,
		HorizontalRule = 1
	}
}