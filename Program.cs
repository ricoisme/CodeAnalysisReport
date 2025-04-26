using CodeAnalysisReport;

if (args.Length < 2)
{
    Console.WriteLine("請提供兩個參數：輸入XML路徑 和 輸出HTM/CSV路徑");
    return;
}

var inputXmlPath = args[0];
var outputHtmlPath = args[1];
var extstion = Path.GetExtension(outputHtmlPath);

if (extstion.StartsWith(".htm", StringComparison.OrdinalIgnoreCase))
{
    var report = new HtmlReportBuilder(inputXmlPath);
    report.SaveAs(outputHtmlPath);
}
else
{
    var report = new CsvReportBuilder(inputXmlPath);
    report.SaveAs(outputHtmlPath);
}


