using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using admxgen;

namespace WebBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackendController : ControllerBase
    {
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromForm] IFormFile jsonFile, [FromForm] IFormFile csvFile)
        {
            if (jsonFile == null || csvFile == null)
            {
                return BadRequest("Both JSON and CSV files are required.");
            }

            var jsonFilePath = Path.GetTempFileName();
            var csvFilePath = Path.GetTempFileName();

            using (var jsonStream = new FileStream(jsonFilePath, FileMode.Create))
            {
                await jsonFile.CopyToAsync(jsonStream);
            }

            using (var csvStream = new FileStream(csvFilePath, FileMode.Create))
            {
                await csvFile.CopyToAsync(csvStream);
            }

            var admxSettings = JsonConvert.DeserializeObject<AdmxSettings>(await System.IO.File.ReadAllTextAsync(jsonFilePath));
            var parser = new InputParser(new StreamReader(csvFilePath));
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

            var admxFilePath = Path.GetTempFileName();
            var admlFilePath = Path.GetTempFileName();

            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (var w = XmlWriter.Create(new StreamWriter(admxFilePath), xmlWriterSettings))
            {
                var ser = new XmlSerializer(parser.Definitions.GetType(), "http://schemas.microsoft.com/GroupPolicy/2006/07/PolicyDefinitions");
                ser.Serialize(w, parser.Definitions);
            }

            using (var w = XmlWriter.Create(new StreamWriter(admlFilePath), xmlWriterSettings))
            {
                var ser = new XmlSerializer(parser.Resources.GetType(), "http://www.microsoft.com/GroupPolicy/PolicyDefinitions");
                ser.Serialize(w, parser.Resources);
            }

            var admxBytes = await System.IO.File.ReadAllBytesAsync(admxFilePath);
            var admlBytes = await System.IO.File.ReadAllBytesAsync(admlFilePath);

            return Ok(new { admx = admxBytes, adml = admlBytes });
        }
    }
}
