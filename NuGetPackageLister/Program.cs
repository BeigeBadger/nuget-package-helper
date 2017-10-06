using ConsoleFormattingHelper;
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
		private const string WelcomeMessageText = "Welcome to nuget-package-lister!";

		private const string EnterPackageFeedUrlPromptMessageText = "Please enter the URL of the NuGet package feed that you would like to access:";
		private const string NoPackageIdEnteredMessageText = "No package id has been entered, all packages will be returned.";
		private const string SuccessfullyContactedServerMessageText = "Successfully contacted the server using the URL provided";
		private const string AttemptingToFindPackagesMessageText = "Attempting to find packages...";

		private static readonly string InitialToolBlurbMessageText =
			"This tool will allow you to specify a NuGet package feed to view the packages for. It was built to support NuGet\r\n" +
			"Server 2.8, If you have a different version it may not work for you.\r\n\r\n" +
			"It uses the following commands from the NuGet.Core package: `FindPackages` with the AllVersions, IncludeDelisted, \r\n" +
			"and AllowPreRelease flags set if you specify a package; otherwise it will use the `GetPackages` command.\r\n" +
			"See https://github.com/NuGet/NuGet2 for more information.";

		private const string FeedSourceDescriptionBlurbMessageText =
			"The URL that you enter should be in the form of: 'https://<domain>.<gTLD>/nuget/' or\r\n" +
			"'https://nuget.<domain>.<gTLD>/nuget/' without the quotes. Your URL feed may have something different on the end\r\n" +
			"like '/api/packages/'. to find the URL for your package feed visit the base URL (without /nuget/) and it should be\r\n" +
			"listed under the Repository URLs section";

		private const string EnterPackageIdFilterPromptMessageText =
			"If you only wish to return results for a specific package, please enter the package id now. Otherwise, press enter.";

		// Message templates
		private const string NumberOfPackagesFoundMessageTemplate = "{0} package/s were found.";

		private const string ExceptionMessageTemplate = "A {0} was thrown\r\nThe message was: {1}\r\nStacktrace: {2}";
		private const string AttemptingToContactServerMessageTemplate = "Attempting to contact the server via '{0}'...";
		private const string InvalidFeedUrlMessageTemplate = "The provided package feed URL \r\n'{0}'\r\nis not valid.";
		private const string PackageIdSpecifiedMessageTemplate = "Only packages with an id that matches '{0}' will be returned.";
		private const string NoPackagesFoundAtFeedUrlMessageTemplate = "No packages were found at the following feed url:\r\n'{0}'";
		private const string OutputLogSummaryMessageTemplate = "A text and a csv file containing the results have been outputted to:\r\n'{0}'.";

		// Statics
		private static readonly SemanticVersion minSemVer = new SemanticVersion("0.0.0");

		private static readonly VersionSpec versionSpec = new VersionSpec { MinVersion = minSemVer, IsMinInclusive = true };

		public static void Main(string[] args)
		{
			ConsoleHelper.PrintInitialBlurbMessage(WelcomeMessageText, InitialToolBlurbMessageText, FeedSourceDescriptionBlurbMessageText);
			ConsoleHelper.PromptUserForInput(EnterPackageFeedUrlPromptMessageText);

			string nugetFeedUrl = ConsoleHelper.WaitForUserPackageFeedUrlInput();

			Uri uriResult;
			bool validUrl = Uri.TryCreate(nugetFeedUrl, UriKind.Absolute, out uriResult);

			if (!validUrl)
			{
				ConsoleHelper.PrintErrorMessageThenHalt($"{InvalidFeedUrlMessageTemplate} {ConsoleHelper.RestartApplicationMessageText}", nugetFeedUrl);

				return;
			}

			// Update the value for the feed url to be all nice and proper
			nugetFeedUrl = uriResult.AbsoluteUri;

			try
			{
				ConsoleHelper.PrintText(AttemptingToContactServerMessageTemplate, nugetFeedUrl);

				IPackageRepository packageRepo = PackageRepositoryFactory.Default.CreateRepository(nugetFeedUrl);

				ConsoleHelper.PrintText(SuccessfullyContactedServerMessageText);
				ConsoleHelper.PromptUserForInput(EnterPackageIdFilterPromptMessageText);

				string nugetPackageId = ConsoleHelper.WaitForUserPackageIdInput();
				bool filterOnPackageId = !string.IsNullOrWhiteSpace(nugetPackageId);

				PrintPackageIdFilteringMessage(nugetPackageId);

				List<IPackage> foundPackages = filterOnPackageId
					? packageRepo.FindPackages(nugetPackageId, versionSpec, IncludePreReleasePackages, IncludeDelistedPackages).ToList()
					: packageRepo.GetPackages().ToList();

				if (!foundPackages.Any())
				{
					ConsoleHelper.PrintErrorMessageThenHalt(NoPackagesFoundAtFeedUrlMessageTemplate, nugetFeedUrl);

					return;
				}

				List<string> foundPackageNamesWithVersionAppended = foundPackages.Select(p => $"{p.Id} {p.Version}").ToList();

				ConsoleHelper.PrintList(foundPackageNamesWithVersionAppended, ConsoleHelper.ListItemDecorator);
				ConsoleHelper.PrintTextSurroundedByHorizontalRules(NumberOfPackagesFoundMessageTemplate, foundPackages.Count.ToString());

				string currentDir = Directory.GetCurrentDirectory();

				string fileName = BuildFileName(nugetPackageId);
				string textFilePath = Path.Combine(currentDir, $"{fileName}.txt");
				string csvFilePath = Path.Combine(currentDir, $"{fileName}.csv");

				File.WriteAllText(textFilePath, string.Join("\r\n", foundPackageNamesWithVersionAppended));
				File.WriteAllText(csvFilePath, string.Join(",", foundPackageNamesWithVersionAppended));

				ConsoleHelper.PrintTextFollowedByHorizontalRule(OutputLogSummaryMessageTemplate, currentDir);
			}
			catch (Exception ex)
			{
				ConsoleHelper.PrintErrorMessageThenHalt($"{ExceptionMessageTemplate} {ConsoleHelper.RestartApplicationMessageText}", ex.GetType().Name, ex.Message, ex.StackTrace); ;

				return;
			}

			Console.ReadLine();
		}

		private static string BuildFileName(string nugetPackageId)
		{
			string basePart = "packages-list";
			string packageIdPart = "";
			string utcTimeStampPart = DateTime.UtcNow.ToString("yyyy-mm-ddTHH-MM-ssZ");

			// Include the package id if its for a specify package
			if (!string.IsNullOrWhiteSpace(nugetPackageId))
				packageIdPart = $"-for-{nugetPackageId}";

			return $"{basePart}{packageIdPart}-at-{utcTimeStampPart}";
		}

		private static void PrintPackageIdFilteringMessage(string nugetPackageId)
		{
			if (string.IsNullOrWhiteSpace(nugetPackageId))
			{
				ConsoleHelper.PrintText(NoPackageIdEnteredMessageText);
			}
			else
			{
				ConsoleHelper.PrintEmptyLine();
				ConsoleHelper.PrintText(PackageIdSpecifiedMessageTemplate, nugetPackageId);
			}

			ConsoleHelper.PrintPaddedText();
			ConsoleHelper.PrintText(AttemptingToFindPackagesMessageText);
		}
	}
}