using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スコア曲線によるスコア計算
/// </summary>
public class ScoreCalc : MonoBehaviour
{//動作確認のためLogで確認できるようにした。ver1.0.2
  /// <summary>
  /// 計算用の基礎曲線データ
  /// </summary>
  float[] pArray;//1000個分の配列,4kBほどになる。嫌ならその都度計算に
  /// <summary>
  /// 属性をまとめておくためのもの
  /// </summary>
  List<Aura> AuraList;

  void Start()
  {
    Init();
    TestLog();
  }

  /// <summary>
  /// テスト用、消しても問題ない
  /// </summary>
  void TestLog()
  {
    //テスト用に追加
    AuraList.Add(Aura.htrt50);//溌溂5が1個分

    //  AuraList.Add(Aura.htrt50);//溌溂5の2個目

    AuraList.Add(Aura.pScoreAlways);//サイズぷくスコア+(常時)

    /* チェーンによる属性や倍率ボーナス、表示用オーラの受け渡しのためにAuraクラスを作成しました。
     * ぷくそれぞれの持つ追加属性情報をまとめておくためのものです
     * 同じAuraを複数持たせたいときはAuraList.Addを複数回する必要があります
     * 独自の受け渡し機構をすでに作っている可能性が高いので、AuraListを使うものはすべてメソッドとして分離して計算式に組み込んであります
     */

    //以下すべてテスト表示用コード
    int start = 1;
    int step = 1;
    int max = 15;

    float combo = 12;
    float pucScore = 345;
    bool fever = true;

    for (int i = start; i < start + max * step; i += step)
    {
      Debug.Log("Size " + i + "→ " + BarLab(i, pucScore, combo, fever) + " / c-" + combo + " s-" + pucScore + " f-" + fever);
    }
    /*https://lab.crowbar.work/puc/score.html
     * ここにコンボ12スコア345、フィーバー＝はい
     * サイズぷくスコア+(常時)=1、溌溂の個数1、倍率5を入力した時のスコアが出る
     * 5532,11064,16599,...
     * 
     * 一応いろいろ調整したんですが、JavaScriptとの数値の取り扱いの違いで精度が合わず、999サイズ＆スコア500以上などの大きい数値になると一の位が合わなかったりします。
     */
  }

  /// <summary>
  /// 初期設定
  /// </summary>
  private void Init()
  {
    pArray = new float[1000];
    FillPArray();
    AuraList = new List<Aura>();
  }
  /// <summary>
  /// スコアの基礎となる配列を計算する
  /// </summary>
  private void FillPArray()
  {
    int n;
    float delta;

    //初期値の設定
    pArray[0] = pArray[1] = 1.95f;
    float tempSigma4_5k = 0f;//Alphaの計算式のsigma

    for (n = 2; n <= 120; n++)
    {
      tempSigma4_5k += Mathf.Ceil((4f / 5f) * (n - 1));

      //記述に忠実に組むとこうだが計算がかなり無駄なので
      //     pArray[n] = pArray[n - 1] + AlphaN(n);
      pArray[n] = pArray[n - 1] + tempSigma4_5k / 200f;
      //  Debug.Log("parr" + n + " " + pArray[n]);
    }
    delta = AlphaN(120);//ALPHA120

    for (; n <= 150; n++)
    {
      delta = delta - GammaN(n);
      pArray[n] = pArray[n - 1] + delta;

    }
    //この時点でdelta=DELTA150

    for (; n <= 200; n++)
    {
      pArray[n] = pArray[n - 1] + delta - (n - 150) * 0.25f;

    }
    for (; n < 1000; n++)
    {//floatの精度の問題で999は2253.251となる。本来なら2253.27
      //2000>>0.019で誤差レベルとして無視する
      //double型なら防げるが1000の配列をメモリに置いておくためfloatのまま
      //
      pArray[n] = pArray[n - 1] + 0.1f;
    }

  }

