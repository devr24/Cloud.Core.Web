# **Cloud.Core.Web**  

AspNetCore web extensions to be used in web based projects, such as MVC web sites and API's.  Adds additional features such as common error formatting,
custom middleware and action filters/results.

## Design
The attribute for Validation (Validate) mentioned below is an example of AOP, where an attribute can be added to controller methods to perform some code before they are executed.  This is known as the 
_Aspect Orientated Programming_ design pattern, as covered in the link:  

- *AOP* - https://www.dotnetcurry.com/patterns-practices/1305/aspect-oriented-programming-aop-csharp-using-solid

## Usage

### Middleware

Unhandled exceptions in the Api code will now be handled with custom middleware that will be added to the MVC middleware pipeline.  It will automatically
return a 500 error with the exception output in the response body (`InternalServerErrorResult`). 

Configure the middleware as follows:

```
// In startup.cs wire up in Configure method...
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    loggerFactory.AddDebug();

    app.UseUnhandledExceptionMiddleware(); // Add custom middleware before UseMvc.

    app.UseMvc();
}
```

### Custom Attributes

**Validation**
A new attribute is available for automatically generating validation failed responses (400 status) with response body of ApiException built using
model state dictionary.  The validation attribute `[Validate]` can be attached to a controller for ALL methods or attached to individual methods, as shown below.

Attach to controller:

```
[Validate]
[Route("api/[controller]")]
public class AccountController : Controller
{
    // Controllers here...
}
```

Attach to controller method:

```
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

```
[HttpPost]
public IActionResult Post(RequestModel model)
{
    if (ModelState.IsValid == false) 
	{
        // Returns 400 status with ApiError as response body.
        return ValidationFailedResult(ModelState);
    }
    // Continue normal execution when ModelState is valid...
}
```

**InternalServerErrorResult**

Use this result when manually responding with internal server error (consider using general exception middleware above instead).

```
[HttpPost]
public IActionResult Post(RequestModel model)
{
    try
    {
        throw new Exception("Oops something's gone wrong!");
    }
    catch(Exception ex)
    {
        // Returns 500 status with ApiError as response body.
        return InternalServerErrorResult(ex);
    }
	
    // Never reaches here on exception...
}
```


## Test Coverage
A threshold will be added to this package to ensure the test coverage is above 80% for statements, branches, functions and lines.  If it's not above the required threshold 
(threshold that will be implemented on ALL of the new core repositories going forward), then the build will fail.

## Compatibility
This package has has been written in .net Standard and can be therefore be referenced from a .net Core or .net Framework application. The advantage of utilising from a .net Core application, 
is that it can be deployed and run on a number of host operating systems, such as Windows, Linux or OSX.  Unlike referencing from the a .net Framework application, which can only run on 
Windows (or Linux using Mono).
 
## Setup
This package requires the .net Core 2.1 SDK, it can be downloaded here: 
https://www.microsoft.com/net/download/dotnet-core/2.1

IDE of Visual Studio or Visual Studio Code, can be downloaded here:
https://visualstudio.microsoft.com/downloads/

## How to access this package
All of the Cloud.Core.* packages are published to our internal NuGet feed.  To consume this on your local development machine, please add the following feed to your feed sources in Visual Studio:
TBC

For help setting up, follow this article: https://docs.microsoft.com/en-us/vsts/package/nuget/consume?view=vsts
