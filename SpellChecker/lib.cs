namespace SpellChecker {
	using LangsLib;
	using STALib;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using System.Windows.Documents;
	using System.Windows.Markup;

	//STA runner
	public class RunSpellCheckWords : RunObject<List<int>> {

		public TaskCompletionSource<List<int>> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunSpellCheckWords(Langs lng, IEnumerable<WordIdx> wordIdx) {
			this.lng = lng; this.wordIdx = wordIdx;
		}

		Langs lng; IEnumerable<WordIdx> wordIdx;
		const int maxWordLen = 48;

		static List<int> doRun(Langs lang, IEnumerable<WordIdx> wordIdx) {
			if (wordIdx == null) return null;
			List<int> wrongIdxs = null;
			TextBox tb = null;
			foreach (var wi in wordIdx) {
				var isError = false;
				if (wi.word.Length > maxWordLen) //too long word => error
					isError = true;
				else { //else check TextBox
					if (tb == null && !textBoxes.TryGetValue(lang, out tb)) {
						tb = new TextBox(); tb.SpellCheck.IsEnabled = true; tb.Language = XmlLanguage.GetLanguage(Metas.lang2string(lang));
						textBoxes.Add(lang, tb);
					}
					tb.Text = wi.word;
					if (tb.GetNextSpellingErrorCharacterIndex(0, LogicalDirection.Forward) != -1) isError = true;
				}
				if (isError) {
					if (wrongIdxs == null) wrongIdxs = new List<int>();
					wrongIdxs.Add(wi.idx);
				}
			}
			//if (wrongIdxs==null)
			//return wrongIdxs;
			return wrongIdxs;
		}

		public List<int> Run() {
			return doRun(lng, wordIdx);
		}

		static Dictionary<Langs, TextBox> textBoxes = new Dictionary<Langs, TextBox>();

		public static Task<List<int>> Check(Langs lang, IEnumerable<WordIdx> words) {
			return Lib.Run(new RunSpellCheckWords(lang, words));
		}

		public static List<int> STACheck(Langs lng, IEnumerable<WordIdx> wordIdx) {
			return doRun(lng, wordIdx);
		}

		}

	public struct WordIdx : IEqualityComparer<WordIdx> {
		public string word; public int idx;

		bool IEqualityComparer<WordIdx>.Equals(WordIdx x, WordIdx y) { return x.word.Equals(y.word); }
		int IEqualityComparer<WordIdx>.GetHashCode(WordIdx obj) { return obj.word.GetHashCode(); }
	}

	//public class RunSpellCheck : RunObject {

	//	public RunSpellCheck(Langs lng, IEnumerable<string> words) {
	//		this.lng = lng; this.words = words;
	//	}

	//	Langs lng; IEnumerable<string> words;

	//	public override object Run() {
	//		if (words == null) return null;
	//		SpellLangResult res = null; int errLen; int wordIdx = 0;
	//		TextBox tb = null;
	//		foreach (var text in words) {
	//			if (tb == null && !textBoxes.TryGetValue(lng, out tb)) {
	//				tb = new TextBox(); tb.SpellCheck.IsEnabled = true; tb.Language = XmlLanguage.GetLanguage(LangsLib.Metas.lang2string(lng));
	//				textBoxes.Add(lng, tb);
	//			}
	//			tb.Text = text; int index = 0;
	//			while ((index = tb.GetNextSpellingErrorCharacterIndex(index, LogicalDirection.Forward)) != -1) {
	//				if (res == null) res = new SpellLangResult();
	//				res.Add(new TPosLen { idx = wordIdx, pos = index, len = errLen = tb.GetSpellingErrorLength(index) });
	//				index += errLen;
	//				//break;
	//			}
	//			wordIdx++;
	//		}
	//		return res;
	//	}

	//	static Dictionary<Langs, TextBox> textBoxes = new Dictionary<Langs, TextBox>();

	//	public static Task<Object> Check(Langs lang, IEnumerable<string> words) {
	//		var res = STALib.Lib.Run(new RunSpellCheck(lang, words)) as Task<Object>;
	//		return res;
	//	}

	//}

	public struct TPosLen { public int idx; public int pos; public int len; }
	//public class SpellLangResult : List<TPosLen> { }

	//public class SpellLang {

	//	static SpellLang() {
	//		//Thread t = new Thread(() => {
	//		//	var tb = new TextBox();
	//		//	tb.SpellCheck.IsEnabled = true;
	//		//	tb.Language = XmlLanguage.GetLanguage("cs-CZ");
	//		//	while (true) {
	//		//		var data = checkThread.Take();
	//		//		if (data == null) break;
	//		//		tb.Text = data;
	//		//	}
	//		//	tb = null;
	//		//	//BlockingCollection<>
	//		//});
	//		//t.SetApartmentState(ApartmentState.STA);
	//		////t.Priority = ThreadPriority.Lowest;
	//		//t.Start();
	//	}


	//	//STA: https://weblog.west-wind.com/posts/2012/sep/18/creating-sta-com-compatible-aspnet-applications
	//	//https://msdn.microsoft.com/en-us/library/dd997371.aspx
	//	static BlockingCollection<string> checkThread = new BlockingCollection<string>();

	//	public static Task<Object> Check(Langs lang, string[] words) {
	//		var res = STALib.Lib.Run(new RunSpellCheck(lang, words)) as Task<Object>;
	//		return res;
	//	}
	//	public SpellLang(Langs lang) {
	//		tb = new TextBox();
	//		tb.SpellCheck.IsEnabled = true;
	//		tb.Language = XmlLanguage.GetLanguage(Metas.lang2string(lang));
	//	}

	//	TextBox tb;

	//	SpellLangResult check(string[] words) {
	//		if (words == null || words.Length == 0 || words[0] == null) return null;
	//		SpellLangResult res = null; int errLen; int wordIdx = 0;
	//		lock (tb) {
	//			foreach (var text in words) {
	//				tb.Text = text; int index = 0;
	//				while ((index = tb.GetNextSpellingErrorCharacterIndex(index, LogicalDirection.Forward)) != -1) {
	//					if (res == null) res = new SpellLangResult();
	//					res.Add(new TPosLen { idx = wordIdx, pos = index, len = errLen = tb.GetSpellingErrorLength(index) });
	//					index += errLen;
	//				}
	//				wordIdx++;
	//			}
	//		}
	//		return res;
	//	}

	//	static Dictionary<Langs, SpellLang> spellLangs = new Dictionary<Langs, SpellLang>();

	//	public static SpellLangResult Check_(Langs lang, string[] words) {
	//		var task = new TaskCompletionSource<SpellLangResult>();
	//		if (!Metas.SpellCheckLangs.Contains(lang)) { task.TrySetResult(null); return null; }
	//		Thread t = new Thread(() => {
	//			SpellLang sl = new SpellLang(lang);
	//			//if (!spellLangs.TryGetValue(lang, out sl))
	//			//	lock (typeof(SpellLang))
	//			//		if (!spellLangs.TryGetValue(lang, out sl))
	//			//			spellLangs.Add(lang, sl = new SpellLang(lang));
	//			task.TrySetResult(sl.check(words));
	//			//task.TrySetResult(null);
	//			//task.TrySetResult(null);
	//		});
	//		t.SetApartmentState(ApartmentState.STA);
	//		t.Priority = ThreadPriority.Lowest;
	//		t.Start();
	//		t.Join();
	//		return null;
	//	}

	//	public static SpellLangResult Check(Langs lang, string text) {
	//		return null; // Check(lang, new string[] { text });
	//	}

	//	public static async Task test() {
	//		//var src = new string[] { "Ahoj", "Verčo", "jak", "se", "máš" };
	//		//var res = await Check(Langs.cs_cz, src);
	//		//var errors = res == null ? null : res.Select(pl => src[pl.idx].Substring(pl.pos, pl.len)).Aggregate((r, i) => r + ", " + i);
	//		//errors = null;
	//	}


	//	//https://blogs.msdn.microsoft.com/wpf/2015/10/29/wpf-in-net-4-6-1/
	//	void v(TextBox TextInput, string actLang) {
	//		//string actLang = TextInput.Language.IetfLanguageTag == "en-gb" ? otherLang : "en-gb";
	//		lock (TextInput) {
	//			TextInput.Language = XmlLanguage.GetLanguage(actLang);

	//			int index = 0;

	//			List<string> suggestions = new List<string>();

	//			while ((index = TextInput.GetNextSpellingErrorCharacterIndex(index, LogicalDirection.Forward)) != -1) {

	//				string currentError = TextInput.Text.Substring(index, TextInput.GetSpellingErrorLength(index));


	//				suggestions.Add(currentError);

	//				foreach (string suggestion in TextInput.GetSpellingError(index).Suggestions) {

	//					suggestions.Add(suggestion);

	//				}

	//				//spellingErrors.Add(index, suggestions.ToArray());

	//				index += currentError.Length;

	//			}

	//			var Errors = suggestions.DefaultIfEmpty().Aggregate((r, i) => r + "\r\n" + i);
	//		}

	//	}
	//}
}

