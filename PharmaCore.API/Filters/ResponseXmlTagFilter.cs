using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PharmaCore.API.Filters;

/// <summary>
/// Reads &lt;response code="..."> XML tags from controller methods
/// and applies them as Swagger response descriptions.
/// </summary>
public sealed class ResponseXmlTagFilter : IOperationFilter
{
    private readonly XPathDocument _xmlDoc;

    public ResponseXmlTagFilter(string xmlPath)
    {
        using var stream = File.OpenRead(xmlPath);
        _xmlDoc = new XPathDocument(stream);
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;
        if (methodInfo.DeclaringType is null) return;

        var memberName = $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
        var nav = _xmlDoc.CreateNavigator();

        // Build parameter list to match the method signature in XML
        var parameters = methodInfo.GetParameters();
        var paramTypes = string.Join(",", parameters.Select(p => p.ParameterType.FullName ?? p.ParameterType.Name));
        var searchName = $"{memberName}({paramTypes})";

        var node = nav.SelectSingleNode($"//member[@name='{XmlEscape(searchName)}']");
        if (node is null) return;

        var responses = node.Select("response");
        while (responses.MoveNext())
        {
            var code = responses.Current.GetAttribute("code", "");
            var desc = responses.Current.Value?.Trim();
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(desc)
                && operation.Responses.TryGetValue(code, out var responseObj))
            {
                responseObj.Description = desc;
            }
        }
    }

    private static string XmlEscape(string value)
    {
        return value.Replace("'", "apos;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
