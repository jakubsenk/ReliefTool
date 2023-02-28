using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataGenerator
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Enter instances count: ");
			int instances = int.Parse(Console.ReadLine());
			Console.WriteLine("Enter attributes count: ");
			int attributes = int.Parse(Console.ReadLine());
			Console.WriteLine("Enter result classes count: ");
			int classesCount = int.Parse(Console.ReadLine());

			List<char> classes = new List<char>();
			char current = 'A';
			for (int i = 0; i < classesCount; i++)
			{
				classes.Add(current);
				current = (char)(current + 1);
			}

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < attributes; i++)
			{
				sb.Append("A").Append(i).Append(",");
			}
			sb.AppendLine("Result");
			for (int i = 0; i < instances; i++)
			{
				for (int j = 0; j < attributes; j++)
				{
					sb.Append(Random.Shared.Next(0, 20000)).Append(",");
				}
				sb.AppendLine(classes[Random.Shared.Next(0, classesCount)].ToString());
			}

			string name = $"data_{instances}_{attributes}_{classesCount}.csv";
			File.WriteAllText(name, sb.ToString());
			Process.Start("explorer", "/select," + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + name);
		}
	}
}