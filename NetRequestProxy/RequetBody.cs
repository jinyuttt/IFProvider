#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：INetClient
* 项目描述 ：
* 类 名 称 ：RequetBody
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



using System.Collections.Generic;

namespace RequestProxy
{
    /* ============================================================================== 
* 功能描述：RequetBody 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public  class RequestBody
    {
        public string SrvName { get; set; }

        public string SrvDLL { get; set; }

        public Dictionary<string,object> Param { get; set; }

        public RequestBody()
        {
            Param = new Dictionary<string, object>();
        }
    }
}
