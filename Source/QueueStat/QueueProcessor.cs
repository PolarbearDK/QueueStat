using System;
using System.Linq;
using System.Messaging;
using System.Xml;
using Miracle.Macros;

namespace QueueStat
{
	public class QueueProcessor : IDisposable
	{
		private readonly Arguments _arguments;
		private readonly XmlWriter writer;

		public QueueProcessor(Arguments arguments)
		{
			_arguments = arguments;

			if (!string.IsNullOrEmpty(arguments.Output))
			{
				var settings = new XmlWriterSettings()
					                             {
						                             Indent = true,
						                             IndentChars = "\t",
													 CloseOutput = true
					                             };


				var filename = arguments.Output.ExpandMacros(new{});

				writer = XmlWriter.Create(filename, settings);
				writer.WriteStartElement("root");
			}
		}

		public void Dispose()
		{
			if (writer != null)
			{
				writer.WriteEndElement();
				writer.Close();
				((IDisposable) writer).Dispose();
			}
		}

		public void ProcessQueues(MessageQueue[] queueList)
		{
			foreach (MessageQueue messageQueue in queueList)
			{
				var path = GetQueuePath(messageQueue, _arguments.MachineName);

				using (var peekQueue = new MessageQueue(path, QueueAccessMode.Peek))
				{
					ProcessQueue(peekQueue);
				}
			}
		}

		private static string GetQueuePath(MessageQueue messageQueue, string machine)
		{
			if (string.IsNullOrEmpty(machine) || machine == "localhost")
				return messageQueue.Path;

			return messageQueue.Path.Replace(
				":" + messageQueue.MachineName + "\\",
				":" + machine + "\\"
				);
		}

		private void ProcessQueue(MessageQueue mq)
		{
			ConsoleColor forg = Console.ForegroundColor;

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.Write("Queue {0}", mq.Path);

			var messageStatistics = new MessageAnalyzer(mq, _arguments.Debug);
			int count = messageStatistics.AnalyzeMessages();
			Statistics[] statistics = messageStatistics.GetStatistics();
			var crlf = new[] {'\n', '\r'};

			Console.WriteLine(" contains {0} messages", count);

			foreach (var grouping in statistics.GroupBy(x => x.Type).Where(x=> _arguments.StatisticsTypes == null || _arguments.StatisticsTypes.Contains(x.Key)))
			{
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Top {0} grouped by {1}", _arguments.Top, grouping.Key);
				foreach (Statistics entry in grouping.OrderByDescending(x => x.Count).Take(_arguments.Top))
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("{0}", entry.Count);

					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("\t{0}", entry.Value);
					if (entry.Value.IndexOfAny(crlf) >= 0)
						Console.WriteLine();
				}
			}

			Console.WriteLine();
			Console.ForegroundColor = forg;

			if (_arguments.Output != null)
			{
				WriteQueue(messageStatistics);
			}
		}

		private void WriteQueue(MessageAnalyzer ms)
		{
			writer.WriteAttributeString("Queue", ms.Queue);

			writer.WriteStartAttribute("StartDateTime");
			writer.WriteValue( ms.AnalyzeStartDateTime);
			writer.WriteEndAttribute();

			writer.WriteStartAttribute("EndDateTime");
			writer.WriteValue(ms.AnalyzeEndDateTime);
			writer.WriteEndAttribute();

			ms.WriteStatistics(writer);

		}
	}
}