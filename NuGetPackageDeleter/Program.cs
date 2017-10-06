using ConsoleFormattingHelper;
using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGetPackageDeleter
{
	internal class Program
	{
		// AllVersions flag is automatically set by the FindPackages method
		private const bool IncludePreReleasePackages = true;

		private const bool IncludeDelistedPackages = true;

		private static readonly SemanticVersion minSemVer = new SemanticVersion("0.0.0");

		private static readonly VersionSpec versionSpec = new VersionSpec { MinVersion = minSemVer, IsMinInclusive = true };

		// Message constants
		private const string WelcomeMessageText = "Welcome to nuget-package-deleter!";

		private const string BlurbMessageText =
			"This tool will allow you to specify NuGet packages, and their versions that you would like to remove from a\r\n" +
			"specified NuGet feed. It was built to support NuGet Server 2.8. If you have a different version it may not work\r\n" +
			"for you. It uses the `RemovePackage` command from the NuGet.Core package.\r\n" +
			"See https://github.com/NuGet/NuGet2 for more information.";

		private const string PackageListSourceBlurbMessageText = "";

		// Message templates

		// RemovePackage is not supported by IPackageRepository =\
		private static void Main(string[] args)
		{
			// TODO: Print instructions
			//ConsoleHelper.PrintInitialBlurbMessage(WelcomeMessageText, BlurbMessageText, PackageListSourceBlurbMessageText);

			//string nugetFeedUrl = "";
			//string nugetPackageId = "";

			//IPackageRepository packageRepo = PackageRepositoryFactory.Default.CreateRepository(nugetFeedUrl);

			//List<IPackage> packages = packageRepo.FindPackages(nugetPackageId, versionSpec, IncludePreReleasePackages, IncludeDelistedPackages).ToList();

			//foreach (IPackage package in packages)
			//{
			//	packageRepo.RemovePackage(package);
			//}

			// TODO: Ask for input file, feed url, and apiKey

			// TODO: Validate and summarise input file

			// TODO: Loop through entries and keep overwall total

			// TODO: Print results

			//Console.ReadLine();
		}
	}
}