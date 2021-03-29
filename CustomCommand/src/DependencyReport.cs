using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCommandSample
{
    public class DependencyReport
    {
        public string FileName { get; set; }

        public List<Dependency> Dependencies { get; set; }

        public string Type { get; set; }
    }
}
