

public sealed class HtmlReportBuilder
{
    private readonly StringBuilder _sb = new StringBuilder();

    public HtmlReportBuilder(string xmlFilePath)
    {
        xmlFilePath = Path.GetFullPath(xmlFilePath);
        if (!File.Exists(xmlFilePath))
            throw new FileNotFoundException(xmlFilePath);

        var xmlText = File.ReadAllText(xmlFilePath);
        var doc = XDocument.Parse(xmlText);
        var assemblyElement = doc.Descendants("Assembly").First();

        AddAssemblyInfo(assemblyElement);

        var typeMetrics = GetAllTypeMetrics(assemblyElement);
        AddTypeInfo(typeMetrics);

        var memberMetrics = GetMemberMetricsAsTypeMetrics(assemblyElement);
        AddMemberInfoAsTypeMetrics(memberMetrics);
    }

    private static string StripHtml(string s) => s
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\r", "")
        .Replace("\n", "<br>");

    public void AddBullets(string title, string[] lines)
    {
        _sb.AppendLine($"<h3>{StripHtml(title)}</h3>");
        _sb.AppendLine("<ul>");
        foreach (string line in lines)
        {
            _sb.AppendLine($"<li>{StripHtml(line)}</li>");
        }
        _sb.AppendLine("</ul>");
    }

    public void AddHeading(string title, int level = 1)
    {
        _sb.AppendLine($"<h{level}>{title}</h{level}>");
    }

    public void AddDiv(string content)
    {
        _sb.AppendLine($"<div>{content}</div>");
    }

    public void AddParagraph(string content)
    {
        _sb.AppendLine($"<p>{content}</p>");
    }

    public void SaveAs(string path)
    {
        var html = @"<!doctype html>
<html lang='en'>
  <head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <link rel='stylesheet' href='https://unpkg.com/@picocss/pico@1.*/css/pico.min.css'>
    <title>Hello, world!</title>
  </head>
  <body>
    <main class='container'>
      {{CONTENT}}
    </main>
  </body>
</html>".Replace("{{CONTENT}}", _sb.ToString());

        path = Path.GetFullPath(path);
        File.WriteAllText(path, html);
        Console.WriteLine($"Saved: {path}");
    }

    void AddAssemblyInfo(XElement assemblyElement)
    {
        var assemblyParts = assemblyElement.Attribute("Name")!.Value.Split(", ");
        var assemblyName = assemblyParts[0];
        var assemblyVersion = assemblyParts[1].Split("=")[1];

        AddHeading("Assembly Metrics", 3);
        AddParagraph($"Assembly: <code>{assemblyName} {assemblyVersion}</code>");
        foreach (XElement metric in assemblyElement.Element("Metrics")!.Descendants())
        {
            var name = metric.Attribute("Name")!.Value;
            var value = int.Parse(metric.Attribute("Value")!.Value.ToString());
            AddParagraph($"{name}: <code>{value:N0}</code>");
        }
    }

    TypeMetrics[] GetAllTypeMetrics(XElement assembly)
    {
        List<TypeMetrics> typeMetrics = new();

        foreach (XElement namespaceElement in assembly.Element("Namespaces")!.Elements("Namespace"))
        {
            var typeNamespace = namespaceElement.Attribute("Name")!.Value;
            foreach (XElement namedType in namespaceElement.Elements("Types").Elements("NamedType"))
            {
                TypeMetrics metrics = new(namedType, typeNamespace);
                typeMetrics.Add(metrics);
            }
        }

        return typeMetrics.ToArray();
    }

    List<TypeMetrics> GetMemberMetricsAsTypeMetrics(XElement assembly)
    {
        var allMemberMetrics = new List<TypeMetrics>();

        foreach (XElement namespaceElement in assembly.Element("Namespaces")!.Elements("Namespace"))
        {
            var typeNamespace = namespaceElement.Attribute("Name")!.Value;
            foreach (XElement namedType in namespaceElement.Elements("Types").Elements("NamedType"))
            {
                var typeName = namedType.Attribute("Name")!.Value;
                var fullTypeName = $"{typeNamespace}.{typeName}";

                var membersElement = namedType.Element("Members");
                if (membersElement != null)
                {
                    foreach (XElement memberElement in membersElement.Elements())
                    {
                        var memberName = memberElement.Attribute("Name")?.Value;
                        var memberType = memberElement.Name.LocalName;

                        if (!string.IsNullOrEmpty(memberName) && memberType is "Method" or "Field" or "Property")
                        {
                            TypeMetrics memberMetrics = CreateMemberTypeMetrics(memberElement, fullTypeName, memberType);
                            allMemberMetrics.Add(memberMetrics);
                        }
                    }
                }
            }
        }

        return allMemberMetrics;
    }

