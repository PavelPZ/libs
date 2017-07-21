using System;
using System.Collections.Generic;
using System.Linq;

namespace LangsLib {

	public struct PhraseSide {
		public Langs src;
		public Langs dest;
		public Langs langOfText() { return dest == Langs._ ? src : dest; }
	}

  public class TPosLen {
    public Int16 Pos;
    public Int16 Len; //could be negative when SpellChecker error
    public string toString() { return string.Format("{0}-{1}", Pos, Len); }
		public static string fromString(IEnumerable<TPosLen> items) { return items == null ? null : items.DefaultIfEmpty().Select(it => it.toString()).Aggregate((r, i) => r + "|" + i); }

		public static Int16[] toArray(List<TPosLen> idxs) {
      var res = new Int16[idxs.Count << 1];
			for (var i = 0; i < idxs.Count; i++) { res[i << 1] = idxs[i].Pos; res[(i << 1) + 1] = (byte)idxs[i].Len; }
			return res;
		}

    public static List<TPosLen> fromArray(Int16[] bytes) {
			var res = new List<TPosLen>();
			for (var i = 0; i < (bytes.Length >> 1); i++) res.Add(new TPosLen { Pos = bytes[i << 1], Len = (sbyte)bytes[(i << 1) + 1] });
			return res;
		}

    public static byte[] toBytes(List<TPosLen> idxs) {
      var array = toArray(idxs);
      byte[] result = new byte[array.Length * sizeof(Int16)];
      Buffer.BlockCopy(array, 0, result, 0, result.Length);
      return result;
    }

    public static List<TPosLen> fromBytes(byte[] bytes) {
      Int16[] result = new Int16[bytes.Length / sizeof(Int16)];
      Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
      return fromArray(result);
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

	public struct SelectedWord : IEqualityComparer<SelectedWord> {
		public string word; //puvodni slovo (po WordBreak)
		public string ftxWord; //lowercase, PhraseWord.maxWordLen
		public int idx; //index zdroje slova v PhraseWords.Idxs

		bool IEqualityComparer<SelectedWord>.Equals(SelectedWord x, SelectedWord y) { return x.ftxWord.Equals(y.ftxWord); }
		int IEqualityComparer<SelectedWord>.GetHashCode(SelectedWord obj) { return obj.ftxWord.GetHashCode(); }
	}

	public class SelectedWords {
		public SelectedWords(PhraseWords phrase, bool correctOnly = true) {
			string pomStr;
			selected = phrase.Idxs.Where(idx => correctOnly ? idx.Len > 0 : true).Select((idx, i) => new SelectedWord { idx = i, word = pomStr = phrase.Text.Substring(idx.Pos, idx.Len), ftxWord = pomStr.Substring(0, Math.Min(idx.Len, PhraseWords.maxWordLen)).ToLower() }).ToArray();
		}
		SelectedWords() { }
		public PhraseWords phrase;
		public SelectedWord[] selected;
		public SelectedWords Except(SelectedWords s) {
			if (phrase != s.phrase) throw new Exception("phrase != s.phrase");
			return new SelectedWords { phrase = phrase, selected = selected.Except(s.selected).ToArray() };
		}
	}

}
