var TARGET = Argument ("t", Argument ("target", "ci"));

// this is used for the NuGet diff as v4 is a major changes
var NUGET_OLD_VERSION = "3.14.4";

var NUGET_VERSION = "3.14.9";

var JAR_VERSION = "3.14.9";
var JAR_URL = $"https://repo1.maven.org/maven2/com/squareup/okhttp3/okhttp/{JAR_VERSION}/okhttp-{JAR_VERSION}.jar";

Task ("externals")
	.Does (() =>
{
	EnsureDirectoryExists ("./externals");
	
	DownloadFile(JAR_URL, "./externals/okhttp3.jar");

	// Update .csproj nuget versions
	XmlPoke("./source/Square.OkHttp3/Square.OkHttp3.csproj", "/Project/PropertyGroup/PackageVersion", NUGET_VERSION);
});

Task("nuget")
	.IsDependentOn("externals")
	.Does(() =>
{
	MSBuild ("./source/Square.OkHttp3.sln", c => {
		c.Configuration = "Release";
		c.Restore = true;
		c.MaxCpuCount = 0;
		c.Targets.Clear();
		c.Targets.Add("Pack");
		c.Properties.Add("PackageOutputPath", new [] { MakeAbsolute(new FilePath("./output")).FullPath });
		c.Properties.Add("PackageRequireLicenseAcceptance", new [] { "true" });
		c.Properties.Add("DesignTimeBuild", new [] { "false" });
	});

	System.IO.File.WriteAllText($"./output/Square.OkHttp3.{NUGET_VERSION}.nupkg.baseversion", NUGET_OLD_VERSION);
});

Task("samples")
	.IsDependentOn("nuget")
	.Does(() =>
{
	MSBuild ("./samples/OkHttp3Sample.sln", c => {
		c.Configuration = "Release";
		c.Restore = true;
		c.MaxCpuCount = 0;
		c.Properties.Add("DesignTimeBuild", new [] { "false" });
	});
});

Task("ci")
	.IsDependentOn("externals")
	.IsDependentOn("nuget")
	.IsDependentOn("samples");

Task ("clean")
	.Does (() =>
{
	if (DirectoryExists ("./externals/"))
		DeleteDirectory ("./externals", new DeleteDirectorySettings {
			Recursive = true,
			Force = true
		});
});

RunTarget (TARGET);
