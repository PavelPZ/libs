namespace SpellChecker {
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows.Controls;
  using System.Windows.Documents;
  using System.Windows.Markup;

  public struct TPosLen { public int pos; public int len; }
  public class SpellLangResult : List<TPosLen> { }
  public class SpellLang {

    public SpellLang(string lang) {
      tb = new TextBox();
      tb.Language = XmlLanguage.GetLanguage(lang);
    }

    TextBox tb;

    SpellLangResult check(string text) {
      if (string.IsNullOrEmpty(text)) return null;
      SpellLangResult res = null;
      lock (tb) {
        tb.Text = text; int index = 0;
        while ((index = tb.GetNextSpellingErrorCharacterIndex(index, LogicalDirection.Forward)) != -1) {
          if (res == null) res = new SpellLangResult();
          int errLen;
          res.Add(new TPosLen { pos = index, len = errLen = tb.GetSpellingErrorLength(index) });
          index += errLen;
        }
      }
      return res;
    }

    static Dictionary<string, SpellLang> spellLangs = new Dictionary<string, SpellLang>();

    public static SpellLangResult Check (string lang, string text) {
      SpellLang sl;
      if (!spellLangs.TryGetValue(lang, out sl))
        lock (typeof(SpellLang))
          if (!spellLangs.TryGetValue(lang, out sl))
            spellLangs.Add(lang, sl = new SpellLang(lang));
      return sl.check(text);
    }

    const string otherLang = "ru-ru";

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

