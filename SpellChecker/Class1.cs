namespace SpellChecker
{
	using System.Collections.Generic;
	using System.Linq;
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public class Lib
	{

		const string otherLang = "ru-ru";

		//https://blogs.msdn.microsoft.com/wpf/2015/10/29/wpf-in-net-4-6-1/
		void Check(System.Windows.Controls.TextBox TextInput)
		{
			string actLang = TextInput.Language.IetfLanguageTag == "en-gb" ? otherLang : "en-gb";
			TextInput.Language = System.Windows.Markup.XmlLanguage.GetLanguage(actLang);

			int index = 0;

			List<string> suggestions = new List<string>();

			while ((index = TextInput.GetNextSpellingErrorCharacterIndex(index, System.Windows.Documents.LogicalDirection.Forward)) != -1)
			{

				string currentError = TextInput.Text.Substring(index, TextInput.GetSpellingErrorLength(index));


				suggestions.Add(currentError);

				foreach (string suggestion in TextInput.GetSpellingError(index).Suggestions)
				{

					suggestions.Add(suggestion);

				}

				//spellingErrors.Add(index, suggestions.ToArray());

				index += currentError.Length;

			}

			var Errors = suggestions.DefaultIfEmpty().Aggregate((r, i) => r + "\r\n" + i);

		}
	}
}

