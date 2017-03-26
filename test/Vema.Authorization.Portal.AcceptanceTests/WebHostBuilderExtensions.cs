using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Vema.Authorization.Portal.AcceptanceTests
{
    public static class WebHostBuilderExtensions
    {

        public static IWebHostBuilder ConfigureRazorFix(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                //services.AddMvcCore();
                services.Configure((RazorViewEngineOptions options) =>
                {
                    var previous = options.CompilationCallback;
                    options.CompilationCallback = (context) =>
                    {
                        previous?.Invoke(context);

                        var assembly = typeof(Startup).GetTypeInfo().Assembly;
                        var assemblies = assembly.GetReferencedAssemblies()
                            .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location))
                            .ToList();

                        assemblies.Add(MetadataReference.CreateFromFile(assembly.Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Private.Corelib")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.ApplicationInsights.AspNetCore")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Html.Abstractions")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Razor")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Razor.Runtime")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Mvc")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Dynamic.Runtime")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Text.Encodings.Web")).Location));
                        assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Threading.Tasks")).Location));
                        context.Compilation = context.Compilation.AddReferences(assemblies);
                    };
                });

                services.AddApplicationInsightsTelemetry();
            });
        }
    }
}