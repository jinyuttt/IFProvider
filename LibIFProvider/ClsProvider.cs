#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：InterfaceProvider
* 项目描述 ：
* 类 名 称 ：ClsProvider
* 类 描 述 ：
* 命名空间 ：InterfaceProvider
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Concurrent;
using System.Threading;
using RequestProxy;

namespace LibInterfaceProvider
{
    /* ============================================================================== 
* 功能描述：ClsProvider 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public  class ClsProvider
    {
        const string Name = "ProxyBilCls";
        const char ClsFlage = '`';
        static ConcurrentDictionary<string, Type> dicType = new ConcurrentDictionary<string, Type>();
        static ModuleBuilder moduleBuilder = null;
        static TypeBuilder typeBuilder = null;
        public static T Create<T>() where T:class
        {
            Type curType = typeof(T);
            Type type = null;
            if(!dicType.TryGetValue(curType.Name,out type))
            {
                CompileALLMethod<T>();
                dicType.TryGetValue(curType.Name, out type);
            }
            if (type != null)
            {
                if (curType.IsGenericType)
                {
                    var cur = type.MakeGenericType(curType.GetGenericArguments());
                    return (T)Activator.CreateInstance(cur);
                }
                else
                {
                    return (T)Activator.CreateInstance(type);
                }
            }
            return null;
           
        }
        /// <summary>
        /// 编译方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static void CompileALLMethod<T>()
        {
            Type curType = typeof(T);
            //
           
            AssemblyName assemblyName = new AssemblyName(Name);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(Name);
            //
            var lst = Filter<T>();
            #region 实现方法
            if (curType.IsGenericType)
            {
                //加入参数即可
                var pName = curType.GetGenericTypeDefinition().GetGenericArguments().Select(X => X.Name).ToArray();
                //重新定义类
                string clsName = curType.Name.Split(new char[] { ClsFlage })[0];
                foreach (string f in pName)
                {
                    clsName = clsName + "_" + f;
                }
                //
                typeBuilder = moduleBuilder.DefineType(clsName + "Cls", TypeAttributes.Public | TypeAttributes.Class,
         typeof(object), new Type[] { curType.GetGenericTypeDefinition() });
                typeBuilder.DefineGenericParameters(pName);
            }
            else
            {
                typeBuilder = moduleBuilder.DefineType(curType.Name + "Cls", TypeAttributes.Public | TypeAttributes.Class,
     typeof(object), new Type[] { curType });
            }

            if (curType.IsGenericType)
            {
                foreach (var method in curType.GetGenericTypeDefinition().GetMethods())
                {
                    if (lst.Contains(method.Name))
                    {
                        continue;//排除属性的方法
                    }
                    if (method.IsGenericMethod)
                    {
                        CompileGenericMethod(typeBuilder, method);

                    }
                    else
                    {
                        CompileMethod(typeBuilder, method);
                    }
                }
            }
            else
            {
                foreach (var method in curType.GetMethods())
                {
                    if (lst.Contains(method.Name))
                    {
                        continue;//排除属性的方法
                    }
                    if (method.IsGenericMethod)
                    {
                        CompileGenericMethod(typeBuilder, method);

                    }
                    else
                    {
                        CompileMethod(typeBuilder, method);
                    }
                }
            }
            //
            #endregion

            CompileProperty<T>(typeBuilder);
            CompileEvent<T>(typeBuilder);
            dicType[curType.Name] = typeBuilder.CreateType();

        }

        /// <summary>
        /// 编译一般方法
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="method"></param>
        private static void CompileMethod(TypeBuilder typeBuilder, MethodInfo method)
        {
            bool isReturn = false;
            var param = method.GetParameters();
            var pType = method.GetParameters().Select(X => X.ParameterType).ToArray();
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;
            MethodBuilder mtd = typeBuilder.DefineMethod(method.Name, methodAttributes, method.CallingConvention, method.ReturnType, pType);
            for (int i = 0; i < param.Length; i++)
            {
                mtd.DefineParameter(i + 1, param[i].Attributes, param[i].Name);

            }
            //

            if (method.ReturnType != typeof(void))
            {
                isReturn = true;
            }
            var il = mtd.GetILGenerator();
            il.DeclareLocal(typeof(RequestBody));
            if (isReturn)
            {
                il.DeclareLocal(method.ReturnType);
            }
            //
            il.Emit(OpCodes.Newobj, typeof(RequestBody).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldstr, method.Name);
            il.Emit(OpCodes.Call, typeof(RequestBody).GetProperty("SrvName").GetSetMethod());

            for (short i = 0; i < param.Length; i++)
            {
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Call, typeof(RequestBody).GetProperty("Param").GetGetMethod());
                il.Emit(OpCodes.Ldstr, param[i].Name);
                il.Emit(OpCodes.Ldarg, i + 1);
                if (param[i].ParameterType.IsValueType)
                {
                    il.Emit(OpCodes.Box, param[i].ParameterType);
                }
                il.Emit(OpCodes.Call, typeof(Dictionary<string, object>).GetMethod("set_Item"));

            }
            //
            il.Emit(OpCodes.Call, typeof(SrvImplFactory).GetProperty("Instance").GetGetMethod());
            il.Emit(OpCodes.Ldloc_0);
            if (isReturn)
            {
                var lblret = il.DefineLabel();
                var mtdinfo = typeof(SrvImplFactory).GetMethod("Request");
                mtdinfo = mtdinfo.MakeGenericMethod(method.ReturnType);//构建对应泛型方法调用
                il.Emit(OpCodes.Call, mtdinfo);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Br_S, lblret);
                il.MarkLabel(lblret);
                il.Emit(OpCodes.Ldloc_1);
            }
            else
            {
                var mtdinfo = typeof(SrvImplFactory).GetMethod("Push");
                il.Emit(OpCodes.Call, mtdinfo);
            }
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// 编译泛型方法
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="method"></param>
        private static void CompileGenericMethod(TypeBuilder typeBuilder, MethodInfo method)
        {
            bool isReturn = false;
            var param = method.GetParameters();
            var pType = method.GetParameters().Select(X => X.ParameterType).ToArray();
            var genericParam = method.GetGenericArguments().Select(X => X.Name).ToArray();
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;
            MethodBuilder mtd = typeBuilder.DefineMethod(method.Name, methodAttributes, method.CallingConvention, method.ReturnType, pType);
            mtd.DefineGenericParameters(genericParam);//定义生成泛型参数

            for (int i = 0; i < param.Length; i++)
            {
                mtd.DefineParameter(i + 1, param[i].Attributes, param[i].Name);
            }
            //

            if (method.ReturnType != typeof(void))
            {
                isReturn = true;
            }
            var il = mtd.GetILGenerator();
            //开始方法体
            il.DeclareLocal(typeof(RequestBody));// RequestBody req;
            if (isReturn)
            {
                il.DeclareLocal(method.ReturnType);
            }
            //
            il.Emit(OpCodes.Newobj, typeof(RequestBody).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);//req=new RequestBody();
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldstr, method.Name);
            il.Emit(OpCodes.Call, typeof(RequestBody).GetProperty("SrvName").GetSetMethod());
            //req.SrvName=XXX;
            for (short i = 0; i < param.Length; i++)
            {
                //req.Param["AA"]=AA;
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Call, typeof(RequestBody).GetProperty("Param").GetGetMethod());
                il.Emit(OpCodes.Ldstr, param[i].Name);
                il.Emit(OpCodes.Ldarg, i + 1);
                if (param[i].ParameterType.IsValueType)
                {
                    il.Emit(OpCodes.Box, param[i].ParameterType);
                }
                il.Emit(OpCodes.Call, typeof(Dictionary<string, object>).GetMethod("set_Item"));
                //

            }
            //
            il.Emit(OpCodes.Call, typeof(SrvImplFactory).GetProperty("Instance").GetGetMethod());
            il.Emit(OpCodes.Ldloc_0);
            if (isReturn)
            {
                //SrvImplFactory.Instance.Request
                var lblret = il.DefineLabel();
                var mtdinfo = typeof(SrvImplFactory).GetMethod("Request");
                mtdinfo = mtdinfo.MakeGenericMethod(method.ReturnType);//构建对应泛型方法调用
                il.Emit(OpCodes.Call, mtdinfo);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Br_S, lblret);
                il.MarkLabel(lblret);
                il.Emit(OpCodes.Ldloc_1);
                //
            }
            else
            {
                //SrvImplFactory.Instance.Push
                var mtdinfo = typeof(SrvImplFactory).GetMethod("Push");
                il.Emit(OpCodes.Call, mtdinfo);
            }
            il.Emit(OpCodes.Ret);
            //方法体结束
        }


        /// <summary>
        /// 移除筛选方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static List<string> Filter<T>()
        {
            List<string> lst = new List<string>();
            Type cur = typeof(T);
            foreach (var e in cur.GetEvents())
            {
                lst.Add("add_" + e.Name);
                lst.Add("remove_" + e.Name);
            }
            foreach (var p in cur.GetProperties())
            {
                lst.Add("get_" + p.Name);
                lst.Add("set_" + p.Name);
            }
            return lst;
        }

        /// <summary>
        /// 构建属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeBuilder"></param>
        private static void CompileProperty<T>(TypeBuilder typeBuilder)
        {
            Type curType = typeof(T);

            foreach (var p in curType.GetProperties())
            {
                FieldBuilder fb = typeBuilder.DefineField(
                 "m_" + p.Name.ToLower(), p.PropertyType, FieldAttributes.Private);
                PropertyBuilder pbNumber = typeBuilder.DefineProperty(p.Name, PropertyAttributes.HasDefault, p.PropertyType, null);
                MethodAttributes getSetAttr = MethodAttributes.Public |
          MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final;
                MethodBuilder mbGetAccessor = typeBuilder.DefineMethod(
                    "get_" + p.Name,
                    getSetAttr,
                      p.PropertyType,
                    Type.EmptyTypes);

                ILGenerator numberGetIL = mbGetAccessor.GetILGenerator();

                numberGetIL.Emit(OpCodes.Ldarg_0);
                numberGetIL.Emit(OpCodes.Ldfld, fb);
                numberGetIL.Emit(OpCodes.Ret);


                MethodBuilder mbSetAccessor = typeBuilder.DefineMethod(
                    "set_" + p.Name,
                    getSetAttr,
                    null,
                    new Type[] { p.PropertyType });

                ILGenerator numberSetIL = mbSetAccessor.GetILGenerator();
                numberSetIL.Emit(OpCodes.Ldarg_0);
                numberSetIL.Emit(OpCodes.Ldarg_1);
                numberSetIL.Emit(OpCodes.Stfld, fb);
                numberSetIL.Emit(OpCodes.Ret);

                pbNumber.SetGetMethod(mbGetAccessor);
                pbNumber.SetSetMethod(mbSetAccessor);
            }

        }

        /// <summary>
        /// 编译事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeBuilder"></param>
        private static void CompileEvent<T>(TypeBuilder typeBuilder)
        {
            Type type = typeof(T);
            foreach (var e in type.GetEvents())
            {
                var ev = typeBuilder.DefineEvent(e.Name, e.Attributes, e.EventHandlerType);
                FieldBuilder fb = typeBuilder.DefineField(e.Name, e.EventHandlerType, FieldAttributes.Private);
                MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName
                    | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;

                MethodBuilder methodAdd = typeBuilder.DefineMethod("add_" + e.Name, attributes, null, new Type[] { e.EventHandlerType });
                var il = methodAdd.GetILGenerator();
                var lbl = il.DefineLabel();
                il.DeclareLocal(e.EventHandlerType);
                il.DeclareLocal(e.EventHandlerType);
                il.DeclareLocal(e.EventHandlerType);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fb);
                il.Emit(OpCodes.Stloc_0);

                il.MarkLabel(lbl);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, typeof(Delegate).GetMethod("Combine", new Type[] { typeof(Delegate), typeof(Delegate) }));
                il.Emit(OpCodes.Castclass, e.EventHandlerType);
                il.Emit(OpCodes.Stloc_2);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldflda, fb);

                il.Emit(OpCodes.Ldloc_2);
                il.Emit(OpCodes.Ldloc_1);
                var mtdinfo = typeof(Interlocked).GetMethods().First(X => X.Name == "CompareExchange" && X.IsGenericMethod);

                il.Emit(OpCodes.Call, mtdinfo.MakeGenericMethod(e.EventHandlerType));
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Bne_Un_S, lbl);
                il.Emit(OpCodes.Ret);

                //
                MethodBuilder methodRemove = typeBuilder.DefineMethod("remove_" + e.Name, attributes, null, new Type[] { e.EventHandlerType });
                il = methodRemove.GetILGenerator();
                var lblrev = il.DefineLabel();
                il.DeclareLocal(e.EventHandlerType);
                il.DeclareLocal(e.EventHandlerType);
                il.DeclareLocal(e.EventHandlerType);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fb);
                il.Emit(OpCodes.Stloc_0);

                il.MarkLabel(lblrev);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, typeof(Delegate).GetMethod("Remove", new Type[] { typeof(Delegate), typeof(Delegate) }));
                il.Emit(OpCodes.Castclass, e.EventHandlerType);
                il.Emit(OpCodes.Stloc_2);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldflda, fb.FieldType);
                il.Emit(OpCodes.Ldloc_2);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Call, mtdinfo.MakeGenericMethod(e.EventHandlerType));
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Bne_Un_S, lblrev);
                il.Emit(OpCodes.Ret);
                ev.SetAddOnMethod(methodAdd);
                ev.SetRemoveOnMethod(methodRemove);

            }
        }

    }
}
