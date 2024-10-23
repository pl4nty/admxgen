using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using admxgen;

namespace admxgen_web.Controllers
{
    public class AdmxGenController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GenerateAdmx(AdmxSettings admxSettings, string policyDefinitions)
        {
            var parser = new InputParser(policyDefinitions);
            parser.Parse();

            parser.Definitions.revision = admxSettings.Revision;
            parser.Definitions.schemaVersion = admxSettings.SchemaVersion;
            parser.Definitions.policyNamespaces.target.prefix = admxSettings.TargetNamespace.Prefix;
            parser.Definitions.policyNamespaces.target.@namespace = admxSettings.TargetNamespace.Namespace;
            parser.Definitions.resources.minRequiredRevision = admxSettings.MinRequiredRevision;
            parser.Definitions.resources.fallbackCulture = admxSettings.FallbackCulture;
            parser.Definitions.supersededAdm = admxSettings.SupersededPolicyFiles.Select(s => new FileReference { fileName = s }).ToArray();

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
            using (var w = XmlWriter.Create(new StringWriter(admxOutput), xmlWriterSettings))
            {
                var ser = new XmlSerializer(parser.Definitions.GetType(), "http://schemas.microsoft.com/GroupPolicy/2006/07/PolicyDefinitions");
                ser.Serialize(w, parser.Definitions);
            }

            var admlOutput = new StringBuilder();
            using (var w = XmlWriter.Create(new StringWriter(admlOutput), xmlWriterSettings))
            {
                var ser = new XmlSerializer(parser.Resources.GetType(), "http://www.microsoft.com/GroupPolicy/PolicyDefinitions");
                ser.Serialize(w, parser.Resources);
            }

            var zipStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                var admxEntry = archive.CreateEntry("output.admx");
                using (var entryStream = admxEntry.Open())
                using (var streamWriter = new StreamWriter(entryStream))
                {
                    streamWriter.Write(admxOutput.ToString());
                }

                var admlEntry = archive.CreateEntry("output.adml");
                using (var entryStream = admlEntry.Open())
                using (var streamWriter = new StreamWriter(entryStream))
                {
                    streamWriter.Write(admlOutput.ToString());
                }
            }

            zipStream.Seek(0, SeekOrigin.Begin);
            return File(zipStream.ToArray(), "application/zip", "output.zip");
        }
    }
}
