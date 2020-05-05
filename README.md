# **Cloud.Core.Web**  
[![Build status](https://dev.azure.com/cloudcoreproject/CloudCore/_apis/build/status/Cloud.Core%20Packages/Cloud.Core.Web_Package)](https://dev.azure.com/cloudcoreproject/CloudCore/_build/latest?definitionId=8) ![Code Coverage](https://cloud1core.blob.core.windows.net/codecoveragebadges/Cloud.Core.Web-LineCoverage.png) [![Cloud.Core.Web package in Cloud.Core feed in Azure Artifacts](https://feeds.dev.azure.com/cloudcoreproject/dfc5e3d0-a562-46fe-8070-7901ac8e64a0/_apis/public/Packaging/Feeds/8949198b-5c74-42af-9d30-e8c462acada6/Packages/c33811e9-eb52-49cd-ae98-dc77a8e80a0e/Badge)](https://dev.azure.com/cloudcoreproject/CloudCore/_packaging?_a=package&feed=8949198b-5c74-42af-9d30-e8c462acada6&package=c33811e9-eb52-49cd-ae98-dc77a8e80a0e&preferRelease=true)



<div id="description">
AspNetCore web extensions to be used in web based projects, such as MVC web sites and API's.  Adds additional features such as common error formatting,
custom middleware and action filters/results.
</div>

## Design
The attribute for Validation (Validate) mentioned below is an example of AOP, where an attribute can be added to controller methods to perform some code before they are executed.  This is known as the 
_Aspect Orientated Programming_ design pattern, as covered in the link:  

- *AOP* - https://www.dotnetcurry.com/patterns-practices/1305/aspect-oriented-programming-aop-csharp-using-solid


## Usage

### Middleware

Unhandled exceptions in the Api code will now be handled with custom middleware that will be added to the MVC middleware pipeline.  It will automatically
return a 500 error with the exception output in the response body (`InternalServerErrorResult`). 

Configure the middleware as follows:

```csharp
// sample startup.cs

private readonly double[] _appVersions = { 1.0 };
private readonly string[] _supportedCultures = { "en" };
	
// Configure services...
public void ConfigureServices(IServiceCollection services)
{
    ...
    
    // Add swagger with version support.
    services.AddSwaggerWithVersions(_appVersions, c => c.IncludeXmlComments("Cloud.App.SampleWebAPI.xml"));

    // Add string translations (localizations) support.
    services.AddLocalization(o =>
    {
        // Translations exist in Resources folder.
        o.ResourcesPath = "Resources";  // Path to language resource files.
    });

    ...
}

// In startup.cs wire up in Configure method...
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    loggerFactory.AddDebug();

    app.UseUnhandledExceptionMiddleware(); // Add custom middleware before UseMvc.

    app.UseSwaggerWithVersion(_appVersions, swaggerJsonPathPrefix);  // Add swagger along with versions.
	    
    app.UseLocalization({ "en", "fr", "tl" });  // Add translations.
	
    app.UseMvc();
}
```

### Custom Attributes

**Validation**
A new attribute is available for automatically generating validation failed responses (400 status) with response body of ApiException built using
model state dictionary.  The validation attribute `[Validate]` can be attached to a controller for ALL methods or attached to individual methods, as shown below.

Attach to controller:

```csharp
[Validate]
[Route("api/[controller]")]
public class AccountController : Controller
{
    // Controllers here...
}
```

Attach to controller method:

```csharp
[Validate]
[HttpPost]
public IActionResult Create(RequestModel model)
{
    // Won't reach here if fails validation due to the "Validate" attribute.
}
```

### Custom Action Results

**ValidationFailedResult**

Use this result when manually responding with validation failed result (consider using attribute above instead).

```csharp
[HttpPost]
public IActionResult Post(RequestModel model)
{
    // Validation using model state...
    if (ModelState.IsValid == false) 
    {
        // Returns 400 status with ApiErrorResult as response body.
        return ValidationFailedResult(ModelState);
    }
    
    // Custom validation check...
    if (model.SomeProp == false)
    {
        // Add localised error, 400 returned with ApiErrorResult as body.
        ModelState.AddModelError(_localizer["SomeErr.Title"], _localizer["SomeErr.Reason"]);
	return BadRequest(new ApiErrorResult(ModelState));
    }
    
    // Continue normal execution when ModelState is valid...
}
```

**InternalServerErrorResult**

Use this result when manually responding with internal server error (consider using general exception middleware above instead).

```csharp
[HttpPost]
public IActionResult Post(RequestModel model)
{
    try
    {
        throw new Exception("Oops something's gone wrong!");
    }
    catch(Exception ex)
    {
        // Returns 500 status with ApiErrorResult as response body.
        return InternalServerErrorResult(ex);
    }
	
    // Never reaches here on exception...
}
```


## Test Coverage
A threshold will be added to this package to ensure the test coverage is above 80% for branches, functions and lines.  If it's not above the required threshold 
(threshold that will be implemented on ALL of the core repositories to gurantee a satisfactory level of testing), then the build will fail.

## Compatibility
This package has has been written in .net Standard and can be therefore be referenced from a .net Core or .net Framework application. The advantage of utilising from a .net Core application, 
is that it can be deployed and run on a number of host operating systems, such as Windows, Linux or OSX.  Unlike referencing from the a .net Framework application, which can only run on 
Windows (or Linux using Mono).
 
## Setup
This package is built using .net Standard 2.1 and requires the .net Core 3.1 SDK, it can be downloaded here: 
https://www.microsoft.com/net/download/dotnet-core/

IDE of Visual Studio or Visual Studio Code, can be downloaded here:
https://visualstudio.microsoft.com/downloads/

## How to access this package
All of the Cloud.Core.* packages are published to a public NuGet feed.  To consume this on your local development machine, please add the following feed to your feed sources in Visual Studio:
https://dev.azure.com/cloudcoreproject/CloudCore/_packaging?_a=feed&feed=Cloud.Core
 
For help setting up, follow this article: https://docs.microsoft.com/en-us/vsts/package/nuget/consume?view=vsts


<img src="https://cloud1core.blob.core.windows.net/icons/cloud_core_small.PNG" />
