using Microsoft.AspNetCore.Mvc;
using admxgen.Models;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;

namespace admxgen.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Generate(AdmxSettings settings, List<admxgen.Models.PolicyDefinition> policyDefinitions)
        {
            var policyDefinitionsObj = new PolicyDefinitions
            {
                policyNamespaces = new PolicyNamespaces
                {
                    target = new PolicyNamespaceAssociation
                    {
                        prefix = settings.TargetNamespace.Prefix,
                        @namespace = settings.TargetNamespace.Namespace
                    }
                },
                resources = new LocalizationResourceReference
                {
                    minRequiredRevision = settings.MinRequiredRevision,
                    fallbackCulture = settings.FallbackCulture
                },
                supportedOn = new SupportedOnTable
                {
                    definitions = policyDefinitions.Select(pd => new SupportedOnDefinition
                    {
                        name = pd.SupportedOnId,
                        displayName = pd.SupportedOn
                    }).ToArray()
                },
                categories = new CategoryList
                {
                    category = policyDefinitions.Select(pd => new Category
                    {
                        name = pd.CategoryId,
                        displayName = pd.Category
                    }).ToArray()
                },
                policies = new PolicyList
                {
                    policy = policyDefinitions.Select(pd => new PolicyDefinition
                    {
                        name = pd.Id,
                        parentCategory = new CategoryReference { @ref = pd.CategoryId },
                        displayName = pd.DisplayName,
                        @class = (PolicyClass)System.Enum.Parse(typeof(PolicyClass), pd.Class),
                        explainText = pd.Explanation,
                        key = pd.RegistryKey,
                        valueName = pd.ValueName,
                        supportedOn = new SupportedOnReference { @ref = pd.SupportedOnId },
                        elements = new object[] { new BooleanElement
                        {
                            id = pd.Id,
                            key = pd.RegistryKey,
                            valueName = pd.ValueName,
                            trueValue = new Value { Item = new ValueDecimal { value = 1 } },
                            falseValue = new Value { Item = new ValueDecimal { value = 0 } }
                        }},
                        presentation = $"$(presentation.{pd.Id})"
                    }).ToArray()
                }
            };

            var resourcesObj = new PolicyDefinitionResources
            {
                resources = new Localization
                {
                    stringTable = new LocalizationStringTable
                    {
                        @string = policyDefinitions.Select(pd => new LocalizedString
                        {
                            id = pd.Id,
                            Value = pd.DisplayName
                        }).ToArray()
                    },
                    presentationTable = new LocalizationPresentationTable
                    {
                        presentation = policyDefinitions.Select(pd => new PolicyPresentation
                        {
                            id = pd.Id,
                            Items = new object[] { new CheckBox { refId = pd.Id, Value = pd.DisplayName } }
                        }).ToArray()
                    }
                }
            };

            var admxSerializer = new XmlSerializer(typeof(PolicyDefinitions));
            var admlSerializer = new XmlSerializer(typeof(PolicyDefinitionResources));

            using (var admxStream = new MemoryStream())
            using (var admlStream = new MemoryStream())
            {
                admxSerializer.Serialize(admxStream, policyDefinitionsObj);
                admlSerializer.Serialize(admlStream, resourcesObj);

                var admxContent = Encoding.UTF8.GetString(admxStream.ToArray());
                var admlContent = Encoding.UTF8.GetString(admlStream.ToArray());

                using (var zipStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        var admxEntry = archive.CreateEntry("policyDefinitions.admx");
                        using (var entryStream = admxEntry.Open())
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            streamWriter.Write(admxContent);
                        }

                        var admlEntry = archive.CreateEntry("policyDefinitions.adml");
                        using (var entryStream = admlEntry.Open())
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            streamWriter.Write(admlContent);
                        }
                    }

                    return File(zipStream.ToArray(), "application/zip", "policyDefinitions.zip");
                }
            }
        }
    }
}
