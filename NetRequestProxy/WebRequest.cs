#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：RequestProxy
* 项目描述 ：
* 类 名 称 ：WebRequest
* 类 描 述 ：
* 命名空间 ：RequestProxy
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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RequestProxy
{
    /* ============================================================================== 
* 功能描述：WebRequest web服务
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
   public class WebRequest
    {
        public async Task<string> PostAsync(string url,string  content)
        {
            HttpClient client = new HttpClient();
            StringContent theContent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            using (HttpResponseMessage message = await client.PostAsync(url, theContent))
            {
                message.EnsureSuccessStatusCode();
                return await message.Content.ReadAsStringAsync();
            }
        }

        public async  Task<string> GetAsync(string url)
        {
            HttpClient client = new HttpClient();
            using (HttpResponseMessage message = await client.GetAsync(url))
            {
                message.EnsureSuccessStatusCode();
                return await message.Content.ReadAsStringAsync();
            }
        }
    }
}
