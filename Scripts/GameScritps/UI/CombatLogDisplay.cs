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

        private void Awake()
        {
            combatLogText = combatLogText ?? transform.Find("CombatLogText")?.GetComponent<Text>();
            // The specific object name "CombatLogText" inside a ScrollView is usually "Text" under "Content"
            // If the direct child of CombatLogDisplay_Panel is the ScrollView, then the path might be "LogScrollView/Viewport/Content/CombatLogText"
            // For a simpler setup if CombatLogText is a direct child Text element of CombatLogDisplay_Panel:
            // combatLogText = combatLogText ?? transform.Find("CombatLogText_Element")?.GetComponent<Text>();
            // For now, assuming "CombatLogText" is the name of the GameObject holding the Text component directly under this panel,
            // or it's correctly linked in Inspector. A more robust Find would require knowing the exact hierarchy if not linked.

            if (combatLogText == null)
            {
                // Attempting a more specific find assuming the structure from ULTRA_DETAILED_SETUP_GUIDE_ZH.md
                // LogScrollView -> Viewport -> Content -> CombatLogText
                Transform textTransform = transform.Find("LogScrollView/Viewport/Content/CombatLogText");
                if (textTransform != null)
                {
                    combatLogText = textTransform.GetComponent<Text>();
                }

                if (combatLogText == null)
                {
                    Debug.LogError("CombatLogDisplay: UI Text组件 (combatLogText) 未能成功获取或链接。请检查Hierarchy中的命名、路径和组件，或在Inspector中手动链接。");
                    // this.enabled = false; // Disabling here might be too early if linking happens in Start of other scripts.
                }
            }
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            // Null check for combatLogText is now primarily in Awake.
            // If still null here, it means Awake failed or it was unassigned after Awake.
            if (combatLogText == null)
            {
                 Debug.LogError("CombatLogDisplay (OnEnable): combatLogText is still null. Ensure it's linked and an error didn't occur in Awake.");
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
