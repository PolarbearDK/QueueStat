using System;
using Miracle.Arguments;

namespace QueueStat
{
	[ArgumentSettings(
		ArgumentNameComparison = StringComparison.InvariantCultureIgnoreCase,
		DuplicateArgumentBehaviour = DuplicateArgumentBehaviour.Unknown,
		StartOfArgument = new[] { '-' },
		ValueSeparator = new[] { ':' },
		ShowHelpOnArgumentErrors = false
		)]
	[ArgumentDescription("Generate NServiceBus MSMQ message statistics.")]
	public class Arguments
	{
		public Arguments()
		{
			// Defaults
			Top = 10;
		}

		[ArgumentPosition(0)]
		[ArgumentRequired]
		[ArgumentDescription("Queue names. NSB queue syntax alowed if MachineName is left blank")]
		public string[] Queues { get; set; }

		[ArgumentName("Top", "T")]
        [ArgumentDescription("Show only 'top' amount of items for each category. Default is 10.")]
		public int Top { get; set; }

        [ArgumentName("StatisticsType", "ST")]
        [ArgumentDescription("List only specified statistics types. Default is all statistics types")]
        public StatisticsType[] StatisticsTypes { get; set; }

		[ArgumentName("MachineName", "Machine", "M")]
        [ArgumentDescription("MSMQ host machine. Must be machine name, not IP. NOTE! Must use full hostname including all domain suffixes if not in same domain as executing PC")]
		public string MachineName { get; set; }

		[ArgumentName("Output", "O")]
        [ArgumentDescription("Output file to store XML summary data in. Supports macros on the format ${name:format}. Macro names include all static properties on System.DateTime or System.Environment.")]
		public string Output { get; set; }

		[ArgumentName("Help", "H", "?")]
		[ArgumentHelp]
		[ArgumentDescription("Show help")]
		public bool Help { get; set; }

		[ArgumentName("Debug", "D")]
        [ArgumentDescription("Enable special debugging features. Not for production.")]
		public bool Debug { get; set; }
	}
}