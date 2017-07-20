using BookDBModel;
using Database;
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

      var lessonIds = new HashSet<int>(book.Meta.Lessons.Select(l => l.Id));
      book.MetaData = JsonConvert.SerializeObject(book.Meta);
      book.Imported = DateTime.UtcNow;
      foreach (var phr in book.Phrases) {
        if (!lessonIds.Contains(phr.LessonId)) errorLog.AppendLine(string.Format("Cannot find lesson in book={0}, lessonId={1}, phrase={2}", book.Name, phr.LessonId, phr.Text));
        phr.Lang = book.Lang;
      }
      ctx.Books.Add(book);
      ctx.SaveChanges();
    }
  }
}
