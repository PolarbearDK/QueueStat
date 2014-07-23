using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace QueueStat
{
	class MessageStatistics
	{
		readonly Dictionary<StatisticsType, Dictionary<string, int>> _statistics = new Dictionary<StatisticsType, Dictionary<string, int>>();

		public void Add(StatisticsType dt, string key)
		{
			Dictionary<string, int> dict;
			if (_statistics.TryGetValue(dt, out dict))
			{
				int counter;
				if (dict.TryGetValue(key, out counter))
				{
					counter++;
				}
				else
				{
					counter = 1;
				}
				dict[key] = counter;
			}
			else
			{
				_statistics[dt] = new Dictionary<string, int> { { key, 1 } };
			}
		}

		public IEnumerable<Statistics> GetAll()
		{
			return _statistics.SelectMany(
				z => z.Value,
				(z, y) => new Statistics()
					          {
						          Type = z.Key,
						          Value = y.Key,
						          Count = y.Value
					          });
		}

		public void WriteStatistics(XmlWriter writer)
		{
			foreach (var type in _statistics)
			{
				writer.WriteStartElement(type.Key.ToString());
				foreach (var stat in type.Value.OrderByDescending(x => x.Value))
				{
					writer.WriteStartElement("Item");
					writer.WriteAttributeString("Count", stat.Value.ToString());
    				writer.WriteValue(stat.Key);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
		}
	}
}