namespace SpellChecker {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Documents;
	using System.Windows.Markup;

	public struct TPosLen { public int idx; public int pos; public int len; }
	public class SpellLangResult : List<TPosLen> { }

	public class SpellLang {

		public SpellLang(string lang) {
			tb = new TextBox();
			tb.SpellCheck.IsEnabled = true;
			tb.Language = XmlLanguage.GetLanguage(lang);
		}

		TextBox tb;

		SpellLangResult check(string[] words) {
			if (words == null || words.Length == 0 || words[0] == null) return null;
			SpellLangResult res = null; int errLen; int wordIdx = 0;
			lock (tb) {
				foreach (var text in words) {
					tb.Text = text; int index = 0;
					while ((index = tb.GetNextSpellingErrorCharacterIndex(index, LogicalDirection.Forward)) != -1) {
						if (res == null) res = new SpellLangResult();
						res.Add(new TPosLen { idx = wordIdx, pos = index, len = errLen = tb.GetSpellingErrorLength(index) });
						index += errLen;
					}
					wordIdx++;
				}
			}
			return res;
		}

		static Dictionary<string, SpellLang> spellLangs = new Dictionary<string, SpellLang>();

		public static void Check(string lang, string[] words, Action<SpellLangResult> callback) {
			Thread t = new Thread(() => {
				SpellLang sl;
				if (!spellLangs.TryGetValue(lang, out sl))
					lock (typeof(SpellLang))
						if (!spellLangs.TryGetValue(lang, out sl))
							spellLangs.Add(lang, sl = new SpellLang(lang));
				callback(sl.check(words));
			});
			t.SetApartmentState(ApartmentState.STA);
			t.Start();
		}

		public static void Check(string lang, string text, Action<SpellLangResult> callback) {
			Check(lang, new string[] { text }, callback);
		}

		public static void test() {
			var src = new string[] { "Ahoj", "Verčo", "jak", "se", "máš" };
			Check("cs-cz", src, res => {
				var errors = res == null ? null : res.Select(pl => src[pl.idx].Substring(pl.pos, pl.len)).Aggregate((r, i) => r + ", " + i);
				errors = null;
			});
		}


		//https://blogs.msdn.microsoft.com/wpf/2015/10/29/wpf-in-net-4-6-1/
		void v(TextBox TextInput, string actLang) {
			//string actLang = TextInput.Language.IetfLanguageTag == "en-gb" ? otherLang : "en-gb";
			lock (TextInput) {
				TextInput.Language = XmlLanguage.GetLanguage(actLang);

				int index = 0;

				List<string> suggestions = new List<string>();

				while ((index = TextInput.GetNextSpellingErrorCharacterIndex(index, LogicalDirection.Forward)) != -1) {

					string currentError = TextInput.Text.Substring(index, TextInput.GetSpellingErrorLength(index));


					suggestions.Add(currentError);

					foreach (string suggestion in TextInput.GetSpellingError(index).Suggestions) {

						suggestions.Add(suggestion);

					}

					//spellingErrors.Add(index, suggestions.ToArray());

					index += currentError.Length;

				}

				var Errors = suggestions.DefaultIfEmpty().Aggregate((r, i) => r + "\r\n" + i);
			}

		}
	}
}

