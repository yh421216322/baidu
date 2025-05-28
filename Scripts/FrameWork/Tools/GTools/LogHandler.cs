using UnityEngine;
using MyTools;
namespace MyGameNamespace
{
    
    /// <summary>
    /// Log日志导出
    /// </summary>
public class LogHandler : MonoBehaviour
{
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 根据日志类型处理日志信息
        switch (type)
        {
            //报错类和异常类输出
            case LogType.Error:
            case LogType.Exception:
                
                FileHelper.WriteLogMessage(logString+"定位："+stackTrace,Consts.LogPath+"ErrorOrExceptionlog.txt");
                // 处理错误和异常
                //Debug.Log("Error or Exception: " +stackTrace);
                break;
            //警告类输出
            case LogType.Warning:
               // MyTools.FileHelper.WriteLogMessage(logString,Consts.LogPath+"log.txt");
                // 处理警告
                //Debug.Log("Warning: " + logString);
                break;
            //日志输出
            default:
               // MyTools.FileHelper.WriteLogMessage(logString,Consts.LogPath+"log.txt");
                // 处理普通日志
                //Debug.Log("Log: " + logString);
                break;
        }
        // 可选：将日志信息发送到服务器或保存到文件等
    }
}}