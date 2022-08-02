﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib
{
	public class DataUnit
	{
		public List<ColumnValue> Columns { get; set; } = new List<ColumnValue>();
		public string ResultClass { get; set; }

		public DataUnit(string resultClass = null)
		{
			ResultClass = resultClass;
		}
	}
}
