

public class CsvReportBuilder
{
    private readonly StringBuilder _sb = new StringBuilder();
    private readonly string _splitChar = ",";

    public CsvReportBuilder(string xmlFilePath)
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

    private static string EscapeCsvField(string s) => $"\"{s.Replace("\"", "\"\"")}\"";

    void AddAssemblyInfo(XElement assemblyElement)
    {
        string[] assemblyParts = assemblyElement.Attribute("Name")!.Value.Split(", ");
        string assemblyName = assemblyParts[0];
        string assemblyVersion = assemblyParts[1].Split("=")[1];

        // 添加 Assembly 資訊到 CSV
        _sb.AppendLine($"Section{_splitChar}Assembly{_splitChar}Version");
        _sb.AppendLine($"Assembly Info{_splitChar}{EscapeCsvField(assemblyName)}{_splitChar}{assemblyVersion}");

        // 添加 Assembly 度量數據
        _sb.AppendLine("Section;Metric Name;Value");
        foreach (XElement metric in assemblyElement.Element("Metrics")!.Descendants())
        {
            string name = metric.Attribute("Name")!.Value;
            int value = int.Parse(metric.Attribute("Value")!.Value.ToString());
            _sb.AppendLine($"Assembly Metrics{_splitChar}{EscapeCsvField(name)}{_splitChar}{value}");
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

        // 添加類型度量數據的 CSV 表頭
        _sb.AppendLine($"Section{_splitChar}Type{_splitChar}Maintainability{_splitChar}Complexity{_splitChar}ClassCoupling{_splitChar}Lines of Code{_splitChar}Lines of Executable Code");

        // 添加每個類型的度量數據
        foreach (TypeMetrics metrics in metricsByType)
        {
            _sb.AppendLine($"Type Metrics{_splitChar}{EscapeCsvField(metrics.FullTypeName)}{_splitChar}" +
                          $"{metrics.MaintainabilityIndex}{_splitChar}{metrics.CyclomaticComplexity}{_splitChar}" +
                          $"{metrics.ClassCoupling}{_splitChar}{metrics.SourceLines}{_splitChar}{metrics.ExecutableLines}");
        }
    }

    public void SaveAs(string path)
    {
        path = Path.GetFullPath(path);
        File.WriteAllText(path, _sb.ToString());
        Console.WriteLine($"Saved: {path}");
    }
}

