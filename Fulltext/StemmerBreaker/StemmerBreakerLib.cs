//SQL query: EXEC sp_help_fulltext_system_components 'wordbreaker'
//SELECT * FROM sys.dm_fts_parser (' "einem Pferd die Sporen geben" ', 1031, 0, 0)
//SELECT * FROM sys.dm_fts_parser('FormsOf(INFLECTIONAL, "Sporen")', 1031, 0, 0)
/*
EXEC sys.sp_fulltext_load_thesaurus_file 1031
SELECT* FROM sys.dm_fts_parser('FormsOf(FREETEXT, "writer")', 1033, 0, 0)
SELECT* FROM sys.dm_fts_parser('FormsOf(THESAURUS, "author")', 1033, 0, 0)
SELECT* FROM sys.dm_fts_parser('FormsOf(INFLECTIONAL, "Bücherregal")', 1031, 0, 0)
SELECT* FROM sys.dm_fts_parser('FormsOf(THESAURUS, "Bücherregal")', 1031, 0, 0)

SELECT* FROM sys.dm_fts_parser('FormsOf(INFLECTIONAL, "Nahrungsmittelunverträglichkeit")', 1031, 0, 0)
SELECT* FROM sys.dm_fts_parser('FormsOf(THESAURUS, "Nahrungsmittelunverträglichkeit")', 1031, 0, 0)
SELECT* FROM sys.dm_fts_parser('FormsOf(FREETEXT, "Nahrungsmittelunverträglichkeit")', 1031, 0, 0)
*/
//Data from registry: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ContentIndex\Language
using LangsLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StemmerBreaker {

	internal class Lib {

    //How to get all used MS SQL DLL's:
    //copy files (not dirs) from c:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\Binn\ to d:\rw\rw\StemmerBreaker\dlls\
    //run d:\rw\rw\stemmerbreaker\test.aspx, breaks in Runner.test.
    //delete all d:\rw\rw\StemmerBreaker\dlls\, with "Skip all". Used files remains undeleted.
    internal static void initFactories(string libBasicPath) {

			//get all LibraryModule's
			var allFiles = allFileNames.ToDictionary(f => f, f => LibraryModule.LoadModule(libBasicPath + f));
			foreach (var it in stemBreakGuids) {
				it.stemmerFactory = getClassFactory(it.stemmerFileName, it, true, allFiles);
				it.breakerFactory = getClassFactory(it.breakerFileName, it, false, allFiles);
			}
		}

		//internal const string neutral = "neutral";

		//lang to item dictionary
		internal static Dictionary<Langs, Lib> items;

		public static bool hasStemmer(Langs lang) {
			Lib item;
			return items.TryGetValue(lang, out item) && item.stemmerFactory != null;
		}

		internal IStemmer getStemmer() {
			var res = stemmerFactory == null ? null : ComHelper.CreateInstance<IStemmer>(stemmerFactory, typeof(IStemmer));
			if (res == null) return null;
			bool pfLicense = false;
			res.Init(1000, out pfLicense);
			return res;
		}

		internal IWordBreaker getWordBreaker() {
			var res = breakerFactory == null ? null : ComHelper.CreateInstance<IWordBreaker>(breakerFactory, typeof(IWordBreaker));
			if (res == null) return null;
			bool pfLicense = false;
			res.Init(true, 1000, out pfLicense);
			return res;
		}

		//********************** private

		Guid stemmerClass;
		Guid breakerClass;
		int locale;
		CultureInfo culture; //null => neutral word breaker
		string stemmerFileName; //==null => stemmer does not exists
		string breakerFileName;
		IClassFactory stemmerFactory; //==null => stemmer does not exists
		IClassFactory breakerFactory;
		//primary data from  HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch registry
		static Dictionary<string, string> guidToFile;
		static Lib[] stemBreakGuids;
		//all stemmer and breaker DLL names
		static HashSet<string> allFileNames = new HashSet<string>();

		static IClassFactory getClassFactory(string file, Lib it, bool isStemmer, Dictionary<string, LibraryModule> modules) {
			var fn = isStemmer ? it.stemmerFileName : it.breakerFileName; if (fn == null) return null;
			var guid = isStemmer ? it.stemmerClass : it.breakerClass;
			var res = ComHelper.GetClassFactory(modules[fn], guid);
			if (res == null) Console.WriteLine(string.Format("*** Error: file={0}, lang={1}, isStemmer={2}", fn, it.culture.Name, isStemmer));
			return res;
		}

		//init static infos
		static Lib() {
			try {

				//*******************************************
				//Data from registry: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\CLSID
				guidToFile = new Dictionary<string, string>() {

{ "022B358F-06B8-4e0d-ADD9-655A8F1E9EDD" ,
"NaturalLanguage6.dll"

}, { "04096682-6ECE-4E9E-90C1-52D81F0422ED" ,
"MsWb70011.dll"

}, { "04B37E30-C9A9-4A7D-8F20-792FC87DDF71" ,
"MsWb7.dll"

}, { "04DA8451-7F63-4870-A4D7-F55BE66BFDFB" ,
"NaturalLanguage6.dll"

}, { "05C9DA2B-6DFF-42c7-81CC-706DAE08BC7A" ,
"NaturalLanguage6.dll"

}, { "095D3BE1-A874-46a5-B989-AE43E3427E3C" ,
"NaturalLanguage6.dll"

}, { "0D5EBEA3-B982-46B9-9378-4C238262C12C" ,
"MsWb7.dll"

}, { "0F0549A6-C2E0-442a-85D7-20E3DB9B6A1F" ,
"NaturalLanguage6.dll"

}, { "0F7679E0-1386-493C-AF00-DAC67B1B16B1" ,
"MsWb7.dll"

}, { "10A2BDBC-7130-420c-9320-A92CC1919206" ,
"NaturalLanguage6.dll"

}, { "136E0057-D7ED-4b85-9F62-1318CFE1573B" ,
"NaturalLanguage6.dll"

}, { "1A3ED173-B201-4470-9FC6-EC46CF8D56F1" ,
"NaturalLanguage6.dll"

}, { "1C0D39B2-C788-40d2-B062-FDF8293D7BC6" ,
"NaturalLanguage6.dll"

}, { "1D49F57D-47D2-4aee-A69B-593EC558773F" ,
"NaturalLanguage6.dll"

}, { "1E69A8EB-0B11-40c3-AC27-906ED77CD946" ,
"NaturalLanguage6.dll"

}, { "212ADF89-1F86-49d0-914A-DF6C5613C81E" ,
"NaturalLanguage6.dll"

}, { "2652B813-2260-4ef3-A311-74A7AC6513D7" ,
"NaturalLanguage6.dll"

}, { "26D868ED-CA80-481B-B138-3E8C661161B1" ,
"MsWb7.dll"

}, { "2CB861BB-B1B4-4e14-A1A7-D3FB30C3F5CF" ,
"NaturalLanguage6.dll"

}, { "3D0B8752-68F8-4f39-929D-DE20ED323F45" ,
"NaturalLanguage6.dll"

}, { "3FAA93F3-79FB-4319-8387-B8FFE074FBDA" ,
"NaturalLanguage6.dll"

}, { "41B9BE05-B3AF-460C-BF0B-2CDD44A093B1" ,
"xmlfilt.dll"
//"ThreadingModel"="Both"

}, { "42001A23-ED2A-4582-8BCC-6320C543E102" ,
"NaturalLanguage6.dll"

}, { "4495524E-2E54-472d-86D7-D671CA588F01" ,
"NaturalLanguage6.dll"

}, { "4645F4F2-6359-4D81-BFB5-DAFBF89812B4" ,
"MsWb7.dll"

}, { "468BFC77-3876-4A47-A6FF-F5F6E8EA7968" ,
"MsWb7.dll"

}, { "49C69FAB-ED5E-4d48-9A65-E4816E5FE642" ,
"NaturalLanguage6.dll"

}, { "52BE2F87-1638-408a-9A98-74239B0B7DB5" ,
"NaturalLanguage6.dll"

}, { "530CC6A4-357F-49e2-AB11-3C481DBEDE31" ,
"NaturalLanguage6.dll"

}, { "53DA1CBB-0F45-46a4-AA6E-47CAAD84C921" ,
"NaturalLanguage6.dll"

}, { "54A12380-E78E-4980-BD96-2EF7076B5BB0" ,
"MsWb7.dll"

}, { "5645C8C2-E277-11CF-8FDA-00AA00A14F93" ,
"mimefilt.dll"
//"ThreadingModel"="Both"
//"Flags"=dword:00000001

}, { "5F104B61-7998-4049-A7BB-C99EFB6B4A4E" ,
"NaturalLanguage6.dll"

}, { "610133F4-ED38-42e7-9C18-EB2A8F76B99A" ,
"NaturalLanguage6.dll"

}, { "61DBD86A-8D1A-4eb0-907C-E4C1BBC8F09A" ,
"NaturalLanguage6.dll"

}, { "68DC71DC-2327-4040-8F03-50D6A9805049" ,
"NaturalLanguage6.dll"

}, { "69483C30-A9AF-4552-8F84-A0796AD5285B" ,
"MsWb7.dll"

}, { "697E2FF0-7FA8-49f1-BB4A-E1D115AA2BBB" ,
"NaturalLanguage6.dll"

}, { "69ED626B-904D-4def-B919-9EF7E4E339DD" ,
"NaturalLanguage6.dll"

}, { "6C53A912-47C6-4959-B342-DF6C9DA9D494" ,
"NaturalLanguage6.dll"

}, { "6D2D2C1D-5A3C-43C2-96D9-F3878D0A1B69" ,
"MsWb7.dll"

}, { "70878DCD-56F6-4681-BC52-BC7F58EDF723" ,
"NaturalLanguage6.dll"

}, { "712720F4-F4FF-46cf-B6EC-2CC24FC873A5" ,
"NaturalLanguage6.dll"

}, { "737FC9B5-BC66-4953-B7DB-B79A72A592A5" ,
"korwbrkr.dll"

}, { "773229CD-D53C-4211-ACD8-8F2C7BF2AE7C" ,
"NaturalLanguage6.dll"

}, { "776266FF-0DC5-4f45-BADA-39A7586FACE2" ,
"NaturalLanguage6.dll"

}, { "7AE7416D-AD97-4a4b-B5AC-B3CA7865AFBE" ,
"NaturalLanguage6.dll"

}, { "7E352021-69D6-4553-86AC-430B0D8FF913" ,
"NaturalLanguage6.dll"

}, { "81442F68-A942-457e-9AF0-C6977E244A7C" ,
"NaturalLanguage6.dll"

}, { "818C68B0-D4C9-475c-B2CF-AF4242F27C8D" ,
"NaturalLanguage6.dll"

}, { "86781CF9-799C-4cff-9AA5-43F4C23FF866" ,
"NaturalLanguage6.dll"

}, { "87824713-C8B0-4379-8556-1689764E4237" ,
"NaturalLanguage6.dll"

}, { "89F38560-A0AE-4d8c-9E8F-83D4DB8A9F85" ,
"NaturalLanguage6.dll"

}, { "8A474D89-6E2F-419C-8DD5-9B50EDC8C787" ,
"MsWb7.dll"

}, { "8A899610-150A-40db-B57A-940EDB3203CE" ,
"NaturalLanguage6.dll"

}, { "8B3302D7-95F6-4bc5-A06A-0D6DEF15DB69" ,
"NaturalLanguage6.dll"

}, { "8E67B6EF-205D-490f-A004-7B04F8F65B62" ,
"NaturalLanguage6.dll"

}, { "92F2118A-E813-4a4d-9DE2-F96A9DC02C53" ,
"NaturalLanguage6.dll"

}, { "950E4995-301B-4613-8042-F041BC34F32B" ,
"MsWb7.dll"

}, { "97E8EFC5-42DA-43ED-9CEC-84FDE3620F1A" ,
"korwbrkr.dll"

}, { "9D0EAB8C-8EF4-4020-B867-2B1E04E4B8E5" ,
"NaturalLanguage6.dll"

}, { "9FAED859-0B30-4434-AE65-412E14A16FB8" ,
"MsWb7.dll"

}, { "9FE6E853-B35F-4fe4-B006-33148455093E" ,
"NaturalLanguage6.dll"

}, { "A0A5A274-A190-4a81-997B-9593D6F6D462" ,
"NaturalLanguage6.dll"

}, { "A25A5CCD-80F4-4e02-AADD-7F39CC55E737" ,
"NaturalLanguage6.dll"

}, { "A9C6B8DD-3CBB-44cb-AA44-4B1C0DBB404D" ,
"NaturalLanguage6.dll"

}, { "AAA3D3BD-6DE7-4317-91A0-D25E7D3BABC3" ,
"MsWb7.dll"

}, { "B675B948-FBA8-46a4-A4C7-D4291785127B" ,
"NaturalLanguage6.dll"

}, { "C28DA8E5-39C2-4f62-82FA-C61D39A196DF" ,
"NaturalLanguage6.dll"

}, { "C4BF21DA-F1E5-4c7f-A611-2698645B19EF" ,
"NaturalLanguage6.dll"

}, { "C700F6EF-A80F-4b24-922A-32308B6FF0C3" ,
"NaturalLanguage6.dll"

}, { "C7310720-AC80-11D1-8DF3-00C04FB6EF4F" ,
"msfte.dll"
//"ThreadingModel"="Both"

}, { "CF923CB5-1187-43AB-B053-3E44BED65FFA" ,
"MsWb7.dll"

}, { "D0458F37-2228-4fc7-9E66-34133DF4C929" ,
"NaturalLanguage6.dll"

}, { "D42C8B70-ADEB-4B81-A52F-C09F24F77DFA" ,
"MsWb7.dll"

}, { "D9581C03-9766-45a6-B970-1EABBE985986" ,
"NaturalLanguage6.dll"

}, { "DC4701DE-1014-44cc-85A6-253F2B30FB9E" ,
"NaturalLanguage6.dll"

}, { "DFA00C33-BF19-482E-A791-3C785B0149B4" ,
"MsWb7.dll"

}, { "E0831C90-BAB0-4ca5-B9BD-EA254B538DAC" ,
"MsWb70804.dll"

}, { "E0CA5340-4534-11CF-B952-00AA0051FE20" ,
"nlhtml.dll"
//"ThreadingModel"="Both"

}, { "E1E0A883-AB68-4c6c-9C8C-808AF2BA4CBA" ,
"NaturalLanguage6.dll"

}, { "E1E5EF84-C4A6-4E50-8188-99AEF3DE2659" ,
"MsWb7.dll"

}, { "E58FA315-E206-4ca4-81CE-F34E18E672C9" ,
"NaturalLanguage6.dll"

}, { "E5B2CB7A-FD35-4d4b-A147-176FEB42244B" ,
"NaturalLanguage6.dll"

}, { "E7B6AEE0-84AE-46ce-B450-DEBF58C90889" ,
"NaturalLanguage6.dll"

}, { "E9B1DF65-08F1-438b-8277-EF462B23A792" ,
"MsWb70404.dll"

}, { "EB6C9433-4AAB-4b71-8B18-8F7A3812E43A" ,
"NaturalLanguage6.dll"

}, { "EE38A9FC-437F-4d03-A593-BB92AF0D153C" ,
"NaturalLanguage6.dll"

}, { "F07F3920-7B8C-11CF-9BE8-00AA004B9986" ,
"offfilt.dll"
//"ThreadingModel"="Both"
//"Flags"=dword:00000001

}, { "F3AEB884-58C8-40cf-AED3-E7EEFFFAA04A" ,
"NaturalLanguage6.dll"

}, { "F51B7203-9BF9-4C39-B655-18FAD8FA8A9A" ,
"MsWb7.dll"

}, { "F70C0935-6E9F-4EF1-9F06-7876536DB900" ,
"MsWb7001e.dll"

}, { "F7B02D8A-65DB-41cb-894D-5BBBF96C1B42" ,
"NaturalLanguage6.dll"

}, { "FBA89535-BFAB-4ef7-804C-109186BF507B" ,
"NaturalLanguage6.dll"

}, { "FD339D76-EA3E-435f-AC29-3FFCE55EB35B" ,
"NaturalLanguage6.dll"

}, { "FD51544B-8050-4c68-ABAE-3E1F7A8C01D3" ,
"NaturalLanguage6.dll"
}
	};
				foreach (var kv in guidToFile.ToArray()) guidToFile[kv.Key.ToUpper()] = kv.Value.ToLower();


				//************************************
				//Data from registry: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language
				stemBreakGuids = new Lib[] {
//"TsaurusFile"="tsara.xml"
new Lib { locale = 0x00000401
, breakerClass = new Guid("{04B37E30-C9A9-4A7D-8F20-792FC87DDF71}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\ben]
//"TsaurusFile"="tsben.xml"
}, new Lib { locale = 0x00000445
, breakerClass = new Guid("{05C9DA2B-6DFF-42c7-81CC-706DAE08BC7A}"),
stemmerClass = new Guid("{89F38560-A0AE-4d8c-9E8F-83D4DB8A9F85}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\bgr]
//"TsaurusFile"="tsbgr.xml"
}, new Lib { locale = 0x00000402
, breakerClass = new Guid("{B675B948-FBA8-46a4-A4C7-D4291785127B}"),
stemmerClass = new Guid("{3FAA93F3-79FB-4319-8387-B8FFE074FBDA}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\cat]
//"TsaurusFile"="tscat.xml"
}, new Lib { locale = 0x00000403
, breakerClass = new Guid("{3D0B8752-68F8-4f39-929D-DE20ED323F45}"),
stemmerClass = new Guid("{697E2FF0-7FA8-49f1-BB4A-E1D115AA2BBB}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\chs]
//"TsaurusFile"="tschs.xml"
}, new Lib { locale = 0x00000804
, breakerClass = new Guid("{E0831C90-BAB0-4ca5-B9BD-EA254B538DAC}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\cht]
//"TsaurusFile"="tscht.xml"
}, new Lib { locale = 0x00000404
, breakerClass = new Guid("{E9B1DF65-08F1-438b-8277-EF462B23A792}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\cze]
//"TsaurusFile"="tscze.xml"
}, new Lib { locale = 0x00000405
, breakerClass = new Guid("{468BFC77-3876-4A47-A6FF-F5F6E8EA7968}"),
stemmerClass = new Guid("{F51B7203-9BF9-4C39-B655-18FAD8FA8A9A}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\dan]
//"TsaurusFile"="tsdan.xml"
}, new Lib { locale = 0x00000406
, breakerClass = new Guid("{4645F4F2-6359-4D81-BFB5-DAFBF89812B4}"),
stemmerClass = new Guid("{950E4995-301B-4613-8042-F041BC34F32B}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\deu]
//"TsaurusFile"="tsdeu.xml"
}, new Lib { locale = 0x00000407
, breakerClass = new Guid("{DFA00C33-BF19-482E-A791-3C785B0149B4}"),
stemmerClass = new Guid("{8A474D89-6E2F-419C-8DD5-9B50EDC8C787}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\eng]
//"TsaurusFile"="tseng.xml"
}, new Lib { locale = 0x00000809
, breakerClass = new Guid("{9FAED859-0B30-4434-AE65-412E14A16FB8}"),
stemmerClass = new Guid("{E1E5EF84-C4A6-4E50-8188-99AEF3DE2659}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\enu]
//"TsaurusFile"="tsenu.xml"
}, new Lib { locale = 0x00000409
, breakerClass = new Guid("{9FAED859-0B30-4434-AE65-412E14A16FB8}"),
stemmerClass = new Guid("{E1E5EF84-C4A6-4E50-8188-99AEF3DE2659}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\esn]
//"TsaurusFile"="tsesn.xml"
}, new Lib { locale = 0x00000c0a
, breakerClass = new Guid("{68DC71DC-2327-4040-8F03-50D6A9805049}"),
stemmerClass = new Guid("{87824713-C8B0-4379-8556-1689764E4237}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\fra]
//"TsaurusFile"="tsfra.xml"
}, new Lib { locale = 0x0000040c
, breakerClass = new Guid("{92F2118A-E813-4a4d-9DE2-F96A9DC02C53}"),
stemmerClass = new Guid("{10A2BDBC-7130-420c-9320-A92CC1919206}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\grk]
//"TsaurusFile"="tsgrk.xml"
}, new Lib { locale = 0x00000408
, breakerClass = new Guid("{0F7679E0-1386-493C-AF00-DAC67B1B16B1}"),
stemmerClass = new Guid("{26D868ED-CA80-481B-B138-3E8C661161B1}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\guj]
//"TsaurusFile"="tsguj.xml"
}, new Lib { locale = 0x00000447
, breakerClass = new Guid("{04DA8451-7F63-4870-A4D7-F55BE66BFDFB}"),
stemmerClass = new Guid("{42001A23-ED2A-4582-8BCC-6320C543E102}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\heb]
//"TsaurusFile"="tsheb.xml"
}, new Lib { locale = 0x0000040d
, breakerClass = new Guid("{E5B2CB7A-FD35-4d4b-A147-176FEB42244B}"),
stemmerClass = new Guid("{E58FA315-E206-4ca4-81CE-F34E18E672C9}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\hin]
//"TsaurusFile"="tshin.xml"
}, new Lib { locale = 0x00000439
, breakerClass = new Guid("{0F0549A6-C2E0-442a-85D7-20E3DB9B6A1F}"),
stemmerClass = new Guid("{FD51544B-8050-4c68-ABAE-3E1F7A8C01D3}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\hrv]
//"TsaurusFile"="tshrv.xml"
}, new Lib { locale = 0x0000041a
, breakerClass = new Guid("{712720F4-F4FF-46cf-B6EC-2CC24FC873A5}"),
stemmerClass = new Guid("{818C68B0-D4C9-475c-B2CF-AF4242F27C8D}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\ind]
//"TsaurusFile"="tsind.xml"
}, new Lib { locale = 0x00000421
, breakerClass = new Guid("{F7B02D8A-65DB-41cb-894D-5BBBF96C1B42}"),
stemmerClass = new Guid("{E7B6AEE0-84AE-46ce-B450-DEBF58C90889}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\isl]
//"TsaurusFile"="tsisl.xml"
}, new Lib { locale = 0x0000040f
, breakerClass = new Guid("{53DA1CBB-0F45-46a4-AA6E-47CAAD84C921}"),
stemmerClass = new Guid("{8E67B6EF-205D-490f-A004-7B04F8F65B62}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\ita]
//"TsaurusFile"="tsita.xml"
}, new Lib { locale = 0x00000410
, breakerClass = new Guid("{7E352021-69D6-4553-86AC-430B0D8FF913}"),
stemmerClass = new Guid("{52BE2F87-1638-408a-9A98-74239B0B7DB5}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\jpn]
//"TsaurusFile"="tsjpn.xml"
}, new Lib { locale = 0x00000411
, breakerClass = new Guid("{04096682-6ECE-4E9E-90C1-52D81F0422ED}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\kan]
//"TsaurusFile"="tskan.xml"
}, new Lib { locale = 0x0000044b
, breakerClass = new Guid("{4495524E-2E54-472d-86D7-D671CA588F01}"),
stemmerClass = new Guid("{095D3BE1-A874-46a5-B989-AE43E3427E3C}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\kor]
//"TsaurusFile"="tskor.xml"
}, new Lib { locale = 0x00000412
, breakerClass = new Guid("{737FC9B5-BC66-4953-B7DB-B79A72A592A5}"),
stemmerClass = new Guid("{97E8EFC5-42DA-43ED-9CEC-84FDE3620F1A}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\lth]
//"TsaurusFile"="tslth.xml"
}, new Lib { locale = 0x00000427
, breakerClass = new Guid("{1C0D39B2-C788-40d2-B062-FDF8293D7BC6}"),
stemmerClass = new Guid("{E1E0A883-AB68-4c6c-9C8C-808AF2BA4CBA}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\lvi]
//"TsaurusFile"="tslvi.xml"
}, new Lib { locale = 0x00000426
, breakerClass = new Guid("{C700F6EF-A80F-4b24-922A-32308B6FF0C3}"),
stemmerClass = new Guid("{9D0EAB8C-8EF4-4020-B867-2B1E04E4B8E5}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\mal]
//"TsaurusFile"="tsmal.xml"
}, new Lib { locale = 0x0000044c
, breakerClass = new Guid("{69ED626B-904D-4def-B919-9EF7E4E339DD}"),
stemmerClass = new Guid("{1A3ED173-B201-4470-9FC6-EC46CF8D56F1}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\mar]
//"TsaurusFile"="tsmar.xml"
}, new Lib { locale = 0x0000044e
, breakerClass = new Guid("{8B3302D7-95F6-4bc5-A06A-0D6DEF15DB69}"),
stemmerClass = new Guid("{81442F68-A942-457e-9AF0-C6977E244A7C}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\msl]
//"TsaurusFile"="tsmsl.xml"
}, new Lib { locale = 0x0000043e
, breakerClass = new Guid("{EB6C9433-4AAB-4b71-8B18-8F7A3812E43A}"),
stemmerClass = new Guid("{A0A5A274-A190-4a81-997B-9593D6F6D462}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\Neutral]
//"TsaurusFile"="tsglobal.xml"
}, new Lib { locale = 0x00000000
, breakerClass = new Guid("{1D49F57D-47D2-4aee-A69B-593EC558773F}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\nld]
//"TsaurusFile"="tsnld.xml"
}, new Lib { locale = 0x00000413
, breakerClass = new Guid("{69483C30-A9AF-4552-8F84-A0796AD5285B}"),
stemmerClass = new Guid("{CF923CB5-1187-43AB-B053-3E44BED65FFA}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\nor]
//"TsaurusFile"="tsnor.xml"
}, new Lib { locale = 0x00000414
, breakerClass = new Guid("{A9C6B8DD-3CBB-44cb-AA44-4B1C0DBB404D}"),
stemmerClass = new Guid("{86781CF9-799C-4cff-9AA5-43F4C23FF866}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\pan]
//"TsaurusFile"="tspan.xml"
}, new Lib { locale = 0x00000446
, breakerClass = new Guid("{9FE6E853-B35F-4fe4-B006-33148455093E}"),
stemmerClass = new Guid("{5F104B61-7998-4049-A7BB-C99EFB6B4A4E}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\plk]
//"TsaurusFile"="tsplk.xml"
}, new Lib { locale = 0x00000415
, breakerClass = new Guid("{0D5EBEA3-B982-46B9-9378-4C238262C12C}"),
stemmerClass = new Guid("{6D2D2C1D-5A3C-43C2-96D9-F3878D0A1B69}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\ptb]
//"TsaurusFile"="tsptb.xml"
}, new Lib { locale = 0x00000416
, breakerClass = new Guid("{A25A5CCD-80F4-4e02-AADD-7F39CC55E737}"),
stemmerClass = new Guid("{DC4701DE-1014-44cc-85A6-253F2B30FB9E}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\ptg]
//"TsaurusFile"="tsptg.xml"
}, new Lib { locale = 0x00000816
, breakerClass = new Guid("{C4BF21DA-F1E5-4c7f-A611-2698645B19EF}"),
stemmerClass = new Guid("{7AE7416D-AD97-4a4b-B5AC-B3CA7865AFBE}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\rom]
//"TsaurusFile"="tsrom.xml"
}, new Lib { locale = 0x00000418
, breakerClass = new Guid("{D0458F37-2228-4fc7-9E66-34133DF4C929}"),
stemmerClass = new Guid("{610133F4-ED38-42e7-9C18-EB2A8F76B99A}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\rus]
//"TsaurusFile"="tsrus.xml"
}, new Lib { locale = 0x00000419
, breakerClass = new Guid("{AAA3D3BD-6DE7-4317-91A0-D25E7D3BABC3}"),
stemmerClass = new Guid("{D42C8B70-ADEB-4B81-A52F-C09F24F77DFA}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\sky]
//"TsaurusFile"="tssky.xml"
}, new Lib { locale = 0x0000041b
, breakerClass = new Guid("{2652B813-2260-4ef3-A311-74A7AC6513D7}"),
stemmerClass = new Guid("{FBA89535-BFAB-4ef7-804C-109186BF507B}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\slv]
//"TsaurusFile"="tsslv.xml"
}, new Lib { locale = 0x00000424
, breakerClass = new Guid("{8A899610-150A-40db-B57A-940EDB3203CE}"),
stemmerClass = new Guid("{022B358F-06B8-4e0d-ADD9-655A8F1E9EDD}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\srb]
//"TsaurusFile"="tssrb.xml"
}, new Lib { locale = 0x00000c1a
, breakerClass = new Guid("{C28DA8E5-39C2-4f62-82FA-C61D39A196DF}"),
stemmerClass = new Guid("{49C69FAB-ED5E-4d48-9A65-E4816E5FE642}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\srl]
//"TsaurusFile"="tssrl.xml"
}, new Lib { locale = 0x0000081a
, breakerClass = new Guid("{EE38A9FC-437F-4d03-A593-BB92AF0D153C}"),
stemmerClass = new Guid("{212ADF89-1F86-49d0-914A-DF6C5613C81E}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\sve]
//"TsaurusFile"="tssve.xml"
}, new Lib { locale = 0x0000041d
, breakerClass = new Guid("{2CB861BB-B1B4-4e14-A1A7-D3FB30C3F5CF}"),
stemmerClass = new Guid("{61DBD86A-8D1A-4eb0-907C-E4C1BBC8F09A}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\tam]
//"TsaurusFile"="tstam.xml"
}, new Lib { locale = 0x00000449
, breakerClass = new Guid("{6C53A912-47C6-4959-B342-DF6C9DA9D494}"),
stemmerClass = new Guid("{1E69A8EB-0B11-40c3-AC27-906ED77CD946}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\tel]
//"TsaurusFile"="tstel.xml"
}, new Lib { locale = 0x0000044a
, breakerClass = new Guid("{136E0057-D7ED-4b85-9F62-1318CFE1573B}"),
stemmerClass = new Guid("{530CC6A4-357F-49e2-AB11-3C481DBEDE31}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\tha]
//"TsaurusFile"="tstha.xml"
}, new Lib { locale = 0x0000041e
, breakerClass = new Guid("{F70C0935-6E9F-4EF1-9F06-7876536DB900}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\trk]
//"TsaurusFile"="tstrk.xml"
}, new Lib { locale = 0x0000041f
, breakerClass = new Guid("{54A12380-E78E-4980-BD96-2EF7076B5BB0}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\ukr]
//"TsaurusFile"="tsukr.xml"
}, new Lib { locale = 0x00000422
, breakerClass = new Guid("{773229CD-D53C-4211-ACD8-8F2C7BF2AE7C}"),
stemmerClass = new Guid("{D9581C03-9766-45a6-B970-1EABBE985986}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\urd]
//"TsaurusFile"="tsurd.xml"
}, new Lib { locale = 0x00000420
, breakerClass = new Guid("{F3AEB884-58C8-40cf-AED3-E7EEFFFAA04A}"),
stemmerClass = new Guid("{776266FF-0DC5-4f45-BADA-39A7586FACE2}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\vit]
//"TsaurusFile"="tsvit.xml"
}, new Lib { locale = 0x0000042a
, breakerClass = new Guid("{70878DCD-56F6-4681-BC52-BC7F58EDF723}"),
stemmerClass = new Guid("{FD339D76-EA3E-435f-AC29-3FFCE55EB35B}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\zh-hk]
//"TsaurusFile"="tscht.xml"
}, new Lib { locale = 0x00000c04
, breakerClass = new Guid("{E9B1DF65-08F1-438b-8277-EF462B23A792}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\zh-mo]
//"TsaurusFile"="tscht.xml"
}, new Lib { locale = 0x00001404
, breakerClass = new Guid("{E9B1DF65-08F1-438b-8277-EF462B23A792}"),

//[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSearch\Language\zh-sg]
//"TsaurusFile"="tschs.xml"
}, new Lib { locale = 0x00001004
, breakerClass = new Guid("{E0831C90-BAB0-4ca5-B9BD-EA254B538DAC}")
}

};
				foreach (var it in stemBreakGuids) {
					it.culture = it.locale > 0 ? CultureInfo.GetCultureInfo(it.locale) : null;
					adjustDLLFileName(it, true);
					adjustDLLFileName(it, false);
				}
				items = new Dictionary<Langs, Lib>();
				items.Add(Langs._, stemBreakGuids.First(g => g.locale == 0));
				foreach (var it in stemBreakGuids) {
					var lng = it.culture == null ? Langs._ : Metas.string2lang(it.culture.Name);
					if (lng == Langs._)
						continue;
					items.Add(lng, it);
				}

				if (items == null) return;

				//items = itemssql.Where(it => Metas.string2lang(it.culture.Name)!=Langs._).ToDictionary(it => it.culture != null ? Metas.string2lang(it.culture.Name) : Langs._, it => it);
			} catch (Exception exp) {
				if (exp == null) return;
			}
		}
		static void adjustDLLFileName(Lib it, bool isStemmer) {
			Guid gd = isStemmer ? it.stemmerClass : it.breakerClass;
			var guid = gd.ToString().ToUpper(); if (guid == "00000000-0000-0000-0000-000000000000") return;
			var res = guidToFile[guid];
			if (res != null) allFileNames.Add(res);
			if (isStemmer) it.stemmerFileName = res; else it.breakerFileName = res;
		}

	}
}