using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReliefWeb.Models
{
	public static class GlobalStore
	{
		private static ConcurrentDictionary<string, bool> dic;

		public static bool CalculationPending
		{
			get
			{

				if (!dic.TryGetValue("calc", out bool result))
					throw new Exception("Unable to get global store value.");
				return result;
			}
			set
			{
				if (!dic.TryUpdate("calc", value, !value))
					throw new Exception("Unable to set global store value.");
			}
		}
		static GlobalStore()
		{
			dic = new ConcurrentDictionary<string, bool>();
			if (!dic.TryAdd("calc", false))
				throw new Exception("Unable to initialize global store.");
		}
	}
}