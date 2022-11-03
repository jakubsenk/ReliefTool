using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public class ColumnValue
	{
		public double NumericValue { get; set; }
		public string StringValue { get; set; }
		public bool IsString { get; set; }

		internal static ColumnValue Clone(ColumnValue item)
		{
			ColumnValue cloned = new ColumnValue();
			cloned.NumericValue = item.NumericValue;
			cloned.StringValue = item.StringValue;
			cloned.IsString = item.IsString;
			return cloned;
		}
	}
}
