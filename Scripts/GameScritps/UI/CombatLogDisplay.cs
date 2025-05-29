using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using QFramework; // 对于此脚本的当前功能而言并非严格必要，但引入它是为了与项目其他部分保持架构上的一致性
using System.Collections.Generic; // 用于 Queue<string> (字符串队列)

namespace YourGameNamespace.UI
{
    // 战斗日志显示组件，用于在UI上显示与战斗相关的日志信息
    public class CombatLogDisplay : MonoBehaviour
    {
        public Text combatLogText; // 在Unity检视面板中分配此Text组件，用于显示战斗日志
        public int maxLogLines = 10; // 日志区域显示的最大行数

        private Queue<string> mLogMessages = new Queue<string>(); // 用于存储日志消息的队列

        void Awake() // Unity生命周期方法，当脚本实例被创建时调用
        {
            // 这是一种简单的方法来挂钩Unity的 Debug.Log 输出。
            // 一个更健壮的解决方案可能会使用QFramework的事件系统或一个专门的日志服务来更精确地捕获和分发日志。
            Application.logMessageReceived += HandleLog; // 订阅Unity的日志消息接收事件
            if (combatLogText == null)
            {
                Debug.LogError("战斗日志显示 (CombatLogDisplay)：UI Text组件 (combatLogText) 未在检视面板中分配！此组件将无法正常工作。");
                this.enabled = false; // 禁用此脚本组件
            }
        }

        // 处理从Unity接收到的日志消息
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            // 筛选出需要在战斗日志UI中显示的相关消息。
            // 我们通常更关心游戏逻辑产生的事件日志，而不是所有的引擎调试信息。
            if (type == LogType.Log || type == LogType.Warning) // 通常捕获常规日志 (Log) 和警告 (Warning) 类型的消息
            {
                // 进行简单的基于关键字的过滤。
                // 更高级的过滤机制可以基于日志的来源脚本、特定的事件标签或更复杂的规则。
                // 注意：由于其他脚本中的日志消息已被翻译成中文，这里的关键词也应使用中文。
                if (logString.Contains("僵尸") || logString.Contains("攻击") ||  // "Zombie", "attacks"
                    logString.Contains("弹药") || logString.Contains("防御者") || // "ammo", "defenders"
                    logString.Contains("生成") || logString.Contains("死亡"))   // "Spawned", "died"
                {
                    if (mLogMessages.Count >= maxLogLines) // 如果日志队列已满
                    {
                        mLogMessages.Dequeue(); // 移除最早的一条消息
                    }
                    // 为增加日志可读性，在每条消息前添加时间戳
                    mLogMessages.Enqueue(System.DateTime.Now.ToString("HH:mm:ss") + ": " + logString);
                    UpdateLogText(); // 更新UI上显示的日志文本
                }
            }
        }

        // 更新UI Text组件以显示当前队列中的日志消息
        void UpdateLogText()
        {
            if (combatLogText != null)
            {
                // 将队列中的所有消息用换行符连接成一个字符串，并设置为Text组件的文本
                combatLogText.text = string.Join("\n", mLogMessages.ToArray());
            }
        }

        void OnDestroy() // Unity生命周期方法，当对象被销毁时调用
        {
            // 在对象销毁时，取消订阅Unity的日志消息接收事件，以防止内存泄漏或错误调用
            Application.logMessageReceived -= HandleLog;
        }
    }
}
