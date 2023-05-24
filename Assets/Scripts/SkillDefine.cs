using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillDefine
{
    public enum Skill
    {
        _None,// スキル：無し
        Critical,// スキル：会心の一撃：敵に与えるダメージ２倍
        DefBreak,// スキル：防御破壊：敵の防御力を０、このスキルで与えるダメージは０
        Heal,
        FireBall,
    }

    public static Dictionary<Skill, string> dec_SkillName = new Dictionary<Skill, string>()
    {
        {Skill._None, "スキル無し" },
        {Skill.Critical, "会心の一撃"},
        {Skill.DefBreak, "防御破壊"},
        {Skill.Heal, "ヒール"},
        {Skill.FireBall, "ファイアボール"},
    };

    public static Dictionary<Skill, string> dec_SkillInfo = new Dictionary<Skill, string>()
    {
        {Skill._None, "----" },
        {Skill.Critical, "ダメージ２倍の攻撃\n(１回限り)" },
        {Skill.DefBreak, "敵の防御力を０にします\n(ダメージは０)" },
        { Skill.Heal, "味方のHPを回復します" },
        {Skill.FireBall, "どの位置に居る敵も攻撃できます\n(ダメージは半分)" },
    };

    /*public static void jIOllll()
    {
        //dec_SkillName.Add()
    }*/
}
