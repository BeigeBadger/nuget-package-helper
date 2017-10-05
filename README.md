# nuget-package-lister
Lists the packages that are provided by a specified endpoint, supports NuGet Server 2.X.X. Built in Visual Studio 2017 using the `NuGet.Core` [library](https://github.com/NuGet/NuGet2).

### Repo Status
#### Overall
[![Build status](https://ci.appveyor.com/api/projects/status/w7q53cjml7k0t003?svg=true&retina=true)](https://ci.appveyor.com/project/BeigeBadger/nuget-package-lister)

#### Master
[![Build status](https://ci.appveyor.com/api/projects/status/w7q53cjml7k0t003/branch/master?svg=true&retina=true)](https://ci.appveyor.com/project/BeigeBadger/nuget-package-lister)

## How to use

1. Build the solution.
2. Find `NuGetPackageLister.exe` in the bin/ folder.
3. Run `NuGetPackageLister.exe`.
4. Pass in the NuGet package feed URL that you would like to query e.g. `https://<domain>.<gTLD>/nuget/` or 
`https://nuget.<domain>.<gTLD>/nuget/` - without the quotes.
 5. Optionally specify a `PackageId` to restrict the results to.
 6. A `.txt` and `.csv` file containing the results will be outputted to the current working directory.
