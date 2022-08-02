using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public class PreparatorResult
	{
		public List<DataUnit> Data { get; private set; }

		public List<string> Keys { get; private set; }

		internal PreparatorResult(List<DataUnit> data, List<string> keys)
		{
			Data = data;
			Keys = keys;
		}
	}
}
