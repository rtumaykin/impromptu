using System;
using System.IO;
using System.Linq;
using Calculator.Extension;
using Impromptu;
using Impromptu.AssemblyResolver;

namespace SharedTypes
{
    class Program
    {
        static void Main(string[] args)
        {
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                var appDomainSetup = AppDomain.CurrentDomain.SetupInformation;

                appDomainSetup.DisallowApplicationBaseProbing = true;
                var workerDomain = AppDomain.CreateDomain("Worker Domain", null, appDomainSetup);
                workerDomain.ExecuteAssembly(typeof(Program).Assembly.Location);
            }
            else
            {
                DefaultContext.WireUpResolver();

                var config = AppDomain.CurrentDomain.BaseDirectory.Split('\\').Last(s => !string.IsNullOrWhiteSpace(s));

                const string additorRelativePath = "..\\..\\..\\Calculator.Extension.Additor\\bin\\";
                const string subtractorRelativePath = "..\\..\\..\\Calculator.Extension.Subtractor\\bin\\";

                var nugetPackageRetriever =
                    new Impromptu.Package.NugetPackageRetriever(new[]
                    {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, additorRelativePath, config),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subtractorRelativePath, config)
                    });

                // cleanup
                var additorFolderPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ImpromptuPackages", "Calculator.Extension.Additor.1.0.0");

                var subtractorFolderPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ImpromptuPackages", "Calculator.Extension.Subtractor.1.0.0");

                // cleanup
                if (Directory.Exists(additorFolderPath))
                    Directory.Delete(additorFolderPath, true);

                if (Directory.Exists(subtractorFolderPath))
                    Directory.Delete(subtractorFolderPath, true);

                var factory = new InstantiatorFactory<ICalculator>(nugetPackageRetriever);
                // this line shows that it is referencing shared type from its local directory
                Console.WriteLine(
                    $"Main Program SharedType Runtime Version: {typeof(SharedType).Assembly.ImageRuntimeVersion}. Codebase: {typeof(SharedType).Assembly.CodeBase}.");

                var additionResult =
                    factory.Instantiate(new InstantiatorKey("Calculator.Extension.Additor", "1.0.0", "Calculator.Extension.Additor"))
                        .Calculate(10, 5);
                Console.WriteLine($"Addition Result = {additionResult}");

                var subtractionResult =
                    factory.Instantiate(new InstantiatorKey("Calculator.Extension.Subtractor", "1.0.0", "Calculator.Extension.Subtractor"))
                        .Calculate(10, 5);
                Console.WriteLine($"Subtraction Result = {subtractionResult}");

                Console.ReadKey();
            }
        }
    }
}
