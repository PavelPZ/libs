using BookDBModel;
using LangsLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace DesignImportBooks {
  class Program {
    static void Main(string[] args) {

      PhraseLib.Test.Run();
      return;


      var book = new Book {
        Name= "AEC_1",
        Lang = (byte)Langs.en_gb,
        Meta = new BookMeta {
          Publisher = "Cambridge University Press",
          BookGroup = "Cambridge Academic-english",
          Title = "Cambridge Academic-english (AEC_1)",
          _path = "RJWordListImports\\Cambridge Academic-english (AEC_1)\\",
          Lessons = new LessonMeta[] {
            new LessonMeta {
              Id = 1,
              Title = "Lesson 1"
            },
            new LessonMeta {
              Id = 2,
              Title = "Lesson 2"
            }

          }
        },
        Phrases = new Phrase[] {
          new Phrase{
            LessonId = 1,
            Text = "How are you",
            isPhraseWord = true,
            Locales = new Locale[] {
              new Locale{
                Lang = (byte)Langs.cs_cz,
                Text = "Jak se máš"
              },
              new Locale{
                Lang = (byte)Langs.sk_sk,
                Text = "Ako sa máš"
              }
            }
          }
        },
      };
      File.WriteAllText(@"d:\temp\dict.json", JsonConvert.SerializeObject(book, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore, Formatting = Formatting.Indented } ), Encoding.UTF8);
      var errorLog = new StringBuilder();
      ImportBooks.importBook(@"d:\temp\dict.json", errorLog);
      if (errorLog.Length > 0) {
        Console.WriteLine(@">>> ERROR: see d:\temp\error.txt");
        File.WriteAllText(@"d:\temp\error.txt", errorLog.ToString());
        Console.ReadKey();
      }
    }
  }
}
