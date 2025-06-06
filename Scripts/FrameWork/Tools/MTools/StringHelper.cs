using System;
using UnityEngine;

namespace MyTools
{
    public static class StringHelper
    {
        // 将整数转换为大写英文单词表示的函数
       public static string ConvertToUpperCase(int number)
        {
            // 用于存储单位、十位和百位的英文单词数组
            string[] units = { "", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] teens = { "十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九" };
            string[] tens = { "", "", "二十", "三十", "四十", "五十", "六十", "七十", "八十", "九十" };

            // 检查特殊情况：零和负数
            if (number == 0)
            {
                return "零";
            }

            if (number < 0)
            {
                return "负" + ConvertToUpperCase(-number);
            }

            // 用于存储最终英文单词表示的变量
            string words = "";

            // 处理千位
            if ((number / 1000) > 0)
            {
                words += ConvertToUpperCase(number / 1000) + " 千";
                number %= 1000;
            }

            // 处理百位
            if ((number / 100) > 0)
            {
                words += ConvertToUpperCase(number / 100) + " 百";
                number %= 100;
            }

            // 处理十位和个位
            if (number > 0)
            {
                // 如果已经有单词且还有更多数字要处理，则添加“和”
                if (words != "")
                {
                    words += "零";
                }

                // 十位和个位分开处理
                if (number < 10)
                {
                    words += units[number];
                }
                else if (number < 20)
                {
                    words += teens[number - 10];
                }
                else
                {
                    words += tens[number / 10];
                    if ((number % 10) > 0)
                    {
                        words += units[number % 10];
                    }
                }
            }

            // 返回最终的英文单词表示
            return words;
        }
    }
}