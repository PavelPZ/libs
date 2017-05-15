using System.Globalization;
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
    no = 0,
    af_za = 1,
    sq_al = 2,
    ar_sa = 3,
    hy_am = 4,
    as_in = 5,
    az_latn_az = 6,
    bn_in = 7,
    eu_es = 8,
    be_by = 9,
    bs_latn_ba = 10,
    br_fr = 11,
    bg_bg = 12,
    ca_es = 13,
    co_fr = 14,
    hr_hr = 15,
    cs_cz = 16,
    da_dk = 17,
    nl_nl = 18,
    en_us = 19,
    en_gb = 20,
    et_ee = 21,
    fi_fi = 22,
    fr_fr = 23,
    gl_es = 24,
    ka_ge = 25,
    de_de = 26,
    el_gr = 27,
    gu_in = 28,
    ha_latn_ng = 29,
    he_il = 30,
    hi_in = 31,
    hu_hu = 32,
    zh_cn = 33,
    zh_hk = 34,
    is_is = 35,
    ig_ng = 36,
    id_id = 37,
    ga_ie = 38,
    it_it = 39,
    ja_jp = 40,
    kn_in = 41,
    km_kh = 42,
    sw_ke = 43,
    ko_kr = 44,
    ky_kg = 45,
    lv_lv = 46,
    lt_lt = 47,
    mk_mk = 48,
    ms_my = 49,
    ml_in = 50,
    mt_mt = 51,
    mi_nz = 52,
    mr_in = 53,
    mn_mn = 54,
    nb_no = 55,
    oc_fr = 56,
    ps_af = 57,
    fa_ir = 58,
    pl_pl = 59,
    pt_br = 60,
    pt_pt = 61,
    pa_in = 62,
    quz_pe = 63,
    ro_ro = 64,
    ru_ru = 65,
    sr_latn_cs = 66,
    nso_za = 67,
    tn_za = 68,
    sk_sk = 69,
    sl_si = 70,
    es_es = 71,
    sv_se = 72,
    ta_in = 73,
    te_in = 74,
    th_th = 75,
    bo_cn = 76,
    tr_tr = 77,
    uk_ua = 78,
    ur_pk = 79,
    uz_latn_uz = 80,
    vi_vn = 81,
    xh_za = 82,
    yo_ng = 83,
    zu_za = 84,
  }

}
