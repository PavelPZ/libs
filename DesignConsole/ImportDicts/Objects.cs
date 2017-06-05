using FileHelpers;
using LangsLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignConsole.ImportDicts {

	public class Meta {
		[JsonConverter(typeof(LangsJSONConverter))]
		public Langs src;
		[JsonConverter(typeof(LangsJSONConverter))]
		public Langs dest;
		public string path;

		public string fullPath() { return Metas.basicPath + path; }
		public string logPath() {
			var fn = fullPath();
			var name = Path.GetFileName(fn);
			var dir = Path.GetDirectoryName(fn);
			return string.Format(@"{0}\ex-{1}", dir, name);
		}

	}

	public class Metas {
		public Meta[] Items;
		public static string basicPath = ConfigurationManager.AppSettings["Dicts.sourcePath"];
		public static Metas read() { return JsonConvert.DeserializeObject<Metas>(File.ReadAllText(Metas.basicPath + @"dictmetas.json")); }
	}

	[DelimitedRecord(";")]
	[IgnoreEmptyLines()]
	[IgnoreFirst()]
	public class CVSDictItem {
		public string Src;
		public string Dest;
		[FieldOptional()]
		public string SrcLog;
		[FieldOptional()]
		public string DestLog;
	}

	public class LangsJSONConverter : JsonConverter {
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			writer.WriteValue(((Langs)value).ToString());
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			var enumString = (string)reader.Value;
			return Enum.Parse(typeof(Langs), enumString, true);
		}
		public override bool CanConvert(Type objectType) {
			return objectType == typeof(string);
		}
	}

}
