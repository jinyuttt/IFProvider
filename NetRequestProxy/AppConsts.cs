using System;
using System.Collections.Generic;

namespace RequestProxy
{
    internal class AppConsts
    {
        /// <summary>
        /// 默认谓词：Post
        /// </summary>
        public static string DefaultHttpVerb { get; set; }

     

        /// <summary>
        /// 默认前缀：APi
        /// </summary>
        public static string DefaultApiPreFix { get; set; }

        /// <summary>
        /// 控制器前缀
        /// </summary>
        public static List<string> ControllerPostfixes { get; set; }

       

       
        /// <summary>
        /// 方法后缀
        /// </summary>
        public static List<string> ActionPostfixes { get; set; }

        /// <summary>
        /// 绑定 特性FormBody的类型
        /// </summary>
        public static List<Type> FormBodyBindingIgnoredTypes { get; set; }

        /// <summary>
        /// 谓词替换集合
        /// </summary>
        public static Dictionary<string, string> HttpVerbs { get; }

        static AppConsts()
        {
            DefaultHttpVerb = "POST";
            DefaultApiPreFix = "api";
            ActionPostfixes =new List<string>() { "Async" };
            HttpVerbs = new Dictionary<string, string>()
            {
                ["Add"] = "POST",
                ["create"] = "POST",
                ["post"] = "POST",

                ["get"] = "GET",
                ["find"] = "GET",
                ["fetch"] = "GET",
                ["query"] = "GET",

                ["update"] = "PUT",
                ["put"] = "PUT",

                ["delete"] = "DELETE",
                ["remove"] = "DELETE",
            };
        }
    }
}