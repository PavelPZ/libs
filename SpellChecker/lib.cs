namespace SpellChecker {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Documents;
	using System.Windows.Markup;
	using LangsLib;
	using System.Threading.Tasks;

	public struct TPosLen { public int idx; public int pos; public int len; }
	public class SpellLangResult : List<TPosLen> { }

	public class SpellLang {

		public SpellLang(Langs lang) {
			tb = new TextBox();
			tb.SpellCheck.IsEnabled = true;
			tb.Language = XmlLanguage.GetLanguage(Metas.lang2string(lang));
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

		static Dictionary<Langs, SpellLang> spellLangs = new Dictionary<Langs, SpellLang>();

		public static Task<SpellLangResult> Check(Langs lang, string[] words) {
			var task = new TaskCompletionSource<SpellLangResult>();
			if (!Metas.SpellCheckLangs.Contains(lang)) { task.TrySetResult(null); return task.Task; }
			Thread t = new Thread(() => {
				SpellLang sl;
				if (!spellLangs.TryGetValue(lang, out sl))
					lock (typeof(SpellLang))
						if (!spellLangs.TryGetValue(lang, out sl))
							spellLangs.Add(lang, sl = new SpellLang(lang));
				task.TrySetResult(sl.check(words));
			});
			t.SetApartmentState(ApartmentState.STA);
			t.Start();
			return task.Task;
		}

		public static Task<SpellLangResult> Check(Langs lang, string text) {
			return Check(lang, new string[] { text });
		}

		public static async Task test() {
			var src = new string[] { "Ahoj", "Verčo", "jak", "se", "máš" };
			var res = await Check(Langs.cs_cz, src);
			var errors = res == null ? null : res.Select(pl => src[pl.idx].Substring(pl.pos, pl.len)).Aggregate((r, i) => r + ", " + i);
			errors = null;
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

