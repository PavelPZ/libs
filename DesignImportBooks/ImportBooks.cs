using BookDBModel;
using Database;
using LangsLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DesignImportBooks {
  public static class ImportBooks {
    public static void importBook(string jsonFile, StringBuilder errorLog) {
      RewiseContext ctx = new RewiseContext();
      Book book = JsonConvert.DeserializeObject<Book>(File.ReadAllText(jsonFile));
      var oldBook = ctx.Books.FirstOrDefault(b => b.Name == book.Name);
      if (oldBook != null) { ctx.Books.Remove(oldBook); ctx.SaveChanges(); }

      //get all used breakers
      var breakers = new Dictionary<byte, StemmerBreaker.Runner>();
      breakers[book.Lang] = new StemmerBreaker.Runner((Langs)book.Lang);
      foreach (var lang in book.Phrases.SelectMany(p => p.Locales).Select(l => l.Lang).Distinct())
        breakers[lang] = new StemmerBreaker.Runner((Langs)lang);


      var lessonIds = new HashSet<int>(book.Meta.Lessons.Select(l => l.Id));
      book.MetaData = JsonConvert.SerializeObject(book.Meta);
      book.Imported = DateTime.UtcNow;
      foreach (var phr in book.Phrases) {
        if (!lessonIds.Contains(phr.LessonId)) errorLog.AppendLine(string.Format("Cannot find lesson in book={0}, lessonId={1}, phrase={2}", book.Name, phr.LessonId, phr.TextJSON));
        phr.Lang = book.Lang;
        phr.TextJSON = new PhraseLib.PhraseText (breakers[phr.Lang], phr.Text).encode();
        //breakers[phr.Lang].wordBreakTyBytes(phr.TextJSON);
        foreach (var loc in phr.Locales) loc.TextJSON = new PhraseLib.PhraseText (breakers[loc.Lang], loc.Text).encode();//loc.TextIdxs = breakers[loc.Lang].wordBreakTyBytes(loc.TextJSON);
      }
      ctx.Books.Add(book);
      ctx.SaveChanges();
    }

    public static void buildFtxIndex(StringBuilder errorLog) {

    }

  }
}
