using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor.Graph;

internal class GraphBuilder
{
    private readonly string[] Colors = { "#88F7E2", "#44D492", "#F5EB67", "#FFA15C", "#FA233E" };
    private readonly string serviceColor = "#48cae4";

    public string Generate(IEnumerable<Service> services)
    {
        var sb = new IndentedStringBuilder();

        WriteHeader(sb);

        WriteServices(sb, services);

        WriteRelations(sb, services);

        //WriteLegend(sb, services);

        WriteFooter(sb);

        return sb.ToString();
    }

    private void WriteFooter(IndentedStringBuilder sb)
    {
        sb.DecrementIndent().AppendLine("}");
    }

    private void WriteRelations(IndentedStringBuilder sb, IEnumerable<Service> services)
    {
        HashSet<string> duplicates = new HashSet<string>();
        sb.AppendLine()
            .AppendLine("// Dependencies");

        foreach (var service in services.Where(s => s.Dependencies.Count > 0))
        {
            foreach (var serviceDependency in service.Dependencies)
            {
                switch (serviceDependency.Type)
                {
                    case "Azure Service Bus":
                        var direction = serviceDependency.Direction == Direction.Receiving ? "back" : "forward";
                        var operation = serviceDependency.Direction == Direction.Receiving ? "Process" : "Send";
                        sb.Append($"{Quote(service.Name)} -> {Quote(serviceDependency.Target)}")
                            .Append("[")
                            .Append($"""color="{GetColor(serviceDependency.Type)}", label="{serviceDependency.Type} ({operation})", """)
                            .Append($"dir={direction}")
                            .Append("]")
                            .AppendLine();
                        break;

                    case "HTTP":
                        sb.Append($"{Quote(service.Name)} -> {Quote(serviceDependency.Target)}")
                            .Append($"""[color="{GetColor(serviceDependency.Type)}", label="{serviceDependency.Kind} - {serviceDependency.Type}"]""")
                            .AppendLine();

                        // Special case api management
                        var lastIndexOfSlash = serviceDependency.Target.LastIndexOf('/');
                        if (lastIndexOfSlash > 0)
                        {
                            var mayBeAService = serviceDependency.Target.Substring(lastIndexOfSlash + 1);
                            var targetService = services.FirstOrDefault(s => s.Name == mayBeAService);
                            if (targetService != null && duplicates.Add(serviceDependency.Target + targetService.Name))
                            {
                                sb.Append($"{Quote(serviceDependency.Target)} -> {Quote(targetService.Name)}")
                                    .Append($"""[color="{GetColor(serviceDependency.Type)}", label="{serviceDependency.Kind} - {serviceDependency.Type}"]""")
                                    .AppendLine();
                            }
                        }

                        break;

                    default:
                        sb.Append($"{Quote(service.Name)} -> {Quote(serviceDependency.Target)}")
                            .Append($"""[color="{GetColor(serviceDependency.Type)}", label="{serviceDependency.Type}"]""")
                            .AppendLine();
                        break;
                }

            }
        }
    }

    private string GetColor(string value) =>
        Colors[Math.Abs(value.GetHashCode()) % Colors.Length];

    private void WriteServices(IndentedStringBuilder sb, IEnumerable<Service> services)
    {
        sb.AppendLine()
            .AppendLine("// Services");

        foreach (var service in services)
        {
            sb.Append(Quote(service.Name)).AppendLine($"""[color="{serviceColor}", tooltip="service"]""");
        }
    }

    private string Quote(string input) => $"\"{input}\"";

    private void WriteHeader(IndentedStringBuilder sb)
    {
        sb.AppendLine("digraph service_dependencies {")
            .IncrementIndent()
            .AppendLine()
            .AppendLine("fontname=\"Helvetica,Arial,sans-serif\"")
            .AppendLine("node [fontname=\"Helvetica,Arial,sans-serif\"]")
            .AppendLine("edge [fontname=\"Helvetica,Arial,sans-serif\"]")
            .AppendLine("node [shape=box, style=filled]")
            .AppendLine("rankdir=LR");
    }
}