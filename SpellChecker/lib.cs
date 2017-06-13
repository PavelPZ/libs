namespace SpellChecker {
	using LangsLib;
	using STALib;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using System.Windows.Documents;
	using System.Windows.Markup;

	//STA runner
	public class RunSpellCheckWords : RunObject<List<int>> {

		static RunSpellCheckWords() {
			NoFrequency = ConfigurationManager.AppSettings["SpellChecker.NoFrequency"] == "true";
		}
		static bool NoFrequency = false;

		public TaskCompletionSource<List<int>> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunSpellCheckWords(Langs lng, IEnumerable<SomePhraseWord> wordIdx) {
			this.lng = lng; this.wordIdx = wordIdx;
		}

		Langs lng; IEnumerable<SomePhraseWord> wordIdx;
		const int maxWordLen = 48;

		static List<int> doRun(Langs lang, IEnumerable<SomePhraseWord> wordIdx) {
			if (wordIdx == null) return null;
			List<int> wrongIdxs = null;
			TextBox tb = null;
			HashSet<string> freq = NoFrequency ? null : (Frequency.forLangs.Contains(lang) ? Frequency.isWord[lang] : null);
			foreach (var wi in wordIdx) {
				var isError = false;
				if (wi.word.Length > maxWordLen) //too long word => error
					isError = true;
				else { //else check TextBox
					if (freq != null && freq.Contains(wi.fullWord.ToLower())) continue;//vyuziti frekvencniho slovniku
					if (!Metas.SpellCheckLangs.Contains(lang)) continue;
					if (tb == null && !textBoxes.TryGetValue(lang, out tb)) {
						tb = new TextBox(); tb.SpellCheck.IsEnabled = true; tb.Language = XmlLanguage.GetLanguage(Metas.lang2string(lang));
						textBoxes.Add(lang, tb);
					}
					tb.Text = wi.fullWord;
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

		public static Task<List<int>> Check(Langs lang, IEnumerable<SomePhraseWord> words) {
			return Lib.Run(new RunSpellCheckWords(lang, words));
		}

		public static List<int> STACheck(Langs lng, IEnumerable<SomePhraseWord> wordIdx) {
			return doRun(lng, wordIdx);
		}

	}

	public struct SomePhraseWord : IEqualityComparer<SomePhraseWord> {
		public string fullWord; /*puvodni slovo*/ public string word; /*lowercase, PhraseWord.maxWordLen*/ public int idx; /*index zdroje slova v PhraseWords.Idxs*/

		bool IEqualityComparer<SomePhraseWord>.Equals(SomePhraseWord x, SomePhraseWord y) { return x.word.Equals(y.word); }
		int IEqualityComparer<SomePhraseWord>.GetHashCode(SomePhraseWord obj) { return obj.word.GetHashCode(); }
	}

	//public struct SomePhraseWords {
	//	public PhraseWords phrase;
	//	public SomePhraseWord[] selected;
	//}

	//public struct TIdxPosLen { public int idx; public int pos; public int len; }
}

