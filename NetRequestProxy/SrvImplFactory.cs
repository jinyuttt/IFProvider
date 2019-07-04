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



using System;
using System.Collections.Generic;
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
        static Lazy<SrvImplFactory> SrvImpl = new Lazy<SrvImplFactory>();
        public Dictionary<string, string> dic = new Dictionary<string, string>();

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
            WebRequest client = new WebRequest();
            string url = dic["url"] + "/" + request.SrvName;
            var  result = client.PostAsync(url, "");
            return default(T);
        }

        /// <summary>
        /// 无返回的请求
        /// </summary>
        /// <param name="request"></param>
        public void Push(RequestBody request)
        {
            WebRequest client = new WebRequest();
            string url = dic["url"] + "/" + request.SrvName;
            var result = client.GetAsync(url);
           
        }
    }
}
