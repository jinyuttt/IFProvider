#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：INetClient
* 项目描述 ：
* 类 名 称 ：SrvImplFactory
* 类 描 述 ：
* 命名空间 ：INetClient
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion



using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RequestProxy
{
    /* ============================================================================== 
* 功能描述：SrvImplFactory 服务请求代理
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public  class SrvImplFactory
    {

        /// <summary>
        /// 单例
        /// </summary>
        static readonly Lazy<SrvImplFactory> SrvImpl = new Lazy<SrvImplFactory>();

        public Dictionary<string, string> dic = new Dictionary<string, string>();
        string version = "";
        string area = "";
       const string controllerPre = "Bil";

        /// <summary>
        /// 单例属性
        /// </summary>
        public static SrvImplFactory Instance
        {
            get { return SrvImpl.Value; }
        }

        /// <summary>
        /// 有返回值的请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public T Request<T>(RequestBody request)
        {
          
            var template = GetControllerUrl(request);
            string url = CreateActionRouteModel(version, area, template.ControllerName, template.ActionName);
            if(!string.IsNullOrEmpty(template.UrlPara))
            {
                url = url + "?" + template.UrlPara;
            }
                var result = Task.Factory.StartNew(async () =>
                {
                    HttpRequest client = new HttpRequest();
                    if (template.Verb == "get")
                    {
                        var r = await client.GetAsync(url);
                        return r;
                    }
                    else if(template.Verb=="delete")
                    {
                        var r = await client.DeleteAsync(url);
                        return r;
                    }
                    else if (template.Verb == "put")
                    {
                        //只考虑一个
                        string p = "";
                        foreach(var kv in template.BodyPara)
                        {
                            p = kv.Value;
                        }
                        var r = await client.PutAsync(url,p);
                        return r;
                    }
                    else
                    {
                        string p = "";
                        foreach (var kv in template.BodyPara)
                        {
                            p = kv.Value;
                        }
                        var r = await client.PostAsync(url, p);
                        return r;
                    }
                });
            
               return JsonConvert.DeserializeObject<T>(result.Result.Result);
           
        }

        /// <summary>
        /// 无返回的请求
        /// </summary>
        /// <param name="request"></param>
        public void Push(RequestBody request)
        {
         
          
            var template = GetControllerUrl(request);
            string url = CreateActionRouteModel(version, area, template.ControllerName, template.ActionName);
            if (!string.IsNullOrEmpty(template.UrlPara))
            {
                url = url + "?" + template.UrlPara;
            }
            var result = Task.Factory.StartNew(async () =>
            {
                HttpRequest client = new HttpRequest();
                if (template.Verb == "get")
                {
                     await client.GetAsync(url);
                   
                }
                else if (template.Verb == "delete")
                {
                     await client.DeleteAsync(url);
                   
                }
                else if (template.Verb == "put")
                {
                    //只考虑一个
                    string p = "";
                    foreach (var kv in template.BodyPara)
                    {
                        p = kv.Value;
                    }
                    await client.PutAsync(url, p);
                   
                }
                else
                {
                    string p = "";
                    foreach (var kv in template.BodyPara)
                    {
                        p = kv.Value;
                    }
                    var r = await client.PostAsync(url, p);
                }
            });

        }

        /// <summary>
        /// 处理API信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static ControllerTemplate GetControllerUrl(RequestBody request)
        {
            
            string srvName = request.SrvName;
            if(srvName.StartsWith("I"))
            {
                srvName = srvName.Substring(1);
            }
            srvName += controllerPre;
          
            var verbKey = request.ExecuteFun.GetPascalOrCamelCaseFirstWord().ToLower();

            string verb;
            //查看是否有谓词对应
            verb = AppConsts.HttpVerbs.ContainsKey(verbKey) ? AppConsts.HttpVerbs[verbKey] : AppConsts.DefaultHttpVerb;

            string action = request.ExecuteFun;
            action = GetRestFulActionName(action);
            string para = "";
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            foreach(var p in request.Param)
            {
                if (IsPrimitiveExtendedIncludingNullable(p.Value.GetType()))
                {
                    para = string.Format("{0}={1}&", p.Key, p.Value);
                }
                else
                {
                    dicPara[p.Key] = JsonConvert.SerializeObject(p.Value);
                }
            }

            if(!string.IsNullOrEmpty(para))
            {
                para = para.Substring(0, para.Length - 1);
                
            }
            //
            ControllerTemplate template = new ControllerTemplate() { ActionName = action,
                BodyPara = dicPara, ControllerName = srvName, UrlPara = para, Verb = verb };
            return template;

        }

       
        /// <summary>
        /// 判断数据类型是否是基础值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeEnums"></param>
        /// <returns></returns>
        public static bool IsPrimitiveExtendedIncludingNullable(Type type, bool includeEnums = false)
        {
            if (IsPrimitiveExtended(type, includeEnums))
            {
                return true;
            }

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsPrimitiveExtended(type.GenericTypeArguments[0], includeEnums);
            }

            return false;
        }

        private static bool IsPrimitiveExtended(Type type, bool includeEnums)
        {
            if (type.GetTypeInfo().IsPrimitive)
            {
                return true;
            }

            if (includeEnums && type.GetTypeInfo().IsEnum)
            {
                return true;
            }

            return type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }
      

        /// <summary>
        /// 设置路由（控制器+Action）
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="areaName">域</param>
        /// <param name="controllerName">控制器名称</param>
        /// <param name="actionName">方法</param>
        /// <returns></returns>
        private static string CreateActionRouteModel(string version, string areaName, string controllerName, string actionName)
        {
            var routeStr =
                $"{AppConsts.DefaultApiPreFix}/{version}/{areaName}/{controllerName}/{actionName}";
            routeStr = routeStr.RemoveRepeat("/");
            return routeStr;
        }


        /// <summary>
        /// 处理Action的名称
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        private static string GetRestFulActionName(string actionName)
        {
            // Remove Postfix  Async
            actionName = actionName.RemovePostFix(AppConsts.ActionPostfixes.ToArray());

            // Remove Prefix 移除 
            var verbKey = actionName.GetPascalOrCamelCaseFirstWord().ToLower();
            if (AppConsts.HttpVerbs.ContainsKey(verbKey))
            {
                //如果包含谓词则替换
                if (actionName.Length == verbKey.Length)
                {
                    return "";
                }
                else
                {
                    //移除谓词
                    return actionName.Substring(verbKey.Length);
                }
            }
            else
            {
                //全部返回
                return actionName;
            }
        }

    }

  internal class ControllerTemplate
    {
        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string UrlPara { get; set; }

        public Dictionary<string,string> BodyPara { get; set; }

        public string Verb { get; set; }
    }
}
