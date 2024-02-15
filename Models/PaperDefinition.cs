using Microsoft.Extensions.Configuration;

namespace BookbindingPdfMaker.Models
{
    internal class PaperDefinition
    {
        public string? Name { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float MetricWidth { get; set; }
        public float MetricHeight { get; set; }

        public PaperDefinition()
        {
            Name = "?";
        }

        public PaperDefinition(IConfigurationSection config)
        {
            if (config == null)
            {
                return;
            }

            Name = config["Name"]!.ToString();

            float.TryParse(config["Width"]!.ToString(), out var f);
            Width = f;

            float.TryParse(config["Height"]!.ToString(), out f);
            Height = f;

            float.TryParse(config["MetricWidth"]!.ToString(), out f);
            MetricWidth = f;

            float.TryParse(config["MetricHeight"]!.ToString(), out f);
            MetricHeight = f;
        }
    }
}
