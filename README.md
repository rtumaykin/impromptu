#![Impromptu Logo](Impromptu-icon.png) impromptu

**impromptu** is a lightweight .NET framework for plugin based systems. It replaces the default .NET assembly binding mechanism with a custom one. This allows to isolate the resolution of plugin's referenced assemblies within the plugin's own directory. As a result there is no need to isolate each plugin into its own AppDomain, which is extremely expensive and inefficient. The framework also makes plugin instantiation extremely fast, since upon the first plugin use, it creates, compiles, and stores a special static Instantiator class, which makes subsequent creation of plugin instances almost as fast as if it was bound during the compile time.

The framework has he following requirements:

 - A plugin must be distributed as a NuGet package with a special structure - all of the plugin assemblies need to be placed into a package folder "impromptu".  
 - A plugin needs to implement a base class or an interface (*Base Type*) that is also known to the main application, therefore the only assembly that will be resolved to the main application binding context, will be the one that contains that type (*Shared Assembly*). Inside the repository, there is an example of an application "[calculator](src/examples/calculator/Calculator)" that uses plugins Additor and Subtractor.  
 - All communication between main application and plugins needs to use only .NET framework types, or types that are defined in the *Shared Assembly*. If main application and a plugin resolves the type each to their own local assembly, then a type mismatch exception will be thrown. To avoid this, such types need to be serialized by caller, and deserialized by callee. Don't use standard serializer, use JSON.NET instead.
 - Main application AppDomain needs to be modified as it is demonstrated in the "calculator" example. We need to prevent default assembly dependency resolution in the AppDomain Base Directory: `appDomainSetup.DisallowApplicationBaseProbing = true;`

There are only  a few simple steps that are necessary to create new instances of a plugin:
```
var nugetPackageRetriever = new Impromptu.Package.NugetPackageRetriever(new[]
	{
	    [path_to_nuget_source]
	});

var factory = new InstantiatorFactory<[BaseType]>;(nugetPackageRetriever);

var instance = factory.Instantiate(new InstantiatorKey("[name_of_nuget_package]", "[version_of_nuget_package]", "[type to instantiate]"), [constructorArg1], ..., [constructorArgN]);
```

That's all for now. More detailed post is coming soon. 
Contact me if you have any questions or suggestions. 
