using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendData
{
    public class MessagePayload
    {
        public string? subject { get; set; }
        public string? body { get; set; }
        public string? icon { get; set; }
        public string? primarykey { get; set; }
        public string? tag { get; set; }
    }
}
