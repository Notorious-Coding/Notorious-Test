using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Common.Configuration
{
    /// <summary>
    /// Make a class able to produce or consume configuration
    /// </summary>
    /// <typeparam name="T">Configuration type.</typeparam>
    public interface IConfigurable<T> where T: class
    {
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public T Configuration { get; set; }
    }

    ///<inheritdoc/>
    public interface IConfigurable : IConfigurable<Dictionary<string, string>>
    {
    }
}
