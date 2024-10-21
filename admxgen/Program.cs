using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace admxgen
{
    public class TargetNamespace
    {
        public string Prefix { get; set; }
        public string Namespace { get; set; }
    }

    public class AdmxSettings
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Revision { get; set; }
        public string MinRequiredRevision { get; set; }
        public TargetNamespace TargetNamespace { get; set; }
        public string SchemaVersion { get; set; }
        public string FallbackCulture { get; set; }
        public List<string> SupersededPolicyFiles { get; set; }
        public string File { get; set; }
    }

    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }
}
