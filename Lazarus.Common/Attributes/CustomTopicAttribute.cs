using System;

namespace Lazarus.Common.Attributes
{
    public class CustomTopicAttribute : Attribute
    {
        public string Name { get; set; }
        public CustomTopicAttribute(string topicname)
        {
            this.Name = topicname;
        }
    }
}
