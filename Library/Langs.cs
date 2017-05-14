using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace Langs {

  public class Meta {
    public int LCID;
    public string Id;
    public string Descr;
    public string Location;
    public string EnglishName;
    public string NativeName;
    //public string DisplayName;
    public bool IsFulltext;
    public bool IsSpellCheck;
    public string SpellCheckId;
    public bool IsLingea;
    public bool IsEuroTalk;
    public bool IsGoethe;
    public bool IsRightToLeft;
    [XmlIgnore]
    public CultureInfo lc;
  }

  public class Metas {
    public Meta[] Langs;
    public void toFile(string fn) {
      if (File.Exists(fn)) File.Delete(fn);
      var ser = new XmlSerializer(typeof(Metas));
      using (var fs = File.OpenWrite(fn))
        ser.Serialize(fs, this);
    }
    public static Metas fromFile(string fn) {
      var ser = new XmlSerializer(typeof(Metas));
      using (var fs = File.OpenRead(fn))
        return ser.Deserialize(fs) as Metas;

    }
  }
}
