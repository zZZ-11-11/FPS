using System.Collections.Generic;
using UnityEngine;

namespace FPS.AI
{
    public sealed class PatrolPath : MonoBehaviour
    {
        [Tooltip("被分配到该巡逻路线上的敌人")]
        public List<EnemyController> enemiesToAssign = new List<EnemyController>();

        [Tooltip("构成路径的点")]
        public List<Transform> pathNodes = new List<Transform>();

        void Start()
        {
            foreach (var enemy in enemiesToAssign)
            {
                enemy.patrolPath = this;
            }
        }

        //获取当前位置到目标节点的距离
        public float GetDistanceToNode(Vector3 origin, int destinationNodeIndex)
        {
            if (destinationNodeIndex < 0 || destinationNodeIndex >= pathNodes.Count ||
                pathNodes[destinationNodeIndex] == null)
            {
                return -1f;
            }

            return (pathNodes[destinationNodeIndex].position - origin).magnitude;
        }

        //根据索引号返回该节点在世界中的坐标
        public Vector3 GetPositionOfPathNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= pathNodes.Count || pathNodes[nodeIndex] == null)
            {
                return Vector3.zero;
            }

            return pathNodes[nodeIndex].position;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (var i = 0; i < pathNodes.Count; i++)
            {
                var nextIndex = i + 1;
                if (nextIndex >= pathNodes.Count)
                {
                    nextIndex -= pathNodes.Count;
                }

                Gizmos.DrawLine(pathNodes[i].position, pathNodes[nextIndex].position);
                Gizmos.DrawSphere(pathNodes[i].position, 0.1f);
            }
        }
    }
}