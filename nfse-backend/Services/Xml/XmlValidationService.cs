using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace nfse_backend.Services.Xml
{
    public class XmlValidationService
    {
        private readonly Dictionary<string, XmlSchemaSet> _schemaSets;
        private readonly List<string> _validationErrors;

        public XmlValidationService()
        {
            _schemaSets = new Dictionary<string, XmlSchemaSet>();
            _validationErrors = new List<string>();
            InitializeSchemas();
        }

        private void InitializeSchemas()
        {
            // NF-e v4.00 schema initialization
            var nfeSchemaSet = new XmlSchemaSet();
            var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schemas", "NFe", "v4.00");
            
            if (Directory.Exists(schemaPath))
            {
                foreach (var xsdFile in Directory.GetFiles(schemaPath, "*.xsd"))
                {
                    nfeSchemaSet.Add(null, xsdFile);
                }
                _schemaSets["NFe"] = nfeSchemaSet;
            }

            // NFS-e schema initialization (ABRASF pattern)
            var nfseSchemaSet = new XmlSchemaSet();
            var nfseSchemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schemas", "NFSe");
            
            if (Directory.Exists(nfseSchemaPath))
            {
                foreach (var xsdFile in Directory.GetFiles(nfseSchemaPath, "*.xsd"))
                {
                    nfseSchemaSet.Add(null, xsdFile);
                }
                _schemaSets["NFSe"] = nfseSchemaSet;
            }
        }

        public bool ValidateXml(string xmlContent, string schemaType, out List<string> errors)
        {
            _validationErrors.Clear();
            errors = new List<string>();

            if (!_schemaSets.ContainsKey(schemaType))
            {
                errors.Add($"Schema type '{schemaType}' not found");
                return false;
            }

            try
            {
                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    Schemas = _schemaSets[schemaType]
                };

                settings.ValidationEventHandler += ValidationEventHandler;

                using (var stringReader = new StringReader(xmlContent))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                {
                    while (xmlReader.Read()) { }
                }

                errors = new List<string>(_validationErrors);
                return _validationErrors.Count == 0;
            }
            catch (Exception ex)
            {
                errors.Add($"XML validation error: {ex.Message}");
                return false;
            }
        }

        private void ValidationEventHandler(object? sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
            {
                _validationErrors.Add($"{e.Severity}: {e.Message} (Line: {e.Exception?.LineNumber}, Position: {e.Exception?.LinePosition})");
            }
        }

        public bool ValidateNFeXml(string xmlContent, out List<string> errors)
        {
            return ValidateXml(xmlContent, "NFe", out errors);
        }

        public bool ValidateNFSeXml(string xmlContent, out List<string> errors)
        {
            return ValidateXml(xmlContent, "NFSe", out errors);
        }
    }
}
