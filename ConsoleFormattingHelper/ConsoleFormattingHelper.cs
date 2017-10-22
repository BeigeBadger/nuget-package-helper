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
		public const string SuccessfullyContactedServerMessageText = "Successfully contacted the server using the URL provided";

		// Message templates
		public const string ExceptionMessageTemplate = "A {0} was thrown\r\nThe message was: {1}\r\nStacktrace: {2}";

		public const string InvalidFeedUrlMessageTemplate = "The provided package feed URL \r\n'{0}'\r\nis not valid.";
		public const string NumberOfPackagesFoundMessageTemplate = "{0} package/s were found.";
		public const string AttemptingToContactServerMessageTemplate = "Attempting to contact the server via '{0}'...";

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

		public static void PrintInitialBlurbMessage(string welcomeMessage, string initialBlurbMessage, string dataSourceMessage)
		{
			PrintPaddedText(textToPrint: welcomeMessage, paddingElement: PaddingElementEnum.HorizontalRule);
			PrintPaddedText(textToPrint: initialBlurbMessage, paddingElement: PaddingElementEnum.BlankLine);
			PrintHorizontalRule();
			PrintPaddedText(textToPrint: dataSourceMessage, paddingElement: PaddingElementEnum.BlankLine);
			PrintHorizontalRule();
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
		public static string WaitForUserInput()
		{
			string userInput;

			do
			{
				userInput = Console.ReadLine().Trim();
			}
			while (Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Enter);

			return userInput;
		}

		#endregion Inputting
	}
}