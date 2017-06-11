namespace SpellChecker {
	using LangsLib;
	using STALib;
	using System.Collections.Generic;
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
			HashSet<string> freq = Frequency.forLangs.Contains(lang) ? Frequency.isWord[lang] : null;
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

		public static Task<List<int>> Check(Langs lang, IEnumerable<WordIdx> words) {
			return Lib.Run(new RunSpellCheckWords(lang, words));
		}

		public static List<int> STACheck(Langs lng, IEnumerable<WordIdx> wordIdx) {
			return doRun(lng, wordIdx);
		}

	}

	public struct WordIdx : IEqualityComparer<WordIdx> {
		public string fullWord; public string word; public int idx;

		bool IEqualityComparer<WordIdx>.Equals(WordIdx x, WordIdx y) { return x.word.Equals(y.word); }
		int IEqualityComparer<WordIdx>.GetHashCode(WordIdx obj) { return obj.word.GetHashCode(); }
	}

	public struct TIdxPosLen { public int idx; public int pos; public int len; }
}

