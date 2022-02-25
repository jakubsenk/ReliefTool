using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public class DataPreparatorOptions
	{
		public bool SkipFirstLine { get; set; } = false;
		public char ColumnSeparator { get; set; } = ',';
		public bool ResultClassIsFirstColumn { get; set; } = false;
		public bool Normalize { get; set; } = false;
	}
}
