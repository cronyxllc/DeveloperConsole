using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
    /// <summary>
    /// An exception that is thrown when a <see cref="ParameterParser{T}"/> for a given type cannot be found or created.
    /// </summary>
	public class ParserNotFoundException : Exception
	{
        public ParserNotFoundException() { }

        public ParserNotFoundException(string message) : base(message) { }

        public ParserNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
