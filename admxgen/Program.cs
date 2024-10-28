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
    public static byte[] CreateZipFile(string admxContent, string admlContent, string filePrefix)
    {
      using (var memoryStream = new MemoryStream())
      {
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
          var admxEntry = archive.CreateEntry($"{filePrefix}.admx");
          using (var admxStream = admxEntry.Open())
          using (var admxWriter = new StreamWriter(admxStream))
          {
            admxWriter.Write(admxContent);
          }

          var admlEntry = archive.CreateEntry($"{filePrefix}.adml");
          using (var admlStream = admlEntry.Open())
          using (var admlWriter = new StreamWriter(admlStream))
          {
            admlWriter.Write(admlContent);
          }
        }
        return memoryStream.ToArray();
      }
    }

    public static byte[] GenerateAdmx(AdmxSettings admxSettings)
    {
      try
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

        var admxOutput = new StringBuilder();
        using (var w = XmlWriter.Create(admxOutput, xmlWriterSettings))
        {
          var ser = new XmlSerializer(parser.Definitions.GetType(), "http://schemas.microsoft.com/GroupPolicy/2006/07/PolicyDefinitions");
          ser.Serialize(w, parser.Definitions);
        }

        var admlOutput = new StringBuilder();
        using (var w = XmlWriter.Create(admlOutput, xmlWriterSettings))
        {
          var ser = new XmlSerializer(parser.Resources.GetType(), "http://www.microsoft.com/GroupPolicy/PolicyDefinitions");
          ser.Serialize(w, parser.Resources);
        }

        return CreateZipFile(admxOutput.ToString(), admlOutput.ToString(), admxSettings.TargetNamespace.Namespace);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
        return Encoding.UTF8.GetBytes(e.ToString());
      }
    }
  }
}
