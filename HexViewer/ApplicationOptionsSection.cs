using System.Configuration;

namespace HexViewer
{
    public sealed class ApplicationOptionsSection : ConfigurationSection
    {
        [ConfigurationProperty("ViewsOptions")]
        public ViewsOptionsElement ViewsOptions
        {
            get => (ViewsOptionsElement)this["ViewsOptions"];
            set => this["ViewsOptions"] = value;
        }

        public class ViewsOptionsElement : ConfigurationElement
        {
            [ConfigurationProperty("currentView", DefaultValue = ViewOption.AvaloniaUI)]
            public ViewOption CurrentView
            {
                get => (ViewOption)this["currentView"];
                set => this["currentView"] = value;
            }
        }
    }
}