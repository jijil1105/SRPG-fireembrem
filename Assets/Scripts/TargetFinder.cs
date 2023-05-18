using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TargetFinder
{
    public class ActionPlan
    {
        public Charactor charaData;
        public MapBlock toMoveBlock;
        public Charactor toAttackChara;
    }

    public static ActionPlan GetRandomActionPlan(MapManager mapManager, CharactorManager charactorManager, List<Charactor> enemyCharas)
    {
        var actionPlans = new List<ActionPlan>();

        var reachableBlocks = new List<MapBlock>();

        var attackableBlocks = new List<MapBlock>();

        foreach(var enemyData in enemyCharas)
        {
            reachableBlocks = mapManager.SearchReachableBlocks(enemyData.XPos, enemyData.ZPos);

            foreach(var block in reachableBlocks)
            {
                attackableBlocks = mapManager.SearchAttackableBlocks(block.XPos, block.ZPos);

                foreach(var attackBlock in attackableBlocks)
                {
                    var chara = charactorManager.GetCharactor(attackBlock.XPos, attackBlock.ZPos);

                    if(chara && !chara.isEnemy)
                    {
                        var newPlan = new ActionPlan();

                        newPlan.charaData = enemyData;
                        newPlan.toMoveBlock = block;
                        newPlan.toAttackChara = chara;
                        actionPlans.Add(newPlan);
                    }
                }
            }
        }

        if (actionPlans.Count > 0)
            return actionPlans[Random.Range(0, actionPlans.Count)];
        else
            return null;
    }
}