  /// <summary>
  /// バールラボのスコア計算機を基にしたスコア算出関数
  /// </summary>
  /// <param name="size"></param>
  /// <param name="pucScore"></param>
  /// <param name="combo"></param>
  /// <param name="isFever"></param>
  public double BarLab(float sizeF, float pucScoreF, float comboF, bool isFever)
  {//https://lab.crowbar.work/
    //https://docs.google.com/spreadsheets/d/1N7Tf3bgzD8YdZjG6tWZ0wuAFB3Ui-L4MMBnfRskYCRA/edit#gid=1483832889
    //スプレッドシートでのLP,LQなどの項目
    double p, q, r, labScore;
    double size = sizeF;
    double pucScore = pucScoreF;
    double combo = comboF;

    p = Math.Floor(Math.Floor(pucScore * CalcPsRatio(AuraList)) * CalcPsPlus(AuraList, isFever, size));
    if (size > 200)
    {
      q = pArray[200] * 200f + (size - 200) * 20;
    }
    else
    {
      q = pArray[(int)size] * 200;
    }

    r = 1 + (CalcComboBonus(AuraList) - 1) + combo / 200;

    labScore = Math.Floor(p * CalcHtrtRatio(AuraList)) * size;
    if (size > 9)
    {
      labScore += p * 2 * (size - 9) + q;
    }//9以下は何もしない

    labScore = Math.Floor(labScore * r) * (isFever ? 3f : 1f);

    return labScore;
  }

  /// <summary>
  /// コンボボーナスを求める
  /// </summary>
  /// <param name="AuraList"></param>
  /// <returns></returns>
  private double CalcComboBonus(List<Aura> AuraList)
  {
    double comboBonus = 1;

    foreach (var item in AuraList)
    {
      if (item.type == AuraType.ComboBonus)
      {
        comboBonus += (double)item.value / 1000;
      }
    }

    return comboBonus;
  }
  /// <summary>
  /// ぷく倍率を求める
  /// </summary>
  /// <param name="AuraList"></param>
  /// <returns></returns>
  private double CalcPsRatio(List<Aura> AuraList)
  {
    double psRatio = 1;
    foreach (var item in AuraList)
    {
      if (item.type == AuraType.ScoreRatioModifier)
      {
        psRatio += (double)item.value / 1000;
      }
    }
    return psRatio;
  }
  /// <summary>
  /// サイズぷくスコア+(各種)を求める
  /// </summary>
  /// <param name="AuraList"></param>
  /// <returns></returns>
  private double CalcPsPlus(List<Aura> AuraList, bool isFever, double size)
  {
    double psPlus = 1;

    foreach (var item in AuraList)
    {
      switch (item.type)
      {
        case AuraType.ScoreCalcModifier://常時
          psPlus += (double)item.value / 1000;
          break;
        case AuraType.ScoreCalcModifierOnFever://フィーバー時
          if (isFever == true)
            psPlus += (double)item.value / 1000;
          break;
        case AuraType.SizeScorePlus20://20以上
          if (size >= 20)
            psPlus += (double)item.value / 1000;
          break;
        case AuraType.SizeScorePlusU15://15未満
          if (size < 15)
            psPlus += (double)item.value / 1000;
          break;
        default:
          break;
      }
    }

    return psPlus;
  }
  /// <summary>
  /// 溌剌の倍率(スコア直接補正)の倍率を求める
  /// </summary>
  /// <param name="hatsuratuRatio"></param>
  /// <returns></returns>
  private double CalcHtrtRatio(List<Aura> AuraList)
  {
    bool firstCheck = true;
    double hatsuratuRatio = 1;
    foreach (var item in AuraList)
    {
      if (item.type == AuraType.HatsuratsuBonus)
      {
        if (firstCheck)
        {
          firstCheck = false;
          hatsuratuRatio = item.number / 10;
        }
        else hatsuratuRatio += item.value / 1000;
      }

    }

    return hatsuratuRatio;
  }


