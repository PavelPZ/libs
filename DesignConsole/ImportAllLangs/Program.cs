using Excel;
using Langs;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

internal class ImportAllLangs {
  const string importPath = @"c:\rw\libs\DesignConsole\ImportAllLangs\";
  internal static void Run() {
    using (FileStream stream = File.Open(importPath + @"RewiseJazyky.xlsx", FileMode.Open, FileAccess.Read))
    using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream)) {
      excelReader.IsFirstRowAsColumnNames = true;
      DataSet result = excelReader.AsDataSet();
      var tb = result.Tables.OfType<DataTable>().First(t => t.TableName == "ToPZCode");
      var rows = tb.Rows.Cast<DataRow>().Where(r => !r.IsNull(0)).ToArray();
      int lcid; CultureInfo lc; string SpellCheckId;
      var metas = rows.Select(r => new Meta() {
        Idx = byte.Parse(r[0].ToString()),
        LCID = lcid = int.Parse(r[1].ToString()),
        lc = lc = CultureInfo.GetCultureInfo(lcid),
        IsRightToLeft = lc.TextInfo.IsRightToLeft,
        EnglishName = lc.EnglishName,
        NativeName = lc.NativeName,
        //DisplayName = lc.DisplayName,
        Descr = r[2].ToString(),
        Location = r[3].ToString(),
        Id = r[4].ToString().Replace('_', '-').ToLower(),
        IsFulltext = r[5].ToString() == "ANO",
        IsEuroTalk = r[6].ToString() == "ANO",
        IsGoethe = r[7].ToString() == "ANO",
        IsLingea = r[8].ToString() == "ANO",
        SpellCheckId = SpellCheckId = r[11].ToString(),
        IsSpellCheck = r[10].ToString() == "ANO" || !string.IsNullOrEmpty(SpellCheckId),
      }).OrderBy(m => m.Id).ToArray();
      //kontroly
      var errors = metas.Where(m => m.lc.Name.ToLower() != m.Id).ToArray();
      //to XML
      new Metas() { Langs = metas }.toFile(importPath + @"RewiseJazyky.xml");
      //C:\rw\libs\Library\LangsIdx.cs
      StringBuilder sb = new StringBuilder();
      using (StringWriter cs = new StringWriter(sb)) {
        cs.WriteLine("public enum Langs {");
        cs.WriteLine("  no = 0,");
        foreach (var meta in metas.Where(m => m.Idx > 0).OrderBy(m => m.Idx)) {
          cs.WriteLine(string.Format("  {0} = {1},", meta.Id.Replace('-', '_'), meta.Idx));
        };
        cs.WriteLine("}");
      }
      File.WriteAllText(importPath + @"library-langs-cs.txt", sb.ToString());
    }
  }
}
