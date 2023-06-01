using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // オブジェクト・プレハブ(インスペクタから指定)
    public Transform blockParent; // マップブロックの親オブジェクトのTransform
    public GameObject blockParentobj;
    public GameObject blockPrefab_Grass; // 草ブロック
    public GameObject blockPrefab_Water; // 水場ブロック

    //-------------------------------------------------------------------------

    // マップデータ
    public MapBlock[,] mapBlocks;

    //-------------------------------------------------------------------------

    // 定数定義
    [SerializeField]
    public int MAP_WIDTH = 9;// マップの横幅
    [SerializeField]
    public int MAP_HEIGHT = 9;// マップの縦(奥行)の幅
    [SerializeField]
    //private int GENERATE_RATIO_GRASS = 80;// 草ブロックが生成される確率

    //-------------------------------------------------------------------------

    void Start()
    {
        //マップを手動で生成する場合

        // マップデータを初期化
        mapBlocks = new MapBlock[MAP_WIDTH, MAP_HEIGHT];

        //手動で配置したブロックを取得
        var objs = blockParentobj.GetComponentsInChildren<MapBlock>();

        // ブロック生成位置の基点となる座標を設定
        Vector3 defaultPos = new Vector3(0.0f, 0.0f, 0.0f);
        defaultPos.x = -(MAP_WIDTH / 2);
        defaultPos.z = -(MAP_HEIGHT / 2);

        int index = 0;

        //取得したブロックを２次元配列で管理して２次元座標と比較しやすくする
        for (int i = 0; i < MAP_WIDTH; i++)
        {
            for(int j = 0; j < MAP_HEIGHT; j++)
            {
                Vector3 pos = defaultPos;
                pos.x += i;
                pos.z += j;

                mapBlocks[i, j] = objs[index];
                mapBlocks[i, j].transform.position = pos;
                mapBlocks[i, j].XPos = (int)pos.x;
                mapBlocks[i, j].ZPos = (int)pos.z;
                if (index < (MAP_WIDTH * MAP_HEIGHT))
                    index++;
            }
        }

        //-------------------------------------------------------------------------
        //マップをランダムで生成する場合

        /*// ブロック生成処理
        for (int i = 0; i < MAP_WIDTH; i++)
        {
            for (int j = 0; j < MAP_HEIGHT; j++)
            {
                Vector3 pos = defaultPos;
                pos.x += i;
                pos.z += j;

                // ブロックの種類を決定
                int rand = Random.Range(0, 100);
                bool isGrass = false;

                if (rand < GENERATE_RATIO_GRASS) { isGrass = true; }

                GameObject obj;

                if (isGrass) { obj = Instantiate(blockPrefab_Grass, blockParent); }// blockParentの子に草ブロックを生成

                else { obj = Instantiate(blockPrefab_Water, blockParent); }// blockParentの子に水場ブロックを生成

                obj.transform.position = pos;

                // 配列mapBlocksにブロックデータを格納
                var mapBlock = obj.GetComponent<MapBlock>();
                mapBlocks[i, j] = mapBlock;

                // ブロックデータ設定
                mapBlock.XPos = (int)pos.x;
                mapBlock.ZPos = (int)pos.z;
            }
        }*/
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// 全てのブロックの選択状態を解除する
	/// </summary>
    public void AllSelectionModeClear()
    {
        for (int i = 0; i < MAP_WIDTH; i++)
            for (int j = 0; j < MAP_HEIGHT; j++)
                mapBlocks[i, j].SetSelectionMode(MapBlock.Highlight.Off);
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// 渡された位置からキャラクターが到達できる場所のブロックをリストにして返す
	/// </summary>
	/// <param name="xPos">基点x位置</param>
	/// <param name="zPos">基点z位置</param>
	/// <returns>条件を満たすブロックのリスト</returns>
    public List<MapBlock> SearchReachableBlocks(int xPos, int zPos)
    {
        var results = new List<MapBlock>();

        int baseX = -1, baseZ = -1;

        for(int i=0; i<MAP_WIDTH; i++)
        {
            for(int j=0; j<MAP_HEIGHT; j++)
            {
                if((mapBlocks[i,j].XPos == xPos)&&(mapBlocks[i,j].ZPos == zPos))
                {
                    baseX = i;
                    baseZ = j;
                    break;
                }
            }
            if (baseX != -1) { break; }
        }


        var moveChara = GetComponent<CharactorManager>().GetCharactor(xPos, zPos);

        if(moveChara)
        {
            if(moveChara.moveType == Charactor.MoveType.Rook || moveChara.moveType == Charactor.MoveType.Queen)
            {
                for (int i = baseX + 1; i < MAP_WIDTH; i++)
                    if (AddReachableList(results, mapBlocks[i, baseZ]))
                        break;

                for (int i = baseX - 1; i >= 0; i--)
                    if (AddReachableList(results, mapBlocks[i, baseZ]))
                        break;

                for (int i = baseZ + 1; i < MAP_HEIGHT; i++)
                    if (AddReachableList(results, mapBlocks[baseX, i]))
                        break;

                for (int i = baseZ - 1; i >= 0; i--)
                    if (AddReachableList(results, mapBlocks[baseX, i]))
                        break;
            }

            if (moveChara.moveType == Charactor.MoveType.Bishop || moveChara.moveType == Charactor.MoveType.Queen)
            {
                for (int i = baseX + 1, j = baseZ + 1; i < MAP_WIDTH && j < MAP_HEIGHT; i++, j++)
                    if (AddReachableList(results, mapBlocks[i, j]))
                        break;

                for (int i = baseX - 1, j = baseZ + 1; i >= 0 && j < MAP_HEIGHT; i--, j++)
                    if (AddReachableList(results, mapBlocks[i, j]))
                        break;

                for (int i = baseX + 1, j = baseZ - 1; i < MAP_WIDTH && j >= 0; i++, j--)
                    if (AddReachableList(results, mapBlocks[i, j]))
                        break;

                for (int i = baseX - 1, j = baseZ - 1; i >= 0 && j >= 0; i--, j--)
                    if (AddReachableList(results, mapBlocks[i, j]))
                        break;


            }
        }

        results.Add(mapBlocks[baseX, baseZ]);

        return results;
    }

    //-------------------------------------------------------------------------

    /// <summary>
	/// (キャラクター到達ブロック検索処理用)
	/// 指定したブロックを到達可能ブロックリストに追加する
	/// </summary>
	/// <param name="reachableList">到達可能ブロックリスト</param>
	/// <param name="targetBlock">対象ブロック</param>
	/// <returns>行き止まりフラグ(行き止まりならtrueが返る)</returns>
    private bool AddReachableList(List<MapBlock> reachableList, MapBlock targetBlock)
    {
        if (!targetBlock.passable)
            return true;

        var charaData = GetComponent<CharactorManager>().GetCharactor(targetBlock.XPos, targetBlock.ZPos);
        if (charaData != null)
            return false;

        reachableList.Add(targetBlock);
        return false;
    }

    /// <summary>
	/// 渡された位置からキャラクターが攻撃できる場所のマップブロックをリストにして返す
	/// </summary>
	/// <param name="xPos">基点x位置</param>
	/// <param name="zPos">基点z位置</param>
	/// <returns>条件を満たすマップブロックのリスト</returns>
    public List<MapBlock> SearchAttackableBlocks(int xPos, int zPos)
    {
        var results = new List<MapBlock>();

        int baseX = -1, baseZ = -1;

        for (int i = 0; i < MAP_WIDTH; i++)
        {
            for(int j = 0; j < MAP_HEIGHT; j++)
            {
                if(mapBlocks[i, j].XPos == xPos && mapBlocks[i, j].ZPos == zPos)
                {
                    baseX = i;
                    baseZ = j;
                    break;
                }
            }
            if (baseX != -1)
                break;
        }

        AddAttackableList(results, baseX + 1, baseZ);

        AddAttackableList(results, baseX - 1, baseZ);

        AddAttackableList(results, baseX, baseZ + 1);

        AddAttackableList(results, baseX, baseZ - 1);

        AddAttackableList(results, baseX + 1, baseZ + 1);
        // X-Z+方向
        AddAttackableList(results, baseX - 1, baseZ + 1);
        // X+Z-方向
        AddAttackableList(results, baseX + 1, baseZ - 1);
        // X-Z-方向
        AddAttackableList(results, baseX - 1, baseZ - 1);

        return results;
    }

    /// <summary>
	/// (キャラクター攻撃可能ブロック検索処理用)
	/// マップデータの指定された配列内番号に対応するブロックを攻撃可能ブロックリストに追加する
	/// </summary>
	/// <param name="attackableList">攻撃可能ブロックリスト</param>
	/// <param name="indexX">X方向の配列内番号</param>
	/// <param name="indexZ">Z方向の配列内番号</param>
    private void AddAttackableList(List<MapBlock> attackableList, int indexX, int indexZ)
    {
        if(indexX < 0 || indexX >= MAP_WIDTH || indexZ < 0 || indexZ >= MAP_HEIGHT )
            return;

        attackableList.Add(mapBlocks[indexX, indexZ]);
    }

    /// <summary>
	/// マップデータ配列をリストにして返す
	/// </summary>
	/// <returns>マップデータのリスト</returns>
	public List<MapBlock> MapBlocksToList()
    {
        // 結果用リスト
        var results = new List<MapBlock>();

        // マップデータ配列の中身を順番にリストに格納
        for (int i = 0; i < MAP_WIDTH; i++)
        {
            for (int j = 0; j < MAP_HEIGHT; j++)
            {
                results.Add(mapBlocks[i, j]);
            }
        }

        return results;
    }
}
