﻿using LangsLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker {
	public static class Frequency {

		static Frequency() {
			foreach (var lang in forLangs) {
				var fn = string.Format(@"{0}{1}\{1}_full.txt", ConfigurationManager.AppSettings["FrequencyData"], langDir[lang]);
				var fr = new List<string>(); langContent[lang] = fr;
				foreach (var l in File.ReadAllLines(fn)) {
					if (string.IsNullOrEmpty(l)) continue;
					var parts = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					fr.Add(parts[0]);
					//fr.Add(new FrequencyItem { word = parts[0], freq = int.Parse(parts[1]) });
				}
				var hs = new HashSet<string>(fr); isWord[lang] = hs;
			}
			var count = langContent.Sum(c => c.Value.Count);
			count = 0;
		}

		static Dictionary<Langs, List<string>> langContent = new Dictionary<Langs, List<string>>();
		public static Dictionary<Langs, HashSet<string>> isWord = new Dictionary<Langs, HashSet<string>>();

		static Dictionary<Langs, string> langDir = new Dictionary<Langs, string> {
			{ Langs.af_za, "af" }, { Langs.ar_sa, "ar" }, { Langs.bg_bg, "bg" }, { Langs.bn_in, "bn" }, { Langs.br_fr, "br" }, { Langs.bs_latn_ba, "bs" }, { Langs.ca_es, "ca" }, { Langs.cs_cz, "cs" }, { Langs.da_dk, "da" }, { Langs.de_de, "de" }, { Langs.el_gr, "el" }, { Langs.en_gb, "en" }, { Langs.es_es, "es" }, { Langs.et_ee, "et" }, { Langs.eu_es, "eu" }, { Langs.fa_ir, "fa" }, { Langs.fi_fi, "fi" }, { Langs.fr_fr, "fr" }, { Langs.gl_es, "gl" }, { Langs.he_il, "he" }, { Langs.hi_in, "hi" }, { Langs.hr_hr, "hr" }, { Langs.hu_hu, "hu" }, { Langs.hy_am, "hy" }, { Langs.id_id, "id" }, { Langs.is_is, "is" }, { Langs.it_it, "it" }, { Langs.ja_jp, "ja" }, { Langs.ka_ge, "ka" }, { Langs.ko_kr, "ko" }, { Langs.lt_lt, "lt" }, { Langs.lv_lv, "lv" }, { Langs.mk_mk, "mk" }, { Langs.ml_in, "ml" }, { Langs.ms_my, "ms" }, { Langs.nl_nl, "nl" }, { Langs.nb_no, "no" }, { Langs.pl_pl, "pl" }, { Langs.pt_pt, "pt" }, { Langs.pt_br, "pt_br" }, { Langs.ro_ro, "ro" }, { Langs.ru_ru, "ru" }, { Langs.sk_sk, "sk" }, { Langs.sl_si, "sl" }, { Langs.sq_al, "sq" }, { Langs.sr_latn_cs, "sr" }, { Langs.sv_se, "sv" }, { Langs.ta_in, "ta" }, { Langs.te_in, "te" }, { Langs.th_th, "th" }, { Langs.tr_tr, "tr" }, { Langs.uk_ua, "uk" }, { Langs.vi_vn, "vi" }, { Langs.zh_cn, "zh" }, { Langs.zh_hk, "zh_tw" },
		};
		public static HashSet<Langs> forLangs = new HashSet<Langs> {
			Langs.af_za, Langs.ar_sa, Langs.bg_bg, Langs.bn_in, Langs.br_fr, Langs.bs_latn_ba, Langs.ca_es, Langs.cs_cz, Langs.da_dk, Langs.de_de, Langs.el_gr, Langs.en_gb, Langs.es_es, Langs.et_ee, Langs.eu_es, Langs.fa_ir, Langs.fi_fi, Langs.fr_fr, Langs.gl_es, Langs.he_il, Langs.hi_in, Langs.hr_hr, Langs.hu_hu, Langs.hy_am, Langs.id_id, Langs.is_is, Langs.it_it, Langs.ja_jp, Langs.ka_ge, Langs.ko_kr, Langs.lt_lt, Langs.lv_lv, Langs.mk_mk, Langs.ml_in, Langs.ms_my, Langs.nl_nl, Langs.nb_no, Langs.pl_pl, Langs.pt_pt, Langs.pt_br, Langs.ro_ro, Langs.ru_ru, Langs.sk_sk, Langs.sl_si, Langs.sq_al, Langs.sr_latn_cs, Langs.sv_se, Langs.ta_in, Langs.te_in, Langs.th_th, Langs.tr_tr, Langs.uk_ua, Langs.vi_vn, Langs.zh_cn, Langs.zh_hk,
		};

	}

	public struct FrequencyItem {
		public string word;
		public int freq;
	}
}
