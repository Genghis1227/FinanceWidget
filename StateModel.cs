using System.Collections.Generic;

namespace FinanceWidget
{
    public class WidgetConfig
    {
        public string Ticker { get; set; } = "JEPQ:NASDAQ";
        public double Left { get; set; }
        public double Top { get; set; }
        public bool KeepOnTop { get; set; } = true;
    }

    public class AppState
    {
        public List<WidgetConfig> Widgets { get; set; } = new List<WidgetConfig>();
    }
}
