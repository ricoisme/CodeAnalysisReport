public struct TypeMetrics
{
    public string TypeName { get; }
    public string TypeNamespace { get; }
    public string FullTypeName { get; }
    public string? MemberType { get; }

    public int MaintainabilityIndex { get; }
    public string MaintainabilityIndexColor
    {
        get
        {
            if (MaintainabilityIndex < 60) return Red;
            if (MaintainabilityIndex < 75) return Yellow;
            return Green;
        }
    }

    public int CyclomaticComplexity { get; }
    public string CyclomaticComplexityColor
    {
        get
        {
            if (CyclomaticComplexity < 50) return Green;
            if (CyclomaticComplexity < 100) return Yellow;
            return Red;
        }
    }


    public int SourceLines { get; }
    public string SourceLinesColor
    {
        get
        {
            if (SourceLines < 500) return Green;
            if (SourceLines < 1000) return Yellow;
            return Red;
        }
    }

    public int ExecutableLines { get; }
    public string ExecutableLinesColor
    {
        get
        {
            if (ExecutableLines < 500) return Green;
            if (ExecutableLines < 1000) return Yellow;
            return Red;
        }
    }

    public int ClassCoupling { get; }
    public string ClassCouplingColor
    {
        get
        {
            if (ClassCoupling < 30) return Green;
            if (ClassCoupling < 50) return Yellow;
            return Red;
        }
    }

    private static string Green => "#d1e7dd00";
    private static string Yellow => "#415f01";
    private static string Red => "#ca3505";

    public TypeMetrics(string typeName, string typeNamespace, string fullTypeName, int maintainabilityIndex, int cyclomaticComplexity, int sourceLines, int executableLines, int classCoupling, string? memberType = null)
    {
        TypeName = typeName;
        TypeNamespace = typeNamespace;
        FullTypeName = fullTypeName;
        MaintainabilityIndex = maintainabilityIndex;
        CyclomaticComplexity = cyclomaticComplexity;
        SourceLines = sourceLines;
        ExecutableLines = executableLines;
        ClassCoupling = classCoupling;
        MemberType = memberType;
    }

    public TypeMetrics(XElement namedTypeElement, string typeNamespace)
    {
        TypeNamespace = typeNamespace;
        TypeName = namedTypeElement.Attribute("Name")!.Value.ToString();
        FullTypeName = TypeNamespace + "." + TypeName;
        MemberType = null;

        foreach (XElement metric in namedTypeElement.Element("Metrics")!.Elements("Metric"))
        {
            string name = metric.Attribute("Name")!.Value.ToString();
            int value = int.Parse(metric.Attribute("Value")!.Value.ToString());

            if (name == "MaintainabilityIndex")
                MaintainabilityIndex = value;
            else if (name == "CyclomaticComplexity")
                CyclomaticComplexity = value;
            else if (name == "SourceLines")
                SourceLines = value;
            else if (name == "ExecutableLines")
                ExecutableLines = value;
            else if (name == "ClassCoupling")
                ClassCoupling = value;

        }
    }
}