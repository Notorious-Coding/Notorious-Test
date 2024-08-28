using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Web.Applications
{
    public interface IConfigurableApplication
    {
        public Dictionary<string, string>? Configuration { get; set; } 
    }
}
