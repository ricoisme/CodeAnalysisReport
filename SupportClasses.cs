


public class Rootobject
{
    public Xml xml { get; set; }
    public Codemetricsreport CodeMetricsReport { get; set; }
}

public class Xml
{
    public string version { get; set; }
    public string encoding { get; set; }
}

public class Codemetricsreport
{
    public string Version { get; set; }
    public Targets Targets { get; set; }
}

public class Targets
{
    public Target Target { get; set; }
}

public class Target
{
    public string Name { get; set; }
    public Assembly Assembly { get; set; }
}

public class Assembly
{
    public string Name { get; set; }
    public Metrics Metrics { get; set; }
    public Namespaces Namespaces { get; set; }
}

public class Metrics
{
    public Metric[] Metric { get; set; }
}

public class Metric
{
    public string Name { get; set; }
    public string Value { get; set; }
}

public class Namespaces
{
    public Namespace[] Namespace { get; set; }
}

public class Namespace
{
    public string Name { get; set; }
    public Metrics1 Metrics { get; set; }
    public Types Types { get; set; }
}

public class Metrics1
{
    public Metric1[] Metric { get; set; }
}

public class Metric1
{
    public string Name { get; set; }
    public string Value { get; set; }
}

public class Types
{
    public object NamedType { get; set; }
}
