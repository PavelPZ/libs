using ExcelDataReader;
using LangsLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

internal class ImportAllLangs {
	//const string importPath = @"c:\rw\libs\DesignConsole\ImportAllLangs\";
	const string importPath = @"d:\rw\libs\DesignConsole\ImportAllLangs\";
	internal static void DirectoriesForFrequency() {
		using (FileStream stream = File.Open(importPath + @"FrequencyDirs.xlsx", FileMode.Open, FileAccess.Read))
		using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream)) {
      //excelReader.IsFirstRowAsColumnNames = true; Chybi ve verzi 3, co s tim? Cosi je tady: https://github.com/ExcelDataReader/ExcelDataReader
      DataSet result = excelReader.AsDataSet();
			var tb = result.Tables.OfType<DataTable>().First(t => t.TableName == "ToPZCode");
			var rows = tb.Rows.Cast<DataRow>().Where(r => !r.IsNull(0)).ToArray();
			var dirs =  rows.Select(r => new { dir = r[0].ToString(), lang = (Langs)Enum.Parse(typeof(Langs), r[1].ToString(), true) })/*.Where(r => Metas.SpellCheckLangs.Contains(r.lang))*/.ToArray();
			StringBuilder sb = new StringBuilder();
			using (StringWriter cs = new StringWriter(sb)) {
				cs.WriteLine("static Dictionary<Langs, string> langDir = new Dictionary<Langs, string> {");
				foreach (var dir in dirs) cs.Write(string.Format("{{ Langs.{0}, \"{1}\" }}, ", dir.lang, dir.dir));
				cs.WriteLine(""); cs.WriteLine("};");
				cs.WriteLine("public static HashSet<Langs> forLangs = new HashSet<Langs> {");
				foreach (var dir in dirs) cs.Write(string.Format("Langs.{0}, ", dir.lang));
				cs.WriteLine(""); cs.WriteLine("};");
			};
			File.WriteAllText(importPath + @"library-frequency-cs.txt", sb.ToString(), Encoding.UTF8);
		}
	}
	internal static void Run() {
		using (FileStream stream = File.Open(importPath + @"RewiseJazyky.xlsx", FileMode.Open, FileAccess.Read))
		using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream)) {
      //excelReader.IsFirstRowAsColumnNames = true; Chybi ve verzi 3, co s tim?
      DataSet result = excelReader.AsDataSet();
			var tb = result.Tables.OfType<DataTable>().First(t => t.TableName == "ToPZCode");
			var rows = tb.Rows.Cast<DataRow>().Where(r => !r.IsNull(0)).ToArray();
			int lcid; CultureInfo lc; string SpellCheckId;
			var metas = rows.Select(r => new Meta() {
				//Idx = byte.Parse(r[0].ToString()),
				LCID = lcid = int.Parse(r[0].ToString()),
				lc = lc = CultureInfo.GetCultureInfo(lcid),
				IsRightToLeft = lc.TextInfo.IsRightToLeft,
				EnglishName = lc.EnglishName,
				NativeName = lc.NativeName,
				//DisplayName = lc.DisplayName,
				Descr = r[1].ToString(),
				Location = r[2].ToString(),
				Id = lc.Name.ToLower().ToLower(),
				IsFulltext = r[4].ToString() == "ANO",
				IsEuroTalk = r[5].ToString() == "ANO",
				IsGoethe = r[6].ToString() == "ANO",
				IsLingea = r[7].ToString() == "ANO",
				SpellCheckId = SpellCheckId = r[10].ToString(),
				IsSpellCheck = r[9].ToString() == "ANO" || !string.IsNullOrEmpty(SpellCheckId),
			}).OrderBy(m => m.Id).ToArray();
			//kontroly
			//var errors = metas.Where(m => m.lc.Name.ToLower() != m.Id).ToArray();
			//to XML
			new Metas() { Items = metas }.toFile(importPath + @"RewiseJazyky.xml");
			//C:\rw\libs\Library\LangsIdx.cs
			//so far defined LANG consts:
			var langEnum = typeof(LangsLib.Langs);
			var soFarLangs = langEnum.GetEnumValues().Cast<int>().ToDictionary(en => langEnum.GetEnumName(en), en => en);
			//add new LANG consts
			var maxLang = soFarLangs.Values.Max();
			foreach (var id in metas.Where(m => m.LCID > 0).OrderBy(m => m.Id).Select(m => m.Id.Replace('-', '_')).Where(idd => !soFarLangs.ContainsKey(idd)))
				soFarLangs.Add(id, ++maxLang);
			//Print
			StringBuilder sb = new StringBuilder();
			using (StringWriter cs = new StringWriter(sb)) {
				//SpellCheck langs
				cs.WriteLine("public static HashSet<Langs> SpellCheckLangs = new HashSet<Langs>() {");
				cs.WriteLine("  " + metas.Where(m => m.IsSpellCheck).Select(m => "Langs." + Metas.string2lang(m.Id).ToString()).Aggregate((r, i) => r + ", " + i));
				cs.WriteLine("};");
				//StemmerBreakerLangs
				cs.WriteLine("public static HashSet<Langs> StemmerBreakerLangs = new HashSet<Langs>() {");
				cs.WriteLine("  " + metas.Where(m => m.IsFulltext).Select(m => "Langs." + Metas.string2lang(m.Id).ToString()).Aggregate((r, i) => r + ", " + i));
				cs.WriteLine("};");
				//Langs
				cs.WriteLine("public enum Langs {");
				foreach (var kv in soFarLangs.OrderBy(sf => sf.Key)) cs.WriteLine(string.Format("  {0} = {1}, // {2}", kv.Key, kv.Value, Metas.langToCharCode((Langs)kv.Value)));
				cs.WriteLine("}");
			}
			File.WriteAllText(importPath + @"library-langs-cs.txt", sb.ToString(), Encoding.UTF8);
		}
	}
	//const string chars
}
