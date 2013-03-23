using System;

namespace HALcyon
{
    public class LinkAttribute : Attribute
    {
        public LinkAttribute(string name, string template)
        {
            this.Name = name;
            this.Template = template;
        }

        public string Template { get; set; }
        public string Name { get; set; }
    }
}