  //18-0921によるP(n)の計算関連、BarLabでも使用中
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n"></param>
  /// <returns></returns>
  float Pn(int n)
  {
    float zeta;

    const float ALPHA120 = 28.8f;
    const float GAMMA150 = 1.065f;

    zeta = ZetaN(n, ALPHA120, GAMMA150);

    return 1f;
  }
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n"></param>
  /// <param name="ALPHA120"></param>
  /// <param name="GAMMA150"></param>
  /// <returns></returns>
  private float ZetaN(int n, float ALPHA120, float GAMMA150)
  {
    float delta, zeta;
    if (n <= 120)//1~120
    {
      zeta = AlphaN(n);
    }
    else if (n <= 150)//120~150
    {
      delta = ALPHA120;
      for (int i = 120; i < n; i++)
      {
        delta -= GammaN(n);
      }
      zeta = delta;
    }
    else if (n <= 200)//150~200
    {
      zeta = EpsilonN(n, GAMMA150);
    }
    else//200~
    {
      zeta = 0.1f;
    }

    return zeta;
  }
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n"></param>
  /// <param name="GAMMA150"></param>
  /// <returns></returns>
  private static float EpsilonN(int n, float GAMMA150)
  {
    return GAMMA150 - (n - 150) * 0.25f;
  }
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n"></param>
  /// <returns></returns>
  private static float AlphaN(int n)
  {
    float sigma = 0f;
    for (int k = 1; k < n; k++)//1~(n-1)まで
    {
      sigma += Mathf.Ceil((4f / 5f) * k);
    }
    return sigma / 200f;
  }
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n"></param>
  /// <returns></returns>
  float BetaN(int n)
  {
    if (n % 5 == 2)
      return 0.04f;
    else if (n % 5 == 3)
      return 0.075f;
    else if (n % 5 == 4)
      return 0.11f;
    else if (n % 5 == 0)
      return 0.15f;
    else return 0f;
  }
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n"></param>
  /// <returns></returns>
  float GammaN(int n)
  {
    float beta = BetaN(n);

    return 0.18f * Mathf.Floor((n - 121) / 5f) + beta + 0.015f;
  }

  //ここより下は消しても問題ない
  /// <summary>
  /// バール氏の記述を基にしたスコア計算18-0921
  /// </summary>
  /// <param name="size"></param>
  /// <param name="score"></param>
  /// <param name="combo"></param>
  void Bar_exdnkrs(float size, float score, float combo)
  {//https://twitter.com/exdnkrs/status/1043162002267365378
   //バール氏のスコア計算関連。サイズ10未満で計算がおかしくなるので中止
   //1~9は(pArray+score)*combo/200で大体あってるしタップでつぶした時のスコアなのでスコアタに影響はない

    float p = 0f, q = 0f, r = 0f, s = 0f;
    float answer;

    const bool feverStat = false;
    const float SKILL_SCORE_RATE = 1f;//状況により変わる

    float comboBonus = 1f;
    float feverBonus = feverStat == true ? 3f : 1f;

    //邪魔なのでコメントアウト、こちらが正式
    //   p= pArray[(int)size];
    p = Pn((int)size);//エラー出ない程度に残す
    q = Mathf.Floor(score * 1f) * SKILL_SCORE_RATE;
    r = 0.015f * (size - 6f);
    //コンボ500で上限を追加
    s = (combo > 500 ? 500 : combo) + 200 * comboBonus;//これは違う可能性がある


    answer = Mathf.Floor((p + q * r) * s) * feverBonus;
    // answer = Mathf.Floor(Mathf.Pow((p + q * r) * s, Mathf.Pow(AGHOST_CONST, SKILL_TIMES))) * feverBonus;
    /* スキル倍率がかかるたびにその商を０．９乗する→まとめて0.9のn乗をかける
     * const float AGHOST_CONST = 0.9f;
     * const int SKILL_TIMES = 1;//スキル倍率がかかった回数
     */
  }
}


/// <summary>
/// ぷくに付与する属性情報
/// </summary>
public class Aura
{//12Byte
  /// <summary>
  /// 属性の種類
  /// </summary>
  public AuraType type;
  /// <summary>
  /// 属性に関する数値(int)
  /// </summary>
  public int number;
  /// <summary>
  /// 属性に関する数値(float)
  /// </summary>
  public float value;

