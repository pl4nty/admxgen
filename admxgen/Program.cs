using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using admxgen;

using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO.Compression;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();

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
    public string SupersededPolicyFiles { get; set; }
    public string File { get; set; }
  }

  class Program
  {
    public static byte[] GenerateAdmx(AdmxSettings admxSettings)
    {
        var parser = new InputParser(new StringReader(admxSettings.File));
        parser.Parse();

        parser.Definitions.revision = admxSettings.Revision;
        parser.Definitions.schemaVersion = admxSettings.SchemaVersion;
        parser.Definitions.policyNamespaces.target.prefix = admxSettings.TargetNamespace.Prefix;
        parser.Definitions.policyNamespaces.target.@namespace = admxSettings.TargetNamespace.Namespace;
        parser.Definitions.resources.minRequiredRevision = admxSettings.MinRequiredRevision;
        parser.Definitions.resources.fallbackCulture = admxSettings.FallbackCulture;
        if (admxSettings.SupersededPolicyFiles != "")
        {
          parser.Definitions.supersededAdm = admxSettings.SupersededPolicyFiles.Split(",").Select(s => new FileReference { fileName = s }).ToArray();
        }
        else
        {
          parser.Definitions.supersededAdm = [];
        }

        parser.Resources.revision = admxSettings.Revision;
        parser.Resources.schemaVersion = admxSettings.SchemaVersion;
        parser.Resources.displayName = admxSettings.DisplayName;
        parser.Resources.description = admxSettings.Description;

        var xmlWriterSettings = new XmlWriterSettings
        {
          Indent = true,
          Encoding = Encoding.UTF8
        };

        using (var memoryStream = new MemoryStream())
        {
          using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
          {
            var filePrefix = admxSettings.TargetNamespace.Namespace;

            using (var admxStream = archive.CreateEntry($"{filePrefix}.admx").Open())
            using (var streamWriter = new StreamWriter(admxStream))
            using (var w = XmlWriter.Create(streamWriter, xmlWriterSettings))
            {
              var ser = new XmlSerializer(parser.Definitions.GetType(), "http://schemas.microsoft.com/GroupPolicy/2006/07/PolicyDefinitions");
              using (var stringWriter = new StringWriter())
              {
                ser.Serialize(stringWriter, parser.Definitions);
                var admxContent = stringWriter.ToString().Replace("<policyNamespaces>", $"<!-- created with https://admxgen.tplant.com.au -->\n  <policyNamespaces>");
                streamWriter.Write(admxContent);
              }
            }

            using (var admlStream = archive.CreateEntry($"{filePrefix}.adml").Open())
            using (var streamWriter = new StreamWriter(admlStream))
            using (var w = XmlWriter.Create(streamWriter, xmlWriterSettings))
            {
              var ser = new XmlSerializer(parser.Resources.GetType(), "http://www.microsoft.com/GroupPolicy/PolicyDefinitions");
              using (var stringWriter = new StringWriter())
              {
                ser.Serialize(stringWriter, parser.Resources);
                var admlContent = stringWriter.ToString().Replace("<displayName>", $"<!-- created with https://admxgen.tplant.com.au -->\n  <displayName>");
                streamWriter.Write(admlContent);
              }
            }
          }
          return memoryStream.ToArray();
        }
    }
  }
}
