using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AOT
{
    public sealed class GameManager : MonoBehaviour
    {
        private PlayerData m_CurrentData;
        private const string file_name = "PlayerInfo.json";

        async UniTaskVoid Start()
        {
            m_CurrentData = await JsonMgr.instance.LoadJsonDataAsync<PlayerData>(file_name, GamePath.DataType.Save);

            if (m_CurrentData != null)
            {
                Debug.Log($"读取成功！玩家: {m_CurrentData.playerName}, 等级: {m_CurrentData.level}");

                // 模拟数据修改并保存
                m_CurrentData.level += 1;
                await JsonMgr.instance.SaveJsonDataAsync(file_name, m_CurrentData, GamePath.DataType.Save);
            }
            else
            {
                Debug.LogWarning("读取数据失败，初始化一份默认数据...");
                m_CurrentData = new PlayerData { playerName = "New Player", level = 1 };
            }
            // 1. 获取句柄
            var handle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Cube.prefab");
            // 2. 等待结果
            var prefab = await handle.Task;

            if (prefab != null)
            {
                Instantiate(prefab);
            }
        }
    }

    public class PlayerData
    {
        public string playerName;
        public int level;
    }
}