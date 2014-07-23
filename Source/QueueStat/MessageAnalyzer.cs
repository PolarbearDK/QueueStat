using System;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using NServiceBus.Transports.Msmq;

namespace QueueStat
{
	class MessageAnalyzer
	{
		private readonly MessageQueue _queue;
		private readonly bool _idDebug;
		public DateTime AnalyzeStartDateTime { get; private set; }
		public DateTime AnalyzeEndDateTime { get; private set; }
		public string Queue { get { return _queue.Path; } }
		readonly MessageStatistics _messageStatistics = new MessageStatistics();

		public MessageAnalyzer(MessageQueue messageQueue, bool idDebug)
		{
			_queue = messageQueue;
			_idDebug = idDebug;
			_queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
			_queue.MessageReadPropertyFilter.Label = true;
			_queue.MessageReadPropertyFilter.Extension = true;
		}

		public int AnalyzeMessages()
		{
			AnalyzeStartDateTime = DateTime.Now;
			int count = 0;
			Cursor cursor = _queue.CreateCursor();

			for (
				Message m = PeekWithoutTimeout(_queue, cursor, PeekAction.Current);
				m != null;
				m = PeekWithoutTimeout(_queue, cursor, PeekAction.Next))
			{
				count++;

				if(_idDebug) Console.WriteLine(m.Id);

				AnalyzeLabel(m.Label);
				AnalyzeBody(m);
				AnalyzeExtension(m);
				AddStatistics(StatisticsType.ResponseQueue, m.ResponseQueue);
			}
			AnalyzeEndDateTime = DateTime.Now;
			return count;
		}

		private static Message PeekWithoutTimeout(MessageQueue mq, Cursor cursor, PeekAction action)
		{
			Message message = null;
			try
			{
				message = mq.Peek(new TimeSpan(1), cursor, action);
			}
			catch (MessageQueueException mqe)
			{
				if (!mqe.Message.ToLower().Contains("timeout"))
				{
					throw;
				}
			}
			return message;
		}

		private void AnalyzeLabel(string label)
		{
			string failedQueue = GetFailedQueue(label);

			AddStatistics(StatisticsType.Queue, GetQueue(failedQueue));
			AddStatistics(StatisticsType.FailedQueue, failedQueue);
			AddStatistics(StatisticsType.Machine, GetMachine(failedQueue));
		}

		private string GetQueue(string failedQueue)
		{
			if (failedQueue == null) return null;
			var pos = failedQueue.IndexOf('@');
			return
				pos >= 0
					? failedQueue.Substring(0, pos)
					: null;
		}

		private string GetMachine(string failedQueue)
		{
			if (failedQueue == null) return null;
			var pos = failedQueue.IndexOf('@');
			return
				pos >= 0
					? failedQueue.Substring(pos+1)
					: null;
		}

		private void AnalyzeBody(Message m)
		{
			if (m.BodyStream.Length > 20)
			{
				using (var reader = XmlReader.Create(m.BodyStream))
				{
					reader.MoveToContent();
					if (reader.IsStartElement("Messages"))
					{
						reader.Read();
						reader.MoveToContent();

						AddStatistics(StatisticsType.Uri, reader.NamespaceURI);
						AddStatistics(StatisticsType.MessageType, reader.NamespaceURI + "." + reader.LocalName);
					}
				}
			}
		}

		private void AnalyzeExtension(Message message)
		{
			var bytes = message.Extension;
			if (bytes.Length > 20)
			{
				using (var mr = new MemoryStream(bytes))
				{
					var ser = new XmlSerializer(typeof(HeaderInfo[]));
					var headers = (HeaderInfo[])ser.Deserialize(mr);

					AddStatistics(StatisticsType.ExceptionType, headers.SingleOrDefault(x => x.Key == "NServiceBus.ExceptionInfo.ExceptionType"));
					AddStatistics(StatisticsType.ExceptionSource, headers.SingleOrDefault(x => x.Key == "NServiceBus.ExceptionInfo.Source"));
					AddStatistics(StatisticsType.ExceptionMessage, headers.SingleOrDefault(x => x.Key == "NServiceBus.ExceptionInfo.Message"));
					AddStatistics(StatisticsType.StackTrace, headers.SingleOrDefault(x => x.Key == "NServiceBus.ExceptionInfo.StackTrace"));
				}
			}
		}

		private static readonly Regex FailedQueueRegex = new Regex(@".*\<FailedQ\>(?<FailedQ>.*)\<\/FailedQ\>.*", RegexOptions.Compiled);

		private string GetFailedQueue(string label)
		{
			var matches = FailedQueueRegex.Matches(label);

			return matches.Count == 1
					   ? matches[0].Groups["FailedQ"].Value
					   : null;
		}

		private void AddStatistics(StatisticsType type, string value)
		{
			if (value != null)
				_messageStatistics.Add(type, value);
		}

		private void AddStatistics(StatisticsType type, HeaderInfo headerInfo)
		{
			if (headerInfo != null)
				AddStatistics(type, headerInfo.Value);
		}

		private void AddStatistics(StatisticsType type, MessageQueue messageQueue)
		{
			if (messageQueue != null)
				AddStatistics(type, messageQueue.Path);
		}

		public Statistics[] GetStatistics()
		{
			return _messageStatistics.GetAll().ToArray();
		}

		public void WriteStatistics(XmlWriter writer)
		{
			_messageStatistics.WriteStatistics(writer);
		}
	}
}

