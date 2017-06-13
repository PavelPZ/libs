using System;
using System.Collections.Generic;
using System.Linq;

namespace LangsLib {

	public struct TPosLen {
		public byte Pos;
		public sbyte Len; //could be negative when SpellChecker error
		public string toString() { return string.Format("{0}-{1}", Pos, Len); }
		public static string fromString(IEnumerable<TPosLen> items) { return items == null ? null : items.DefaultIfEmpty().Select(it => it.toString()).Aggregate((r, i) => r + "|" + i); }

		public static byte[] toBytes(List<TPosLen> idxs) {
			var res = new byte[idxs.Count << 1];
			for (var i = 0; i < idxs.Count; i++) { res[i << 1] = idxs[i].Pos; res[(i << 1) + 1] = (byte)idxs[i].Len; }
			return res;
		}
		public static List<TPosLen> fromBytes(byte[] bytes) {
			var res = new List<TPosLen>();
			for (var i = 0; i < (bytes.Length >> 1); i++) res.Add(new TPosLen { Pos = bytes[i << 1], Len = (sbyte)bytes[(i << 1) + 1] });
			return res;
		}
	}

	//public struct TWord {
	//	public string word;
	//	public bool isWrong;
	//}

	public class PhraseWords {
		public PhraseWords(string text, List<TPosLen> idxs = null) { Text = text; Idxs = idxs; }
		public PhraseWords(PhraseWords phr) : this(phr.Text) { Idxs = phr.Idxs; }
		public const sbyte maxWordLen = 24;
		public string Text;
		public List<TPosLen> Idxs;

		//public IEnumerable<TWord> toWords() {
		//	int lastPos = 0;
		//	return Idxs.Select(idx => new TWord { isWrong = idx.Len<0, word = Text.Substring(lastPos = lastPos + idx.Pos, Math.Abs(idx.Len))});
		//}
	}

	public struct PhraseSide {
		public Langs src;
		public Langs dest;
		public Langs langOfText() { return dest == Langs._ ? src : dest; }
	}

	public struct SelectedWord : IEqualityComparer<SelectedWord> {
		public string word; //puvodni slovo (po WordBreak)
		public string ftxWord; //lowercase, PhraseWord.maxWordLen
		public int idx; //index zdroje slova v PhraseWords.Idxs
		public bool isWrong; //word SpellCheck error

		bool IEqualityComparer<SelectedWord>.Equals(SelectedWord x, SelectedWord y) { return x.ftxWord.Equals(y.ftxWord); }
		int IEqualityComparer<SelectedWord>.GetHashCode(SelectedWord obj) { return obj.ftxWord.GetHashCode(); }
	}

	public class SelectedWords {
		public SelectedWords(PhraseWords phrase, bool correctOnly) {
			string pomStr;
			selected = phrase.Idxs.Where(idx => correctOnly ? idx.Len > 0 : true).Select((idx, i) => new SelectedWord { idx = i, word = pomStr = phrase.Text.Substring(idx.Pos, idx.Len), ftxWord = pomStr.Substring(0, Math.Min(idx.Len, PhraseWords.maxWordLen)).ToLower() }).ToArray();
		}
		public PhraseWords phrase;
		public SelectedWord[] selected;
	}

}
