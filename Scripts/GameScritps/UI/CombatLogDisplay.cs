using UnityEngine;
using UnityEngine.UI;
using QFramework; // 对于此脚本当前功能而言并非严格必要，但有助于保持一致性
using System.Collections.Generic;

namespace YourGameNamespace.UI
{
    public class CombatLogDisplay : MonoBehaviour
    {
        public Text combatLogText; // 在Unity检视面板中分配
        public int maxLogLines = 10;

        private Queue<string> mLogMessages = new Queue<string>();

        void Awake()
        {
            // 这是挂钩到 Debug.Log 的一种简单方法。
            // 更健壮的解决方案可能会使用QFramework事件或专用的日志服务。
            Application.logMessageReceived += HandleLog;
            if (combatLogText == null)
            {
                Debug.LogError("战斗日志显示：combatLogText 未在检视面板中分配！");
                this.enabled = false;
            }
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            // 筛选要在战斗日志中显示的相关消息
            // 我们关心的是游戏事件，而不是所有的调试日志。
            if (type == LogType.Log || type == LogType.Warning) // 捕获常规日志和警告
            {
                // 简单的关键字过滤。更高级的过滤可以基于日志来源或特定的事件类型。
                if (logString.Contains("Zombie") || logString.Contains("attacks") || 
                    logString.Contains("ammo") || logString.Contains("defenders") || 
                    logString.Contains("Spawned") || logString.Contains("died"))
                {
                    if (mLogMessages.Count >= maxLogLines)
                    {
                        mLogMessages.Dequeue();
                    }
                    // 为清晰起见，在前面添加时间戳
                    mLogMessages.Enqueue(System.DateTime.Now.ToString("HH:mm:ss") + ": " + logString);
                    UpdateLogText();
                }
            }
        }

        void UpdateLogText()
        {
            if (combatLogText != null)
            {
                combatLogText.text = string.Join("\n", mLogMessages.ToArray());
            }
        }

        void OnDestroy()
        {
            // 对象销毁时清理处理器
            Application.logMessageReceived -= HandleLog;
        }
    }
}
