using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// スコアを画像で表示するシステムのコントローラ
/// </summary>
public class ScoreCalc : MonoBehaviour
{
  //バール氏のスコア計算関連

  /// <summary>
  /// バール氏の記述を基にしたスコア計算
  /// </summary>
  /// <param name="size">サイズ</param>
  /// <param name="score">スコア</param>
  /// <param name="combo">コンボ数</param>
  void Bar_exdnkrs(float size, float score, float combo)
  {//https://twitter.com/exdnkrs/status/1043162002267365378
    float p = 0f, q = 0f, r = 0f, s = 0f;
    float answer;

    const bool feverStat = true;
    const float SKILL_SCORE_RATE = 1f;//状況により変わる

    float comboBonus = feverStat == true ? 1f : 3f;
    float feverBonus = feverStat == true ? 1f : 3f;

    //邪魔なのでコメントアウト、こちらが正式
    //   p= pArray[(int)size];
    p = Pn((int)size);//エラー出ない程度に残す
    q = Mathf.Floor(score * 1f) * SKILL_SCORE_RATE;
    r = 0.015f * (size - 6f);
    //コンボ500で上限を追加
    s = (combo > 500f ? 500f : combo) + 200f * comboBonus;//これは違う可能性がある


    answer = Mathf.Floor((p + q * r) * s) * feverBonus;


    Debug.Log("ScoreCalc size " + size + "/score " + score + "/combo " + combo + " = ans " + answer);

    // answer = Mathf.Floor(Mathf.Pow((p + q * r) * s, Mathf.Pow(AGHOST_CONST, SKILL_TIMES))) * feverBonus;
    /* スキル倍率がかかるたびにその商を0.9乗する→まとめて0.9のn乗をかける
     * const float AGHOST_CONST = 0.9f;
     * const int SKILL_TIMES = 1;//スキル倍率がかかった回数
     */
  }

  //邪魔なのでコメントアウト、こちらが正式
  // static float[] pArray=new float[1000];

  /// <summary>
  /// スコアの基礎となる配列を計算する
  /// </summary>
  void FillPArray()
  {
    int n;
    float delta;
    //エラー出ない程度に残す
    float[] pArray = new float[1000];

    pArray[0] = 1.95f;

    for (n = 1; n <= 120; n++)
    {
      pArray[n] = pArray[n - 1] + AlphaN(n);
    }

    delta = AlphaN(120);//ALPHA120

    for (; n < 150; n++)
    {
      delta = delta - GammaN(n);
      pArray[n] = pArray[n - 1] + delta;
    }
    //この時点でdelta=DELTA150
    for (; n < 200; n++)
    {
      delta = delta - GammaN(n);
      pArray[n] = pArray[n - 1] + delta - (n - 150) * 0.25f;
    }
    for (; n < 1000; n++)
    {
      pArray[n] = pArray[n - 1] + 0.1f;
    }

  }
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n">n</param>
  /// <returns></returns>
  float Pn(int n)
  {
    float zeta;

    float ALPHA120 = 1f;//とりあえず
    float GAMMA150 = 1f;//とりあえず

    zeta = ZetaN(n, ALPHA120, GAMMA150);

    return 1f;
  }
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n"></param>
  /// <param name="ALPHA120">alpha(120)</param>
  /// <param name="GAMMA150">gamma(150)</param>
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
  /// <param name="n">n</param>
  /// <param name="GAMMA150">gamma(150)</param>
  /// <returns></returns>
  private static float EpsilonN(int n, float GAMMA150)
  {
    return GAMMA150 - (n - 150) * 0.25f;
  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="n"></param>
  /// <returns></returns>
  private static float AlphaN(int n)
  {
    float sigma = 0f;
    for (int k = 0; k < n - 1; k++)
    {
      sigma += Mathf.Ceil((4f / 5f) * k);
    }
    return sigma / 200f;

  }
  /// <summary>
  /// スコアの計算の関数
  /// </summary>
  /// <param name="n">n</param>
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
  /// <param name="n">n</param>
  /// <returns></returns>
  float GammaN(int n)
  {
    float beta = BetaN(n);

    return 0.18f * Mathf.Floor((n - 121) / 5f) + beta + 0.015f;
  }

  void Start()
  {
    for (int i = 0; i < 10; i++)
    {//テスト用、これだと毎回1000のサイズの配列を計算している
      Bar_exdnkrs(10f, 555f, i * 50);

    }
  }

}