  //精度の関係で1000倍の値を使用する
  //スコア計算関連定数(変数)
  /// <summary>
  /// 溌溂5.0の時のデータ
  /// </summary>
  public static readonly Aura htrt50 = new Aura(AuraType.HatsuratsuBonus, 50, 3333f);
  public static readonly Aura htrt52 = new Aura(AuraType.HatsuratsuBonus, 52, 3536f);
  public static readonly Aura htrt54 = new Aura(AuraType.HatsuratsuBonus, 54, 3753f);
  public static readonly Aura htrt56 = new Aura(AuraType.HatsuratsuBonus, 56, 3964f);
  public static readonly Aura htrt58 = new Aura(AuraType.HatsuratsuBonus, 58, 4175f);
  public static readonly Aura htrt60 = new Aura(AuraType.HatsuratsuBonus, 60, 4386f);

  //小道具関連
  //https://docs.google.com/spreadsheets/d/1N7Tf3bgzD8YdZjG6tWZ0wuAFB3Ui-L4MMBnfRskYCRA/edit#gid=1483832889
  public static readonly Aura pScoreAlways = new Aura(AuraType.ScoreCalcModifier, 0, 10f);//1%なら10f
  public static readonly Aura pScoreFever = new Aura(AuraType.ScoreCalcModifierOnFever, 0, 12f);//1.2％なら12f
  public static readonly Aura pScoreRatio = new Aura(AuraType.ScoreRatioModifier, 0, 3000f);//4倍なら(4-1)*1000=3000f
  public static readonly Aura ComboBonus = new Aura(AuraType.ComboBonus, 0, 20f);//2%なら20f


  /// <summary>
  /// コンストラクタ
  /// </summary>
  public Aura(AuraType type, int number, float value)
  {
    this.type = type;
    this.number = number;
    this.value = value;
  }
  /// <summary>
  /// ゼロにする
  /// </summary>
  public void Init()
  {
    type = AuraType.None;
    number = 0;
    value = 0f;
  }
}

/// <summary>
/// 属性の種類やスコア倍率を適用するタイミングを定義
/// </summary>
public enum AuraType
{//(int)AuraType.xxxで3つ目のパラメータとして使用可能
  //一般
  /// <summary>
  /// 属性なし
  /// </summary>
  None = 0,
  /// <summary>
  /// サイズを変更する
  /// </summary>
  SizeModifier,
  /// <summary>
  /// 重さの変更
  /// </summary>
  MassModifier,
  /// <summary>
  /// 素点を変更する
  /// </summary>
  BaseScoreChanger,
  /// <summary>
  ///見た目を変更する
  /// </summary>
  SpriteChanger,
  /// <summary>
  ///表示されるオーラを追加する
  /// </summary>
  WearAura,
  /// <summary>
  /// 位置を固定する（ガラスの靴）
  /// </summary>
  PositionAnchor,
  /// <summary>
  /// チェーン可能不可能の属性（ボス、シンボル）
  /// </summary>
  ChainableChanger,

  //小道具
  /// <summary>
  /// スコア＋
  /// </summary>
  ScoreCalcModifier,
  /// <summary>
  /// フィーバー時スコア＋
  /// </summary>
  ScoreCalcModifierOnFever,
  /// <summary>
  /// スコア倍率の変更
  /// </summary>
  ScoreRatioModifier,
  /// <summary>
  /// スキルゲージのたまり方の変更
  /// </summary>
  SkillGaugeBonus,
  /// <summary>
  /// コンボボーナスの変更
  /// </summary>
  ComboBonus,
  /// <summary>
  /// フィーバーゲージのたまり方のボーナス
  /// </summary>
  FeverGaugeChargeBonus,
  /// <summary>
  /// 20以上スコア+
  /// </summary>
  SizeScorePlus20,
  /// <summary>
  /// 15未満スコア+
  /// </summary>
  SizeScorePlusU15,

  //スキル関連
  /// <summary>
  /// スコアに直接倍率補正を行う
  /// </summary>
  HatsuratsuBonus,
  /// <summary>
  /// スコア倍率の変更
  /// </summary>
  ScoreRateBonus,
}
