using System;
using System.Collections.Generic;

namespace ConsoleFormattingHelper
{
	public static class ConsoleHelper
	{
		// Message constants
		private const string HorizontalRule = "====================================================================================================================";

		public const string ListItemDecorator = "\r\n * ";
		public const string RestartApplicationMessageText = "Please restart the application and try again.";

		// Message templates
		private const string ExceptionMessageTemplate = "A {0} was thrown\r\nThe message was: {1}\r\nStacktrace: {2}";

		#region Formatting

		public static void PrintEmptyLine()
		{
			PrintText(string.Empty);
		}

		public static void PrintHorizontalRule()
		{
			PrintText(HorizontalRule);
		}

		public static void PrintPaddedText(bool padBefore = true, bool padAfter = true, string textToPrint = HorizontalRule, PaddingElementEnum paddingElement = PaddingElementEnum.BlankLine)
		{
			if (padBefore)
			{
				if (paddingElement == PaddingElementEnum.BlankLine)
					PrintEmptyLine();
				else if (paddingElement == PaddingElementEnum.HorizontalRule)
					PrintHorizontalRule();
			}

			PrintText(textToPrint);

			if (padAfter)
			{
				if (paddingElement == PaddingElementEnum.BlankLine)
					PrintEmptyLine();
				else if (paddingElement == PaddingElementEnum.HorizontalRule)
					PrintHorizontalRule();
			}
		}

		public static void PromptUserForInput(string promptText, bool addEmptyLineBeforePrompt = true)
		{
			if (addEmptyLineBeforePrompt)
				PrintEmptyLine();

			PrintText(promptText);
		}

		public static void PrintText(string messageToPrint, params string[] messageParameters)
		{
			Console.WriteLine(messageToPrint, messageParameters);
		}

		public static void PrintTextFollowedByEmptyLine(string messageToPrint, params string[] messageParameters)
		{
			PrintText(messageToPrint, messageParameters);
			PrintEmptyLine();
		}

		public static void PrintTextFollowedByHorizontalRule(string messageToPrint, params string[] messageParameters)
		{
			PrintText(messageToPrint, messageParameters);
			PrintHorizontalRule();
		}

		public static void PrintTextSurroundedByHorizontalRules(string messageToPrint, params string[] messageParameters)
		{
			PrintHorizontalRule();
			PrintText(messageToPrint, messageParameters);
			PrintHorizontalRule();
		}

		public static void PrintErrorMessageThenHalt(string errorMessage, params string[] errorParameters)
		{
			PrintEmptyLine();
			PrintHorizontalRule();

			Console.ForegroundColor = ConsoleColor.Red;

			PrintText(errorMessage, errorParameters);
			Console.ReadLine();

			// Reset the text colour
			Console.ForegroundColor = ConsoleColor.White;

			PrintHorizontalRule();
			PrintEmptyLine();
		}

		public static void PrintList(List<string> listToPrint, string listSeparator)
		{
			PrintText($"{listSeparator}{string.Join(listSeparator, listToPrint)}");
		}

		#endregion Formatting

		#region Inputting

		// TODO: Make this generic and move to own class
		public static string WaitForUserPackageFeedUrlInput()
		{
			string nugetFeedUrl;

			do
			{
				nugetFeedUrl = Console.ReadLine().Trim();
			}
			while (string.IsNullOrWhiteSpace(nugetFeedUrl));

			PrintEmptyLine();

			return nugetFeedUrl;
		}

		// TODO: Move to own class
		public static string WaitForUserPackageIdInput()
		{
			string nugetPackageId;

			do
			{
				nugetPackageId = Console.ReadLine().Trim();
			}
			while (Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Enter);

			return nugetPackageId;
		}

		#endregion Inputting
	}
}