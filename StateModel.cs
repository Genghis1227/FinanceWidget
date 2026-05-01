using System.Collections.Generic;

namespace FinanceWidget
{
    public class WidgetConfig
    {
        public string Ticker { get; set; } = string.Empty;
        public double Left { get; set; }
        public double Top { get; set; }
        public bool KeepOnTop { get; set; } = false;
        public bool UseBetaSite { get; set; } = false;
    }

    public class AppState
    {
        public List<WidgetConfig> Widgets { get; set; } = new List<WidgetConfig>();
    }
}
