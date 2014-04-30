# Splunk SDK for C# v2.0
### Version 2.0 preview

> **Important:** The Splunk Software Development Kit (SDK) for C# version 2.0 
> is a complete rewrite of the Splunk SDK for C# version 1.0, and introduces 
> completely new APIs. **Applications built with Splunk SDK for C# 1.x will not 
> recompile using Splunk SDK for C# version 2.0.** For more information, see
> [Compatibility](#compat).

The Splunk SDK for C# v2.0 contains library code and examples designed to 
enable developers to build applications using Splunk Enterprise.

Splunk Enterprise is a search engine and analytic environment that uses a 
distributed map-reduce architecture to efficiently index, search, and process 
large time-varying data sets.

The Splunk Enterprise product is popular with system administrators for 
aggregation and monitoring of IT machine data, security, compliance and a 
wide variety of other scenarios that share a requirement to efficiently 
index, search, analyze, and generate real-time notifications from large 
volumes of time series data.

The Splunk developer platform enables developers to take advantage of the 
same technology used by the Splunk Enterprise product to build exciting 
new applications that are enabled by Splunk Enterprise's unique capabilities.

## What's new in Version 2.0

The Splunk SDK for C# version 2.0 introduces new modern APIs that leverage 
the latest .NET platform advancements.

* Async - All APIs are 100% asynchronous supporting the new [async/await](http://msdn.microsoft.com/en-us/library/hh191443.aspx) features.
* All APIs follow .NET guidelines and abide by FxCop and StyleCop rules.
* Reactive Extensions - Splunk Enterprise query results implement [IObservable<T>](http://msdn.microsoft.com/library/dd990377), allowing usage with the [.NET Reactive Extensions](http://msdn.microsoft.com/data/gg577610).
* Support for multiple platforms - The Splunk API client (Splunk.Client.dll) in the new version is a [Portable Class Library](http://msdn.microsoft.com/library/vstudio/gg597391.aspx).

## Supported platforms
* Microsoft .NET Framework 4.5
* Windows 8.1
* Windows Phone 8.1
* iOS (via Xamarin.iOS)
* Android (via Xamarin.Android)
* OS X (via Xamarin.Mac)

## [Compatibility](id:compat)

The Splunk SDK for C# version 2.0 is a rewrite of the Splunk SDK for C# 
version 1.0, and introduces completely new APIs.

> **Important:** Applications built with the Splunk SDK for C# v1.x will 
> not recompile using the Splunk SDK for C# v2.0.

Splunk SDK for C# v2.0 includes a subset of the capability in version 
1.0 of the SDK, and focuses on the most common scenarios that we have 
seen customers using. The major focus areas are _search_, _search jobs_, 
_configuration_, and _modular inputs_.

Following is a breakdown of the areas covered:
* Login
* Access control (users and passwords)
* Searches (normal, blocking, oneshot, and export)
* Jobs
* Reports ("saved searches" in Splunk Enterprise 5)
* Configuration and Config Properties
* Indexes
* Inputs (sending simple and streamed events to Splunk Enterprise)
* Applications
* Modular inputs

For detailed API coverage, see the Splunk SDK for C# v2.0 API reference 
documentation.

Below is an example of a simple normal search:

```csharp
using Splunk.Client;

var service = new Service(Scheme.Https, "localhost", 8089));

//login
await service.LoginAsync("admin", "changeme");

//create a job
var job = await service.StartJobAsync("search index=_internal | head 10");

//get the results
var searchResults = await job.GetSearchResultsAsync());

//loop through the results
foreach (var record in searchResults)
{
    Console.WriteLine(string.Format("{0:D8}: {1}", ++recordNumber, record));
}
```

For detailed API coverage, see the Splunk SDK for C# v2.0 API reference
documentation.

## Migration

Because applications built using the Splunk SDK for C# version 1.0 will not 
recompile using the Splunk SDK for C# version 2.0, your code will need to be 
migrated to the new platform. A migration guide is forthcoming.

## Getting started with the Splunk SDK for C# 

The Splunk SDK for C# contains library code and examples that show how to 
programmatically interact with Splunk for a variety of scenarios including 
searching, saved searches, data inputs, and many more, along with building 
complete applications. 

The information in this Readme provides steps to get going quickly. In the 
future we plan to roll out more in-depth documentation.

### Requirements

Here's what you need to get going with the Splunk SDK for C# v2.0.

#### Splunk Enterprise

If you haven't already installed Splunk Enterprise, download it at 
<http://www.splunk.com/download>. For more information about installing and 
running Splunk Enterprise and system requirements, see the
[Splunk Installation Manual](http://docs.splunk.com/Documentation/Splunk/latest/Installation). 

You must have a valid Splunk Enterprise install on your network, and be 
able to connect to it from the computer on which you install the SDK. 

> **Important:** The Splunk SDK for C# v2.0 need _not_ be installed on the same 
> computer as the one on which you've installed Splunk Enterprise.

#### IDE

The Splunk SDK for C# v2.0 supports development in several integrated 
development environments (IDEs):

* Microsoft Visual Studio 2012 and later. Visual Studio downloads are available 
on the [Visual Studio Downloads webpage](http://www.microsoft.com/visualstudio/downloads).
* [Xamarin](http://xamarin.com) on Windows and OS X. Using Xamarin, you can 
interact with Splunk Enterprise from your [iOS](http://iosapi.xamarin.com/), [Android](http://androidapi.xamarin.com/), or [OS X](http://macapi.xamarin.com/) app.
* [MonoDevelop](http://monodevelop.com). Using MonoDevelop with the SDK, you 
can build the ability to interact with Splunk into your Linux, Windows, or OS X
desktop app.

#### Splunk SDK for C# 

[Get the Splunk SDK for C# v2.0](https://github.com/splunk/splunk-sdk-csharp-pcl/archive/master.zip). 
Download the ZIP file and extract its contents.

If you are interested in contributing to the Splunk SDK for C#, you can 
[get it from GitHub](https://github.com/splunk/splunk-sdk-csharp) and clone the 
resources to your computer.

### Building the SDK

Before starting to develop custom software, you must first build the SDK. Once 
you've downloaded and extracted the SDK, build it using your preferred IDE. 

In Visual Studio, do the following:

1. At the root level of the **splunk-sdk-csharp-pcl** directory, open the 
**splunk-sdk-csharp-pcl.sln** file in Visual Studio.
2. On the **BUILD** menu, click **Build Solution**.

This will build the SDK, the examples, and the unit tests.

### Examples and unit tests

The Splunk SDK for C# includes full unit tests which run using 
[xunit](https://github.com/xunit/xunit). They are located in the **test** 
directory.

The Splunk SDK for C# also includes several examples. They are located in 
the **examples** directory.

### Changelog

The **CHANGELOG.md** file in the root of the repository contains a description
of changes for each version of the SDK. You can also find it online at
[https://github.com/splunk/splunk-sdk-csharp/blob/master/CHANGELOG.md](https://github.com/splunk/splunk-sdk-csharp/blob/master/CHANGELOG.md). 

### Branches

The **master** branch always represents a stable and released version of the SDK.
You can read more about our branching model on our Wiki at 
[https://github.com/splunk/splunk-sdk-csharp/wiki/Branching-Model](https://github.com/splunk/splunk-sdk-java/wiki/Branching-Model).

## Documentation and resources

If you need to know more:

* For all things developer with Splunk, your main resource is the [Splunk
  Developer Portal](http://dev.splunk.com).
* For conceptual and how-to documentation, see the Overview of the Splunk SDK for C#.
* For API reference documentation, see the Splunk SDK for C# Reference.
* For more about the Splunk REST API, see the [REST API 
  Reference](http://docs.splunk.com/Documentation/Splunk/latest/RESTAPI).
* For more about about Splunk Enterprise in general, see 
  [Splunk>Docs](http://docs.splunk.com/Documentation/Splunk).
* For more about this SDK's repository, see our GitHub Wiki.

## Community

Stay connected with other developers building on Splunk.

<table>

<tr>
<td><em>Email</em></td>
<td><a href="mailto:devinfo@splunk.com">devinfo@splunk.com</a></td>
</tr>

<tr>
<td><em>Issues</em>
<td><a href="https://github.com/splunk/splunk-sdk-csharp-pcl/issues/">
https://github.com/splunk/splunk-sdk-csharp/issues</a></td>
</tr>

<tr>
<td><em>Answers</em>
<td><a href="http://splunk-base.splunk.com/tags/csharp/">
http://splunk-base.splunk.com/tags/csharp/</a></td>
</tr>

<tr>
<td><em>Blog</em>
<td><a href="http://blogs.splunk.com/dev/">http://blogs.splunk.com/dev/</a></td>
</tr>

<tr>
<td><em>Twitter</em>
<td><a href="http://twitter.com/splunkdev">@splunkdev</a></td>
</tr>

</table>

### Contributions

If you want to make a code contribution, go to the 
[Open Source](http://dev.splunk.com/view/opensource/SP-CAAAEDM)
page for more information.

### Support

This product is currently in development and officially unsupported. We will 
be triaging any issues filed by the community, however, and addressing them 
as appropriate. Please [file](https://github.com/splunk/splunk-sdk-csharp-pcl) 
issues for any problems that you encounter.

### Contact Us

You can reach the Dev Platform team at devinfo@splunk.com.

## License

The Splunk SDK for C# is licensed under the Apache License 2.0. Details can be 
found in the LICENSE file.
