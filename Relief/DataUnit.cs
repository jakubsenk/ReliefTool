using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public class DataUnit
	{
		public List<object> Columns { get; set; } = new List<object>();
		public string ResultClass { get; set; }

		public DataUnit(string resultClass = null)
		{
			ResultClass = resultClass;
		}
	}
}
