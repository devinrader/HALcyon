using System;

namespace HALcyon
{
    public class EmbeddedAttribute : Attribute
    {
        public EmbeddedAttribute(string name)
        {
            this.Name = name;
            this.Template = string.Empty;
        }

        public EmbeddedAttribute(string name, string linkTemplate)
        {
            this.Name = name;
            this.Template = Template;
        }

        public string Name { get; set; }
        public string Template {get;set;}

    }
}
