using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ImportAllLangs {

  public class LangMeta {
    public int LCID;
    public string Id;
    public string Descr;
    public string Location;
    public string EnglishName;
    public string NativeName;
    public string DisplayName;
    public bool isFulltext;
    public bool isSpellCheck;
    public bool isLingea;
    public bool isEuroTalk;
    public bool isGoethe;
    public bool IsRightToLeft;
    [XmlIgnore]
    public CultureInfo lc;
  }

  public class LangMetas {
    public LangMeta[] Langs;
  }

  class Program {
    static void Main(string[] args) {
      using (FileStream stream = File.Open(@"c:\rw\libs\ImportAllLangs\RewiseJazyky.xlsx", FileMode.Open, FileAccess.Read))
      using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream)) {
        excelReader.IsFirstRowAsColumnNames = true;
        DataSet result = excelReader.AsDataSet();
        var tb = result.Tables.OfType<DataTable>().First(t => t.TableName == "ToPZCode");
        var rows = tb.Rows.Cast<DataRow>().Where(r => !r.IsNull(0)).ToArray();
        int lcid; CultureInfo lc;
        var metas = rows.Select(r => new LangMeta() {
          LCID = lcid = int.Parse(r[0].ToString()),
          lc = lc = CultureInfo.GetCultureInfo(lcid),
          IsRightToLeft = lc.TextInfo.IsRightToLeft,
          EnglishName = lc.EnglishName,
          NativeName = lc.NativeName,
          DisplayName = lc.DisplayName,
          Descr = r[1].ToString(),
          Location = r[2].ToString(),
          Id = r[3].ToString().Replace('_', '-').ToLower(),
          isFulltext = r[4].ToString() == "ANO",
          isEuroTalk = r[5].ToString() == "ANO",
          isGoethe = r[6].ToString() == "ANO",
          isLingea = r[7].ToString() == "ANO",
          isSpellCheck = r[9].ToString() == "ANO",
        }).OrderBy(m => m.Id).ToArray();
        //kontroly
        var errors = metas.Where(m => m.lc.Name.ToLower()!=m.Id).ToArray();
        //to XML
        var ser = new XmlSerializer(typeof(LangMetas));
        using (var fs = File.OpenWrite(@"c:\rw\libs\ImportAllLangs\RewiseJazyky.xml"))
          ser.Serialize(fs, new LangMetas() { Langs = metas });
      }
    }
  }
}
