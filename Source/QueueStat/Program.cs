using System;
using System.Linq;
using System.Messaging;
using Miracle.Arguments;

namespace QueueStat
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var arguments = args.ParseCommandLine<Arguments>();
            if (arguments == null)
                return 1;

            var backgroundColor = Console.BackgroundColor;
            var foregroundColor = Console.ForegroundColor;

            try
            {
                return Main(arguments);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception: " + ex.ToString());
                return 100;
            }
            finally
            {
                Console.BackgroundColor = backgroundColor;
                Console.ForegroundColor = foregroundColor;
            }
        }

        private static int Main(Arguments arguments)
        {
            MessageQueue[] queueList = GetMatchingQueues(arguments);
            if (!queueList.Any())
            {
                Console.Error.WriteLine("No queue matched {0}", string.Join(",", arguments.Queues));
                return 10;
            }

            using (var processor = new QueueProcessor(arguments))
            {
                processor.ProcessQueues(queueList);
            }

            return 0;
        }

        private static MessageQueue[] GetMatchingQueues(Arguments parsedArguments)
        {
            return parsedArguments.Queues.Select(queue => QueueFactory.GetQueue(parsedArguments.MachineName, queue)).ToArray();
        }
    }
}