    private TypeMetrics CreateMemberTypeMetrics(XElement memberElement, string fullTypeName, string memberType)
    {
        var memberName = memberElement.Attribute("Name")!.Value;
        // 臨时的 XElement 來模擬 TypeMetrics 的建構子輸入
        var tempElement = new XElement("NamedType",
            new XAttribute("Name", memberName),
            new XElement("Metrics", memberElement.Element("Metrics")?.Elements()) // 複製度量元素
        );
        var memberMetrics = new TypeMetrics(tempElement, fullTypeName);
        return new TypeMetrics(
            memberName, // 使用成員名稱作為 TypeName
            fullTypeName, // 使用完整的類型名稱作為 TypeNamespace
            $"{fullTypeName}.{memberName}", // 組合 FullTypeName
            memberMetrics.MaintainabilityIndex,
            memberMetrics.CyclomaticComplexity,
            memberMetrics.SourceLines,
            memberMetrics.ExecutableLines,
            memberMetrics.ClassCoupling,
            memberType
        );
    }

    void AddTypeInfo(TypeMetrics[] metricsByType)
    {
        AddHeading("Metrics by Type", 3);

        _sb.AppendLine("<table>");

        _sb.AppendLine("<tr>");
        _sb.AppendLine("<th><b>Type</b></th>");
        _sb.AppendLine("<th><b>Maintainability</b></th>");
        _sb.AppendLine("<th><b>Complexity</b></th>");
        _sb.AppendLine("<th><b>ClassCoupling</b></th>");
        _sb.AppendLine("<th><b>Lines of Code</b></th>");
        _sb.AppendLine("<th><b>Lines of Executable Code</b></th>");
        _sb.AppendLine("</tr>");

        foreach (TypeMetrics metrics in metricsByType)
        {
            _sb.AppendLine("<tr>");
            _sb.AppendLine($"<td><code>{metrics.FullTypeName}</code></td>");
            _sb.AppendLine($"<td style='background-color: {metrics.MaintainabilityIndexColor};'>{metrics.MaintainabilityIndex}</td>");
            _sb.AppendLine($"<td style='background-color: {metrics.CyclomaticComplexityColor};'>{metrics.CyclomaticComplexity}</td>");
            _sb.AppendLine($"<td style='background-color: {metrics.ClassCouplingColor};'>{metrics.ClassCoupling}</td>");
            _sb.AppendLine($"<td style='background-color: {metrics.SourceLinesColor};'>{metrics.SourceLines}</td>");
            _sb.AppendLine($"<td style='background-color: {metrics.ExecutableLinesColor};'>{metrics.ExecutableLines}</td>");
            _sb.AppendLine("</tr>");
        }

        _sb.AppendLine("</table>");
    }

    void AddMemberInfoAsTypeMetrics(List<TypeMetrics> memberMetrics)
    {
        AddHeading("Metrics by Member", 3);

        _sb.AppendLine("<table>");
        _sb.AppendLine("<tr>");
        _sb.AppendLine("<th><b>Name</b></th>");
        _sb.AppendLine("<th><b>Type</b></th>"); // 新增 Type 欄位
        _sb.AppendLine("<th><b>Container Type</b></th>"); // 新增 Container Type 欄位
        _sb.AppendLine("<th><b>Maintainability</b></th>");
        _sb.AppendLine("<th><b>Complexity</b></th>");
        _sb.AppendLine("<th><b>Lines of Code</b></th>");
        _sb.AppendLine("<th><b>Executable Lines</b></th>");
        _sb.AppendLine("<th><b>Class Coupling</b></th>");
        _sb.AppendLine("</tr>");

        foreach (var memberMetric in memberMetrics)
        {
            _sb.AppendLine("<tr>");
            _sb.AppendLine($"<td><code>{memberMetric.TypeName}</code></td>");
            _sb.AppendLine($"<td><code>{memberMetric.MemberType}</code></td>"); // 顯示成員類型
            _sb.AppendLine($"<td><code>{memberMetric.TypeNamespace}</code></td>"); // 顯示容器類型
            _sb.AppendLine($"<td style='background-color: {memberMetric.MaintainabilityIndexColor};'>{memberMetric.MaintainabilityIndex}</td>");
            _sb.AppendLine($"<td style='background-color: {memberMetric.CyclomaticComplexityColor};'>{memberMetric.CyclomaticComplexity}</td>");
            _sb.AppendLine($"<td style='background-color: {memberMetric.SourceLinesColor};'>{memberMetric.SourceLines}</td>");
            _sb.AppendLine($"<td style='background-color: {memberMetric.ExecutableLinesColor};'>{memberMetric.ExecutableLines}</td>");
            _sb.AppendLine($"<td style='background-color: {memberMetric.ClassCouplingColor};'>{memberMetric.ClassCoupling}</td>");
            _sb.AppendLine("</tr>");
        }

        _sb.AppendLine("</table>");
    }
}
