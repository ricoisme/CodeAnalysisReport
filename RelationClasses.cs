


public sealed class Rootobject
{
    public Xml xml { get; set; }
    public Codemetricsreport CodeMetricsReport { get; set; }
}

public sealed class Xml
{
    public string version { get; set; }
    public string encoding { get; set; }
}

public sealed class Codemetricsreport
{
    public string Version { get; set; }
    public Targets Targets { get; set; }
}

public sealed class Targets
{
    public Target Target { get; set; }
}

public sealed class Target
{
    public string Name { get; set; }
    public Assembly Assembly { get; set; }
}

public sealed class Assembly
{
    public string Name { get; set; }
    public Metrics Metrics { get; set; }
    public Namespaces Namespaces { get; set; }
}

public sealed class Metrics
{
    public Metric[] Metric { get; set; }
}

public sealed class Metric
{
    public string Name { get; set; }
    public string Value { get; set; }
}

public sealed class Namespaces
{
    public Namespace[] Namespace { get; set; }
}

public sealed class Namespace
{
    public string Name { get; set; }
    public Metrics1 Metrics { get; set; }
    public Types Types { get; set; }
}

public sealed class Metrics1
{
    public Metric1[] Metric { get; set; }
}

public sealed class Metric1
{
    public string Name { get; set; }
    public string Value { get; set; }
}

public sealed class Types
{
    public object NamedType { get; set; }
}
