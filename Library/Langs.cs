using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace LangsLib {

	public class Meta {
		//public byte Idx;
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
		public Meta[] Items;
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

		public static char langToCharCode(Langs lng) { return langCodes[(int)lng]; }
		static char[] langCodes = Enumerable.Range(32, 126 - 32).Concat(Enumerable.Range(192, 255 - 192)).Select(n => (char)n).ToArray();

		public static string lang2string(Langs lang) { return lang.ToString().Replace('_', '-'); }
		public static Langs string2lang(string lang) { try { return (Langs)Enum.Parse(typeof(Langs), lang.Replace('-', '_').ToLower()); } catch { return Langs._; } }
		public static int lang2LCID(Langs lang) { return CultureInfo.GetCultureInfo(lang2string(lang)).LCID; }

		public static HashSet<Langs> SpellCheckLangs = new HashSet<Langs>() {
			Langs.ar_sa, Langs.bg_bg, Langs.bn_in, Langs.ca_es, Langs.cs_cz, Langs.da_dk, Langs.de_de, Langs.el_gr, Langs.en_gb, Langs.en_us, Langs.es_es, Langs.fi_fi, Langs.fr_fr, Langs.gu_in, Langs.he_il, Langs.hi_in, Langs.hr_hr, Langs.hu_hu, Langs.id_id, Langs.it_it, Langs.kn_in, Langs.lt_lt, Langs.lv_lv, Langs.ml_in, Langs.mr_in, Langs.ms_my, Langs.nb_no, Langs.nl_nl, Langs.pa_in, Langs.pl_pl, Langs.pt_br, Langs.pt_pt, Langs.ro_ro, Langs.ru_ru, Langs.sk_sk, Langs.sl_si, Langs.sv_se, Langs.ta_in, Langs.te_in, Langs.tr_tr, Langs.uk_ua, Langs.ur_pk, Langs.vi_vn
		};

		public static HashSet<Langs> StemmerBreakerLangs = new HashSet<Langs>() {
			Langs.ar_sa, Langs.bg_bg, Langs.bn_in, Langs.ca_es, Langs.cs_cz, Langs.da_dk, Langs.de_de, Langs.el_gr, Langs.en_gb, Langs.en_us, Langs.es_es, Langs.fr_fr, Langs.gu_in, Langs.he_il, Langs.hi_in, Langs.hr_hr, Langs.id_id, Langs.is_is, Langs.it_it, Langs.ja_jp, Langs.kn_in, Langs.ko_kr, Langs.lt_lt, Langs.lv_lv, Langs.ml_in, Langs.mr_in, Langs.ms_my, Langs.nb_no, Langs.nl_nl, Langs.pa_in, Langs.pl_pl, Langs.pt_br, Langs.pt_pt, Langs.ro_ro, Langs.ru_ru, Langs.sk_sk, Langs.sl_si, Langs.sr_latn_cs, Langs.sv_se, Langs.ta_in, Langs.te_in, Langs.th_th, Langs.tr_tr, Langs.uk_ua, Langs.ur_pk, Langs.vi_vn, Langs.zh_cn, Langs.zh_hk
		};
	}

	public struct TPosLen {
		public int Pos;
		public int Len; //could be negative when SpellChecker error
		public string encode() { return string.Format("{0}-{1}", Pos, Len); }
		public static string encode(IEnumerable<TPosLen> items) { return items == null ? null : items.DefaultIfEmpty().Select(it => it.encode()).Aggregate((r, i) => r + "|" + i); }
	}

	public class PhraseWords {
		public string Text;
		public List<TPosLen> Idxs; //list of 
		//public string[] getWords(bool toLower = true, bool incErrors = false) { return Idxs.Select(idx => !incErrors && idx.Len<0 ? null : Text.Substring(idx.Pos, Math.Abs(idx.Len))).Where(w => w!=null).Select(w => toLower ? w.ToLower() : w).ToArray(); }
	}

	public struct PhraseSide {
		public Langs src;
		public Langs dest;
		//public string encode(string word) { return PhraseSide.encode(src, dest, word); }
		//public static string encode(Langs src, Langs dest, string word) { return Metas.langToCharCode(src) + Metas.langToCharCode(dest) + word; }
		public Langs langOfText() { return dest == Langs._ ? src : dest; }
		public byte[] getDictId() { return new byte[] { (byte) src, (byte) dest }; }
		}

	public enum Langs {
		_ = 0, //  
		af_za = 1, // !
		ar_sa = 2, // "
		as_in = 3, // #
		az_latn_az = 4, // $
		be_by = 5, // %
		bg_bg = 6, // &
		bn_in = 7, // '
		bo_cn = 8, // (
		br_fr = 9, // )
		bs_latn_ba = 10, // *
		ca_es = 11, // +
		co_fr = 12, // ,
		cs_cz = 13, // -
		da_dk = 14, // .
		de_de = 15, // /
		el_gr = 16, // 0
		en_gb = 17, // 1
		en_us = 18, // 2
		es_es = 19, // 3
		et_ee = 20, // 4
		eu_es = 21, // 5
		fa_ir = 22, // 6
		fi_fi = 23, // 7
		fr_fr = 24, // 8
		ga_ie = 25, // 9
		gl_es = 26, // :
		gu_in = 27, // ;
		ha_latn_ng = 28, // <
		he_il = 29, // =
		hi_in = 30, // >
		hr_hr = 31, // ?
		hu_hu = 32, // @
		hy_am = 33, // A
		id_id = 34, // B
		ig_ng = 35, // C
		is_is = 36, // D
		it_it = 37, // E
		ja_jp = 38, // F
		ka_ge = 39, // G
		km_kh = 40, // H
		kn_in = 41, // I
		ko_kr = 42, // J
		ky_kg = 43, // K
		lt_lt = 44, // L
		lv_lv = 45, // M
		mi_nz = 46, // N
		mk_mk = 47, // O
		ml_in = 48, // P
		mn_mn = 49, // Q
		mr_in = 50, // R
		ms_my = 51, // S
		mt_mt = 52, // T
		nb_no = 53, // U
		nl_nl = 54, // V
		nso_za = 55, // W
		oc_fr = 56, // X
		pa_in = 57, // Y
		pl_pl = 58, // Z
		ps_af = 59, // [
		pt_br = 60, // \
		pt_pt = 61, // ]
		quz_pe = 62, // ^
		ro_ro = 63, // _
		ru_ru = 64, // `
		sk_sk = 65, // a
		sl_si = 66, // b
		sq_al = 67, // c
		sr_latn_cs = 68, // d
		sv_se = 69, // e
		sw_ke = 70, // f
		ta_in = 71, // g
		te_in = 72, // h
		th_th = 73, // i
		tn_za = 74, // j
		tr_tr = 75, // k
		uk_ua = 76, // l
		ur_pk = 77, // m
		uz_latn_uz = 78, // n
		vi_vn = 79, // o
		xh_za = 80, // p
		yo_ng = 81, // q
		zh_cn = 82, // r
		zh_hk = 83, // s
		zu_za = 84, // t
	}

}
