using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGameNamespace
{


    public static class Consts
    {
        //目录

        public static readonly string LogPath = Application.dataPath;
    }

    //屏幕方向
    public enum ScreenOrientation
    {
        Horizontal,//横屏
        Vertical,//竖屏
    }
}
