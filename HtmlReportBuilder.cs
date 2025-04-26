

public class HtmlReportBuilder
{
    private readonly StringBuilder _sb = new StringBuilder();

    public HtmlReportBuilder(string xmlFilePath)
    {
        xmlFilePath = Path.GetFullPath(xmlFilePath);
        if (!File.Exists(xmlFilePath))
            throw new FileNotFoundException(xmlFilePath);

        string xmlText = File.ReadAllText(xmlFilePath);
        XDocument doc = XDocument.Parse(xmlText);
        XElement assemblyElement = doc.Descendants("Assembly").First();

        AddAssemblyInfo(assemblyElement);

        TypeMetrics[] typeMetrics = GetAllTypeMetrics(assemblyElement);
        AddTypeInfo(typeMetrics);
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
        string html = @"<!doctype html>
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
        string[] assemblyParts = assemblyElement.Attribute("Name")!.Value.Split(", ");
        string assemblyName = assemblyParts[0];
        string assemblyVersion = assemblyParts[1].Split("=")[1];

        AddHeading("Assembly Metrics", 3);
        AddParagraph($"Assembly: <code>{assemblyName} {assemblyVersion}</code>");
        foreach (XElement metric in assemblyElement.Element("Metrics")!.Descendants())
        {
            string name = metric.Attribute("Name")!.Value;
            int value = int.Parse(metric.Attribute("Value")!.Value.ToString());
            AddParagraph($"{name}: <code>{value:N0}</code>");
        }
    }

    TypeMetrics[] GetAllTypeMetrics(XElement assembly)
    {
        List<TypeMetrics> typeMetrics = new();

        foreach (XElement namespaceElement in assembly.Element("Namespaces")!.Elements("Namespace"))
        {
            string typeNamespace = namespaceElement.Attribute("Name")!.Value;
            foreach (XElement namedType in namespaceElement.Elements("Types").Elements("NamedType"))
            {
                TypeMetrics metrics = new(namedType, typeNamespace);
                typeMetrics.Add(metrics);
            }
        }

        return typeMetrics.ToArray();
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
}
