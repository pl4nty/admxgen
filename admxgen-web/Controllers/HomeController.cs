using Microsoft.AspNetCore.Mvc;
using admxgen;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace admxgen_web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string displayName, string description, string revision, string minRequiredRevision, string schemaVersion, string targetNamespacePrefix, string targetNamespace, string fallbackCulture, string supersededPolicyFiles, string csvData)
        {
            var admxSettings = new AdmxSettings
            {
                DisplayName = displayName,
                Description = description,
                Revision = revision,
                MinRequiredRevision = minRequiredRevision,
                SchemaVersion = schemaVersion,
                TargetNamespace = new TargetNamespace
                {
                    Prefix = targetNamespacePrefix,
                    Namespace = targetNamespace
                },
                FallbackCulture = fallbackCulture,
                SupersededPolicyFiles = supersededPolicyFiles.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries).ToList(),
                File = "input.csv"
            };

            var parser = new InputParser(new StringReader(csvData));
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

            string admxContent;
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
            {
                var serializer = new XmlSerializer(parser.Definitions.GetType(), "http://schemas.microsoft.com/GroupPolicy/2006/07/PolicyDefinitions");
                serializer.Serialize(xmlWriter, parser.Definitions);
                admxContent = stringWriter.ToString();
            }

            string admlContent;
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
            {
                var serializer = new XmlSerializer(parser.Resources.GetType(), "http://www.microsoft.com/GroupPolicy/PolicyDefinitions");
                serializer.Serialize(xmlWriter, parser.Resources);
                admlContent = stringWriter.ToString();
            }

            TempData["AdmxContent"] = admxContent;
            TempData["AdmlContent"] = admlContent;

            return RedirectToAction("Result");
        }

        [HttpGet]
        public IActionResult Result()
        {
            ViewBag.AdmxContent = TempData["AdmxContent"];
            ViewBag.AdmlContent = TempData["AdmlContent"];
            return View();
        }
    }
}
