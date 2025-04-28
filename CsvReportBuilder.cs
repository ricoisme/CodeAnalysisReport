

public sealed class CsvReportBuilder
{
    private readonly StringBuilder _sb = new StringBuilder();
    private readonly string _splitChar = ",";

    public CsvReportBuilder(string xmlFilePath)
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

    private static string EscapeCsvField(string s) => $"\"{s.Replace("\"", "\"\"")}\"";

    void AddAssemblyInfo(XElement assemblyElement)
    {
        var assemblyParts = assemblyElement.Attribute("Name")!.Value.Split(", ");
        var assemblyName = assemblyParts[0];
        var assemblyVersion = assemblyParts[1].Split("=")[1];

        // Assembly 資訊到 CSV
        _sb.AppendLine($"Section{_splitChar}Assembly{_splitChar}Version");
        _sb.AppendLine($"Assembly Info{_splitChar}{EscapeCsvField(assemblyName)}{_splitChar}{assemblyVersion}");

        // Assembly 度量數據
        _sb.AppendLine("Section;Metric Name;Value");
        foreach (XElement metric in assemblyElement.Element("Metrics")!.Descendants())
        {
            var name = metric.Attribute("Name")!.Value;
            var value = int.Parse(metric.Attribute("Value")!.Value.ToString());
            _sb.AppendLine($"Assembly Metrics{_splitChar}{EscapeCsvField(name)}{_splitChar}{value}");
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

    void AddTypeInfo(TypeMetrics[] metricsByType)
    {

        // 類型度量數據的 CSV 表頭
        _sb.AppendLine($"Section{_splitChar}Type{_splitChar}Maintainability{_splitChar}Complexity{_splitChar}ClassCoupling{_splitChar}Lines of Code{_splitChar}Lines of Executable Code");

        // 每個類型的度量數據
        foreach (TypeMetrics metrics in metricsByType)
        {
            _sb.AppendLine($"Type Metrics{_splitChar}{EscapeCsvField(metrics.FullTypeName)}{_splitChar}" +
                          $"{metrics.MaintainabilityIndex}{_splitChar}{metrics.CyclomaticComplexity}{_splitChar}" +
                          $"{metrics.ClassCoupling}{_splitChar}{metrics.SourceLines}{_splitChar}{metrics.ExecutableLines}");
        }
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
                        var memberType = memberElement.Name.LocalName; // Method、Field、Property

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
        // 一個臨时的 XElement 來模擬 TypeMetrics 的建構子輸入
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
            memberType // 將成員類型儲存在一個額外的欄位 (需要在 TypeMetrics 中新增)
        );
    }

    void AddMemberInfoAsTypeMetrics(List<TypeMetrics> memberMetrics)
    {
        // CSV 表頭
        _sb.AppendLine($"Name{_splitChar}Type{_splitChar}Container Typepe{_splitChar}Maintainability{_splitChar}Complexity{_splitChar}ClassCoupling{_splitChar}Lines of Code{_splitChar}Lines of Executable Code");

        // 每個類型的度量數據
        foreach (var memberMetric in memberMetrics)
        {
            _sb.AppendLine($"{EscapeCsvField(memberMetric.TypeName)}{_splitChar}{EscapeCsvField(memberMetric.MemberType)}{_splitChar}{EscapeCsvField(memberMetric.TypeNamespace)}{_splitChar}" +
                          $"{memberMetric.MaintainabilityIndex}{_splitChar}{memberMetric.CyclomaticComplexity}{_splitChar}" +
                          $"{memberMetric.ClassCoupling}{_splitChar}{memberMetric.SourceLines}{_splitChar}{memberMetric.ExecutableLines}");
        }
    }

    public void SaveAs(string path)
    {
        path = Path.GetFullPath(path);
        File.WriteAllText(path, _sb.ToString());
        Console.WriteLine($"Saved: {path}");
    }
}

