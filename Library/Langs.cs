﻿using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace Langs {

  public class Meta {
    public byte Idx;
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

  public struct PhraseSide {
    public Langs src;
    public Langs dest;
  }

  public enum Langs {
    _ = 0,
    af_za = 1,
    ar_sa = 2,
    as_in = 3,
    az_latn_az = 4,
    be_by = 5,
    bg_bg = 6,
    bn_in = 7,
    bo_cn = 8,
    br_fr = 9,
    bs_latn_ba = 10,
    ca_es = 11,
    co_fr = 12,
    cs_cz = 13,
    da_dk = 14,
    de_de = 15,
    el_gr = 16,
    en_gb = 17,
    en_us = 18,
    es_es = 19,
    et_ee = 20,
    eu_es = 21,
    fa_ir = 22,
    fi_fi = 23,
    fr_fr = 24,
    ga_ie = 25,
    gl_es = 26,
    gu_in = 27,
    ha_latn_ng = 28,
    he_il = 29,
    hi_in = 30,
    hr_hr = 31,
    hu_hu = 32,
    hy_am = 33,
    id_id = 34,
    ig_ng = 35,
    is_is = 36,
    it_it = 37,
    ja_jp = 38,
    ka_ge = 39,
    km_kh = 40,
    kn_in = 41,
    ko_kr = 42,
    ky_kg = 43,
    lt_lt = 44,
    lv_lv = 45,
    mi_nz = 46,
    mk_mk = 47,
    ml_in = 48,
    mn_mn = 49,
    mr_in = 50,
    ms_my = 51,
    mt_mt = 52,
    nb_no = 53,
    nl_nl = 54,
    nso_za = 55,
    oc_fr = 56,
    pa_in = 57,
    pl_pl = 58,
    ps_af = 59,
    pt_br = 60,
    pt_pt = 61,
    quz_pe = 62,
    ro_ro = 63,
    ru_ru = 64,
    sk_sk = 65,
    sl_si = 66,
    sq_al = 67,
    sr_latn_cs = 68,
    sv_se = 69,
    sw_ke = 70,
    ta_in = 71,
    te_in = 72,
    th_th = 73,
    tn_za = 74,
    tr_tr = 75,
    uk_ua = 76,
    ur_pk = 77,
    uz_latn_uz = 78,
    vi_vn = 79,
    xh_za = 80,
    yo_ng = 81,
    zh_cn = 82,
    zh_hk = 83,
    zu_za = 84,
  }

}
