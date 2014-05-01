using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using NHail.WebAPI.OData.Interfaces;

namespace NHail.WebAPI.OData
{
    //https://gist.github.com/dariusclay/8673940
    //http://stackoverflow.com/questions/22711496/entityframework-model-first-metadata-for-breezejs
    //https://gist.github.com/raghuramn/5864013
    public class DbContextEdmModel : IEdmModelFactory
    {
        private const string CsdlFileExtension = ".csdl";

        private const string EntityConnectionMetadataPatternText =
            @"^(res://\*/(?<name>[^\|]+))(\|res://\*/(?<name>[^\|]+)?)*$";

        private const RegexOptions EntityConnectionMetadataRegexOptions =
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase |
            RegexOptions.ExplicitCapture;

        private readonly Regex _entityConnectionMetadataPattern = new Regex(EntityConnectionMetadataPatternText,
                                                                           EntityConnectionMetadataRegexOptions);

        private Stream GetCsdlStreamFromMetadata<TSource>(ObjectContext context)
        {
            var metadata = new EntityConnectionStringBuilder(context.Connection.ConnectionString).Metadata;
            var assembly = Assembly.GetAssembly(typeof(TSource));

            var csdlResource =
                _entityConnectionMetadataPattern.Matches(metadata)
                                                .Cast<Match>()
                                                .SelectMany(m => m.Groups["name"].Captures.OfType<Capture>())
                                                .Single(c => c.Value.EndsWith(CsdlFileExtension));
            return assembly.GetManifestResourceStream(csdlResource.Value);
        }

        private IEdmModel NotCodeFirstModel<TSource>(IObjectContextAdapter source)
        {
            using (var csdlStream = GetCsdlStreamFromMetadata<TSource>(source.ObjectContext))
            {
                using (var reader = XmlReader.Create(csdlStream))
                {
                    IEdmModel model;
                    IEnumerable<EdmError> errors;
                    if (!CsdlReader.TryParse(new[] { reader }, out model, out errors))
                    {
                        return null;
                    }
                    return model;
                }
            }
        }

        private IEdmModel CodeFistModel(DbContext context)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream))
                {
                    System.Data.Entity.Infrastructure.EdmxWriter.WriteEdmx(context, writer);
                    writer.Close();
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = XmlReader.Create(stream))
                    {
                        return EdmxReader.Parse(reader);
                    }
                }
            }
        }

        public virtual IEdmModel EdmModel<TSource>(TSource source)
        {
            var dbContext = source as DbContext;
            if (dbContext == null)
            {
                return null;
            }

            try
            {
                return CodeFistModel(dbContext);
            }
            catch (NotSupportedException)
            {
            }
            var objContext = (IObjectContextAdapter) source;
            return NotCodeFirstModel<TSource>(objContext);
        }
    }
}