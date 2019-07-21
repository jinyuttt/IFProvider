#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：RequestProxy
* 项目描述 ：
* 类 名 称 ：TestCls
* 类 描 述 ：
* 所在的域 ：DESKTOP-QFAK48O
* 命名空间 ：RequestProxy
* 机器名称 ：DESKTOP-QFAK48O 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2019
* 更新时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion


using System;
using System.Collections.Generic;
using System.Text;

namespace RequestProxy
{

    /* ============================================================================== 
* 功能描述：TestCls
* 创 建 者：jinyu
* 创建日期：2019
* 更新时间 ：2019
* ==============================================================================*/
   public class TestCls
    {
        public void Test()
        {
            RequestBody request = new RequestBody();
            request.SrvName = "Test";
            request.ExecuteFun = "Fun";
        }
    }
}
