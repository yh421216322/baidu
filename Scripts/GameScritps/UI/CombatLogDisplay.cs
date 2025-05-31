using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System.Collections.Generic;

namespace YourGameNamespace.UI
{
    public class CombatLogDisplay : MonoBehaviour, IController
    {
        public Text combatLogText;
        public int maxLogLines = 10;

        private Queue<string> mLogMessages = new Queue<string>();

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            if (combatLogText == null)
            {
                Debug.LogError("CombatLogDisplay: UI Text组件 (combatLogText) 未在检视面板中分配！此组件将无法正常工作。");
                this.enabled = false;
            }
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Log || type == LogType.Warning)
            {
                // Keywords for filtering should be localized if they are meant to match localized log messages
                // Example: "僵尸" (Zombie), "攻击" (attacks), "弹药" (ammo), "防御者" (defenders), "生成" (Spawned), "死亡" (died)
                if (logString.Contains("僵尸") || logString.Contains("攻击") ||
                    logString.Contains("弹药") || logString.Contains("防御者") ||
                    logString.Contains("生成") || logString.Contains("死亡"))
                {
                    if (mLogMessages.Count >= maxLogLines)
                    {
                        mLogMessages.Dequeue();
                    }
                    // Timestamp format "HH:mm:ss" is generally universal.
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

        // Awake and OnDestroy are removed as their logic is moved to OnEnable/OnDisable or not needed.
        // No Update() method was present or needed.
    }
}
