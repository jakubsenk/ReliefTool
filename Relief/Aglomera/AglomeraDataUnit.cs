using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefLib.Aglomera
{
	internal class AglomeraDataUnit : DataUnit, IComparable<AglomeraDataUnit>
	{
		public List<int> CalcIndexes { get; } = new List<int>();
		private Dictionary<AglomeraDataUnit, int> comparations = new Dictionary<AglomeraDataUnit, int>();

		public AglomeraDataUnit(List<int> calcIndexes)
		{
			this.CalcIndexes.AddRange(calcIndexes);
		}

		public AglomeraDataUnit()
		{
			
		}

		public int CompareTo(AglomeraDataUnit other)
		{
			if (comparations.ContainsKey(other))
			{
				return comparations[other];
			}

			if (Columns.Count != other.Columns.Count)
			{
				throw new InvalidOperationException("Unable to compare units with different column count");
			}
			for (int i = 0; i < CalcIndexes.Count; i++)
			{
				if (Columns[CalcIndexes[i]] != other.Columns[CalcIndexes[i]])
				{
					comparations.Add(other, (int)((other.Columns[CalcIndexes[i]].NumericValue - Columns[CalcIndexes[i]].NumericValue) * 1000));
					return comparations[other];
				}
			}
			return 0;
		}
	}
}
