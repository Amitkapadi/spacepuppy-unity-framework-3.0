﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Dynamic
{

    public interface IDynamic
    {
        object this[string sMemberName] { get; set; }

        bool SetValue(string sMemberName, object value, params object[] index);
        object GetValue(string sMemberName, params object[] args);
        bool TryGetValue(string sMemberName, out object result, params object[] args);
        object InvokeMethod(string sMemberName, params object[] args);

        bool HasMember(string sMemberName, bool includeNonPublic);
        IEnumerable<string> GetMemberNames(bool includeNonPublic);
        IEnumerable<MemberInfo> GetMembers(bool includeNonPublic);
        MemberInfo GetMember(string sMemberName, bool includeNonPublic);
        
    }

    [System.Flags()]
    public enum DynamicMemberAccess
    {
        Inaccessible = 0,
        Read = 1,
        Write = 2,
        ReadWrite = 3
    }

    public static class DynamicUtil
    {
        
        #region IDynamic Methods

        public static bool SetValue(this object obj, string sprop, object value)
        {
            if (obj == null) return false;

            if (obj is IDynamic)
            {
                try
                {
                    return (obj as IDynamic).SetValue(sprop, value, (object[])null);
                }
                catch
                {

                }
            }
            else
            {
                return SetValueDirect(obj, sprop, value, (object[])null);
            }

            return false;
        }

        public static bool SetValue(this object obj, string sprop, object value, params object[] index)
        {
            if (obj == null) return false;

            if (obj is IDynamic)
            {
                try
                {
                    return (obj as IDynamic).SetValue(sprop, value, index);
                }
                catch
                {

                }
            }
            else
            {
                return SetValueDirect(obj, sprop, value, index);
            }

            return false;
        }

        public static bool SetValue(this object obj, MemberInfo member, object value)
        {
            if (obj == null) return false;

            return SetValueDirect(obj, member, value, (object[])null);
        }

        public static bool SetValue(this object obj, MemberInfo member, object value, params object[] index)
        {
            if (obj == null) return false;

            return SetValueDirect(obj, member, value, index);
        }

        public static object GetValue(this object obj, string sprop, params object[] args)
        {
            if (obj == null) return null;

            if (obj is IDynamic)
            {
                try
                {
                    return (obj as IDynamic).GetValue(sprop, args);
                }
                catch
                {

                }
            }
            else
            {
                return GetValueDirect(obj, sprop, args);
            }
            return null;
        }

        public static object GetValue(this object obj, MemberInfo member, params object[] args)
        {
            if (obj == null) return null;

            if (obj is IDynamic)
            {
                try
                {
                    return (obj as IDynamic).GetValue(member.Name, args);
                }
                catch
                {

                }
            }
            else
            {
                return GetValueDirect(obj, member, args);
            }
            return null;
        }

        public static bool TryGetValue(this object obj, string sMemberName, out object result, params object[] args)
        {
            if(obj == null)
            {
                result = null;
                return false;
            }

            if(obj is IDynamic)
            {
                try
                {
                    return (obj as IDynamic).TryGetValue(sMemberName, out result, args);
                }
                catch
                {
                    result = null;
                    return false;
                }
            }
            else
            {
                return TryGetValueDirect(obj, sMemberName, out result, args);
            }
        }

        public static object InvokeMethod(this object obj, string name, params object[] args)
        {
            if (obj == null) return false;

            if(obj is IDynamic)
            {
                try
                {
                    return (obj as IDynamic).InvokeMethod(name, args);
                }
                catch
                {

                }
            }
            else
            {
                return InvokeMethodDirect(obj, name, args);
            }

            return null;
        }

        public static bool HasMember(object obj, string name, bool includeNonPublic)
        {
            if (obj == null) return false;

            if (obj is IDynamic)
            {
                return (obj as IDynamic).HasMember(name, includeNonPublic);
            }
            else
            {
                return TypeHasMember(obj.GetType(), name, includeNonPublic);
            }
        }

        public static IEnumerable<MemberInfo> GetMembers(object obj, bool includeNonPublic)
        {
            if (obj == null) return Enumerable.Empty<MemberInfo>();

            if (obj is IDynamic)
            {
                return (obj as IDynamic).GetMembers(includeNonPublic);
            }
            else
            {
                return GetMembersFromType(obj.GetType(), includeNonPublic);
            }
        }

        public static IEnumerable<MemberInfo> GetMembers(object obj, bool includeNonPublic, MemberTypes mask)
        {
            if (obj == null) return Enumerable.Empty<MemberInfo>();

            if (obj is IDynamic)
            {
                return FilterMembers((obj as IDynamic).GetMembers(includeNonPublic), mask);
            }
            else
            {
                return GetMembersFromType(obj.GetType(), includeNonPublic, mask);
            }
        }

        public static IEnumerable<string> GetMemberNames(object obj, bool includeNonPublic)
        {
            if (obj == null) return Enumerable.Empty<string>();

            if (obj is IDynamic)
            {
                return (obj as IDynamic).GetMemberNames(includeNonPublic);
            }
            else
            {
                return GetMemberNamesFromType(obj.GetType(), includeNonPublic);
            }
        }

        public static IEnumerable<string> GetMemberNames(object obj, bool includeNonPublic, MemberTypes mask)
        {
            if (obj == null) return Enumerable.Empty<string>();

            if (obj is IDynamic)
            {
                return (from m in (obj as IDynamic).GetMembers(includeNonPublic) where (m.MemberType & mask) != 0 select m.Name);
            }
            else
            {
                return GetMemberNamesFromType(obj.GetType(), includeNonPublic, mask);
            }
        }

        public static MemberInfo GetMember(object obj, string sMemberName, bool includeNonPublic)
        {
            if(obj == null) return null;

            if(obj is IDynamic)
            {
                return (obj as IDynamic).GetMember(sMemberName, includeNonPublic);
            }
            else
            {
                return GetMemberFromType(obj.GetType(), sMemberName, includeNonPublic);
            }
        }
        
        #endregion

        #region Direct Reflection

        public static bool SetValueDirect(object obj, string sprop, object value)
        {
            return SetValueDirect(obj, sprop, value, (object[])null);
        }

        public static bool SetValueDirect(object obj, string sprop, object value, params object[] index)
        {
            if (string.IsNullOrEmpty(sprop)) return false;

            //if (sprop.Contains('.'))
            //    obj = DynamicUtil.ReduceSubObject(obj, sprop, out sprop);
            if (obj == null) return false;

            try
            {
                var vtp = (value != null) ? value.GetType() : null;
                var member = GetValueSetterMemberFromType(obj.GetType(), sprop, vtp, true);
                if (member != null)
                {
                    switch (member.MemberType)
                    {
                        case MemberTypes.Field:
                            (member as FieldInfo).SetValue(obj, value);
                            return true;
                        case MemberTypes.Property:
                            (member as PropertyInfo).SetValue(obj, value, index);
                            return true;
                        case MemberTypes.Method:
                            var arr = ArrayUtil.Temp(value);
                            (member as MethodInfo).Invoke(obj, arr);
                            ArrayUtil.ReleaseTemp(arr);
                            return true;
                    }
                }

                if (vtp != null)
                {
                    member = GetValueSetterMemberFromType(obj.GetType(), sprop, null, true);
                    if (member != null)
                    {
                        var rtp = GetReturnType(member);
                        object cobj = null;
                        if (ConvertUtil.TryToPrim(value, rtp, out cobj))
                            value = cobj;

                        switch (member.MemberType)
                        {
                            case MemberTypes.Field:
                                (member as FieldInfo).SetValue(obj, value);
                                return true;
                            case MemberTypes.Property:
                                (member as PropertyInfo).SetValue(obj, value, index);
                                return true;
                            case MemberTypes.Method:
                                var arr = ArrayUtil.Temp(value);
                                (member as MethodInfo).Invoke(obj, arr);
                                ArrayUtil.ReleaseTemp(arr);
                                return true;
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        public static bool SetValueDirect(object obj, MemberInfo member, object value)
        {
            return SetValueDirect(obj, member, value, (object[])null);
        }

        public static bool SetValueDirect(object obj, MemberInfo member, object value, params object[] index)
        {
            if (obj == null) return false;

            if (member == null) return false;

            try
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        (member as FieldInfo).SetValue(obj, value);
                        return true;
                    case MemberTypes.Property:
                        if ((member as PropertyInfo).CanWrite)
                        {
                            (member as PropertyInfo).SetValue(obj, value, index);
                            return true;
                        }
                        else
                            return false;
                    case MemberTypes.Method:
                        var arr = ArrayUtil.Temp(value);
                        (member as MethodInfo).Invoke(obj, arr);
                        ArrayUtil.ReleaseTemp(arr);
                        return true;
                }
            }
            catch
            {

            }

            return false;
        }

        public static object GetValueDirect(object obj, string sprop, params object[] args)
        {
            const BindingFlags BINDING = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (string.IsNullOrEmpty(sprop)) return null;

            //if (sprop.Contains('.'))
            //    obj = DynamicUtil.ReduceSubObject(obj, sprop, out sprop);
            if (obj == null) return null;

            try
            {
                var tp = obj.GetType();

                while (tp != null)
                {
                    var members = tp.GetMember(sprop, BINDING);
                    if (members == null || members.Length == 0) return null;

                    foreach (var member in members)
                    {
                        switch (member.MemberType)
                        {
                            case System.Reflection.MemberTypes.Field:
                                var field = member as System.Reflection.FieldInfo;
                                return field.GetValue(obj);

                            case System.Reflection.MemberTypes.Property:
                                {
                                    var prop = member as System.Reflection.PropertyInfo;
                                    var paramInfos = prop.GetIndexParameters();
                                    if (prop.CanRead && DynamicUtil.ParameterSignatureMatches(args, paramInfos, false))
                                    {
                                        return prop.GetValue(obj, args);
                                    }
                                    break;
                                }
                            case System.Reflection.MemberTypes.Method:
                                {
                                    var meth = member as System.Reflection.MethodInfo;
                                    var paramInfos = meth.GetParameters();
                                    if (DynamicUtil.ParameterSignatureMatches(args, paramInfos, false))
                                    {
                                        return meth.Invoke(obj, args);
                                    }
                                    break;
                                }
                        }
                    }

                    tp = tp.BaseType;
                }
            }
            catch
            {

            }
            return null;
        }

        public static object GetValueDirect(this object obj, MemberInfo member, params object[] args)
        {
            switch (member.MemberType)
            {
                case System.Reflection.MemberTypes.Field:
                    var field = member as System.Reflection.FieldInfo;
                    return field.GetValue(obj);

                case System.Reflection.MemberTypes.Property:
                    {
                        var prop = member as System.Reflection.PropertyInfo;
                        var paramInfos = prop.GetIndexParameters();
                        if (prop.CanRead && DynamicUtil.ParameterSignatureMatches(args, paramInfos, false))
                        {
                            return prop.GetValue(obj, args);
                        }
                        break;
                    }
                case System.Reflection.MemberTypes.Method:
                    {
                        var meth = member as System.Reflection.MethodInfo;
                        var paramInfos = meth.GetParameters();
                        if (DynamicUtil.ParameterSignatureMatches(args, paramInfos, false))
                        {
                            return meth.Invoke(obj, args);
                        }
                        break;
                    }
            }

            return null;
        }

        public static bool TryGetValueDirect(object obj, string sprop, out object result, params object[] args)
        {
            const BindingFlags BINDING = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            result = null;
            if (string.IsNullOrEmpty(sprop)) return false;

            //if (sprop.Contains('.'))
            //    obj = DynamicUtil.ReduceSubObject(obj, sprop, out sprop);
            if (obj == null) return false;

            try
            {
                var tp = obj.GetType();

                while (tp != null)
                {
                    var members = tp.GetMember(sprop, BINDING);
                    if (members == null || members.Length == 0) return false;

                    foreach (var member in members)
                    {
                        switch (member.MemberType)
                        {
                            case System.Reflection.MemberTypes.Field:
                                var field = member as System.Reflection.FieldInfo;
                                result = field.GetValue(obj);
                                return true;

                            case System.Reflection.MemberTypes.Property:
                                {
                                    var prop = member as System.Reflection.PropertyInfo;
                                    var paramInfos = prop.GetIndexParameters();
                                    if (prop.CanRead && DynamicUtil.ParameterSignatureMatches(args, paramInfos, false))
                                    {
                                        result = prop.GetValue(obj, args);
                                        return true;
                                    }
                                    break;
                                }
                            case System.Reflection.MemberTypes.Method:
                                {
                                    var meth = member as System.Reflection.MethodInfo;
                                    var paramInfos = meth.GetParameters();
                                    if (DynamicUtil.ParameterSignatureMatches(args, paramInfos, false))
                                    {
                                        result = meth.Invoke(obj, args);
                                        return true;
                                    }
                                    break;
                                }
                        }
                    }

                    tp = tp.BaseType;
                }
            }
            catch
            {

            }
            return false;
        }

        public static object InvokeMethodDirect(object obj, string name, params object[] args)
        {
            const BindingFlags BINDING = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                         BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding;
            if (string.IsNullOrEmpty(name)) return null;
            //if (name.Contains('.'))
            //    obj = DynamicUtil.ReduceSubObject(obj, name, out name);
            if (obj == null) return false;
            
            var tp = obj.GetType();
            try
            {
                return tp.InvokeMember(name, BINDING, null, obj, args);
            }
            catch
            {
                return null;
            }
        }

        public static bool HasMemberDirect(object obj, string name, bool includeNonPublic)
        {
            if (obj == null) return false;
            if (string.IsNullOrEmpty(name)) return false;
            
            return TypeHasMember(obj.GetType(), name, includeNonPublic);
        }

        public static IEnumerable<MemberInfo> GetMembersDirect(object obj, bool includeNonPublic, MemberTypes mask = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method)
        {
            if (obj == null) return Enumerable.Empty<MemberInfo>();

            return GetMembersFromType(obj.GetType(), includeNonPublic, mask);
        }

        public static IEnumerable<MemberInfo> GetMemberNamesDirect(object obj, bool includeNonPublic, MemberTypes mask = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method)
        {
            if (obj == null) return Enumerable.Empty<MemberInfo>();

            return GetMembersFromType(obj.GetType(), includeNonPublic, mask);
        }





        public static bool TypeHasMember(System.Type tp, string name, bool includeNonPublic)
        {
            const BindingFlags BINDING = BindingFlags.Public | BindingFlags.Instance;
            const BindingFlags PRIV_BINDING = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            //if (name.Contains('.'))
            //    tp = DynamicUtil.ReduceSubType(tp, name, includeNonPublic, out name);
            if (tp == null) return false;

            var member = tp.GetMember(name, BINDING);
            if (member != null && member.Length > 0) return true;

            if(includeNonPublic)
            {
                while (tp != null)
                {
                    member = tp.GetMember(name, PRIV_BINDING);
                    if (member != null && member.Length > 0) return true;
                    tp = tp.BaseType;
                }
            }
            return false;
        }

        public static IEnumerable<MemberInfo> GetMembersFromType(System.Type tp, bool includeNonPublic, MemberTypes mask = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method)
        {
            const BindingFlags BINDING = BindingFlags.Public | BindingFlags.Instance;
            const BindingFlags PRIV_BINDING = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            if (tp == null) yield break;

            foreach (var m in tp.GetMembers(BINDING))
            {
                if ((m.MemberType & mask) != 0)
                {
                    yield return m;


                }
            }

            if (includeNonPublic)
            {
                while (tp != null)
                {
                    foreach (var m in tp.GetMembers(PRIV_BINDING))
                    {
                        if ((m.MemberType & mask) != 0)
                        {
                            yield return m;
                        }
                    }
                    tp = tp.BaseType;
                }
            }
        }

        public static IEnumerable<string> GetMemberNamesFromType(System.Type tp, bool includeNonPublic, MemberTypes mask = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method)
        {
            const BindingFlags BINDING = BindingFlags.Public | BindingFlags.Instance;
            const BindingFlags PRIV_BINDING = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            if (tp == null) yield break;

            foreach (var m in tp.GetMembers(BINDING))
            {
                if ((m.MemberType & mask) != 0)
                {
                    yield return m.Name;
                }
            }

            if (includeNonPublic)
            {
                while (tp != null)
                {
                    foreach (var m in tp.GetMembers(PRIV_BINDING))
                    {
                        if ((m.MemberType & mask) != 0)
                        {
                            yield return m.Name;
                        }
                    }
                    tp = tp.BaseType;
                }
            }
        }



        public static MemberInfo GetMemberFromType(Type tp, string sMemberName, bool includeNonPublic, MemberTypes mask = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method)
        {
            const BindingFlags BINDING_PUBLIC = BindingFlags.Public | BindingFlags.Instance;
            const BindingFlags PRIV_BINDING = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            if (tp == null) throw new ArgumentNullException("tp");

            //if (sMemberName.Contains('.'))
            //{
            //    tp = DynamicUtil.ReduceSubType(tp, sMemberName, includeNonPublic, out sMemberName);
            //    if (tp == null) return null;
            //}

            try
            {
                MemberInfo[] members;

                members = tp.GetMember(sMemberName, BINDING_PUBLIC);
                foreach (var member in members)
                {
                    if ((member.MemberType & mask) != 0) return member;
                }

                while (includeNonPublic && tp != null)
                {
                    members = tp.GetMember(sMemberName, PRIV_BINDING);
                    tp = tp.BaseType;
                    if (members == null || members.Length == 0) continue;

                    foreach (var member in members)
                    {
                        if ((member.MemberType & mask) != 0) return member;
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        public static MemberInfo GetValueMemberFromType(Type tp, string sprop, bool includeNonPublic)
        {
            const BindingFlags BINDING_PUBLIC = BindingFlags.Public | BindingFlags.Instance;
            const BindingFlags PRIV_BINDING = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            if (tp == null) throw new ArgumentNullException("tp");

            //if (sprop.Contains('.'))
            //{
            //    tp = DynamicUtil.ReduceSubType(tp, sprop, includeNonPublic, out sprop);
            //    if (tp == null) return null;
            //}

            try
            {
                MemberInfo[] members;

                members = tp.GetMember(sprop, BINDING_PUBLIC);
                foreach (var member in members)
                {
                    if (IsValidValueMember(member)) return member;
                }

                while (includeNonPublic && tp != null)
                {
                    members = tp.GetMember(sprop, PRIV_BINDING);
                    tp = tp.BaseType;
                    if (members == null || members.Length == 0) continue;

                    foreach (var member in members)
                    {
                        if (IsValidValueMember(member)) return member;
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        public static MemberInfo GetValueSetterMemberFromType(Type tp, string sprop, Type valueType, bool includeNonPublic)
        {
            const BindingFlags BINDING_PUBLIC = BindingFlags.Public | BindingFlags.Instance;
            const BindingFlags PRIV_BINDING = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            if (tp == null) throw new ArgumentNullException("tp");

            //if (sprop.Contains('.'))
            //{
            //    tp = DynamicUtil.ReduceSubType(tp, sprop, includeNonPublic, out sprop);
            //    if (tp == null) return null;
            //}

            try
            {
                System.Type ltp;
                MemberInfo[] members;

                //first strict test
                members = tp.GetMember(sprop, BINDING_PUBLIC);
                foreach (var member in members)
                {
                    if (IsValidValueSetterMember(member, valueType)) return member;
                }

                ltp = tp;
                while (includeNonPublic && ltp != null)
                {
                    members = ltp.GetMember(sprop, PRIV_BINDING);
                    ltp = ltp.BaseType;
                    if (members == null || members.Length == 0) continue;

                    foreach (var member in members)
                    {
                        if (IsValidValueSetterMember(member, valueType)) return member;
                    }
                }
            }
            catch
            {

            }

            return null;
        }



        [System.Obsolete("Poorly named method and return type. Use GetDynamicParameterInfo instead.")]
        public static System.Type[] GetParameters(MemberInfo info)
        {
            switch(info.MemberType)
            {
                case MemberTypes.Field:
                    return new System.Type[] { (info as FieldInfo).FieldType };
                case MemberTypes.Property:
                    return new System.Type[] { (info as PropertyInfo).PropertyType };
                case MemberTypes.Method:
                    {
                        var paramInfos = (info as MethodBase).GetParameters();
                        Type[] arr = new Type[paramInfos.Length];
                        for(int i = 0; i < arr.Length; i++)
                        {
                            arr[i] = paramInfos[i].ParameterType;
                        }
                        return arr;
                    }
                default:
                    return ArrayUtil.Empty<System.Type>();
            }
        }

        public static DynamicParameterInfo[] GetDynamicParameterInfo(MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return new DynamicParameterInfo[] { new DynamicParameterInfo(info, info.Name, (info as FieldInfo).FieldType) };
                case MemberTypes.Property:
                    return new DynamicParameterInfo[] { new DynamicParameterInfo(info, info.Name, (info as PropertyInfo).PropertyType) };
                case MemberTypes.Method:
                    {
                        var paramInfos = (info as MethodBase).GetParameters();
                        DynamicParameterInfo[] arr = new DynamicParameterInfo[paramInfos.Length];
                        for (int i = 0; i < arr.Length; i++)
                        {
                            arr[i] = new DynamicParameterInfo( paramInfos[i]);
                        }
                        return arr;
                    }
                default:
                    return ArrayUtil.Empty<DynamicParameterInfo>();
            }
        }

        public static Type GetReturnType(MemberInfo info)
        {
            if (info == null) return null;

            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return (info as FieldInfo).FieldType;
                case MemberTypes.Property:
                    return (info as PropertyInfo).PropertyType;
                case MemberTypes.Method:
                    return (info as MethodInfo).ReturnType;
            }
            return null;
        }

        /// <summary>
        /// If the member is writeable, returns the the type it expects.
        /// Field - type of the field
        /// Property - type of the property
        /// Method - type of the first parameter, if any, otherwise null.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static Type GetInputType(MemberInfo info)
        {
            if (info == null) return null;

            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return (info as FieldInfo).FieldType;
                case MemberTypes.Property:
                    return (info as PropertyInfo).PropertyType;
                case MemberTypes.Method:
                    {
                        var meth = info as MethodInfo;
                        if (meth == null) return null;
                        var arr = meth.GetParameters();
                        if (arr.Length == 0) return null;
                        return arr[0].ParameterType;
                    }
            }
            return null;
        }

        public static DynamicMemberAccess GetMemberAccessLevel(MemberInfo info)
        {
            if (info == null) return DynamicMemberAccess.Inaccessible;

            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return DynamicMemberAccess.ReadWrite;
                case MemberTypes.Property:
                    var pinfo = info as PropertyInfo;
                    if (pinfo.CanRead)
                        return (pinfo.CanWrite) ? DynamicMemberAccess.ReadWrite : DynamicMemberAccess.Read;
                    else
                        return (pinfo.CanWrite) ? DynamicMemberAccess.Write : DynamicMemberAccess.Inaccessible;
                case MemberTypes.Method:
                    var minfo = info as MethodInfo;
                    if (minfo.ReturnType != typeof(void))
                        return DynamicMemberAccess.ReadWrite;
                    else
                        return DynamicMemberAccess.Write;
                default:
                    return DynamicMemberAccess.Inaccessible;
            }
        }

        public static object GetValueWithMember(MemberInfo info, object targObj)
        {
            if (info == null) return null;

            try
            {
                switch (info.MemberType)
                {
                    case MemberTypes.Field:
                        return (info as FieldInfo).GetValue(targObj);
                    case MemberTypes.Property:
                        return (info as PropertyInfo).GetValue(targObj, null);
                    case MemberTypes.Method:
                        return (info as MethodInfo).Invoke(targObj, null);
                }
            }
            catch
            {

            }
            
            return null;
        }

        public static object GetValueWithMember(MemberInfo info, object targObj, bool ignoreMethod)
        {
            if (info == null) return null;

            try
            {
                switch (info.MemberType)
                {
                    case MemberTypes.Field:
                        return (info as FieldInfo).GetValue(targObj);
                    case MemberTypes.Property:
                        return (info as PropertyInfo).GetValue(targObj, null);
                    case MemberTypes.Method:
                        if (ignoreMethod)
                            return null;
                        else
                            return (info as MethodInfo).Invoke(targObj, null);
                }
            }
            catch
            {

            }

            return null;
        }


        /// <summary>
        /// Returns members that return/accept types that are considered easily serializable. 
        /// Easily serialized types are those that can be referenced by a VariantReference.
        /// </summary>
        /// <param name="obj">Object to find member of</param>
        /// <param name="mask">MemberType mask</param>
        /// <param name="access">Access mask</param>
        /// <returns></returns>
        public static IEnumerable<System.Reflection.MemberInfo> GetEasilySerializedMembers(object obj, MemberTypes mask = MemberTypes.All, DynamicMemberAccess access = DynamicMemberAccess.ReadWrite, bool ignoreObsoleteMembers = true)
        {
            if (obj == null) yield break;

            bool bRead = (access & DynamicMemberAccess.Read) != 0;
            bool bWrite = (access & DynamicMemberAccess.Write) != 0;
            var members = com.spacepuppy.Dynamic.DynamicUtil.GetMembers(obj, false);
            foreach (var mi in members)
            {
                if ((mi.MemberType & mask) == 0) continue;
                if (ignoreObsoleteMembers && mi.IsObsolete()) continue;

                if ((mi.DeclaringType.IsAssignableFrom(typeof(UnityEngine.MonoBehaviour)) ||
                     mi.DeclaringType.IsAssignableFrom(typeof(SPComponent))) && mi.Name != "enabled") continue;

                switch (mi.MemberType)
                {
                    case System.Reflection.MemberTypes.Method:
                        {
                            var m = mi as System.Reflection.MethodInfo;
                            if (m.IsSpecialName) continue;
                            if (m.IsGenericMethod) continue;

                            var parr = m.GetParameters();
                            if (parr.Length == 0)
                            {
                                yield return m;
                            }
                            else
                            {
                                bool pass = true;
                                foreach (var p in parr)
                                {
                                    if (!(VariantReference.AcceptableType(p.ParameterType) || p.ParameterType == typeof(object)))
                                    {
                                        pass = false;
                                        break;
                                    }
                                }
                                if (pass) yield return m;
                            }
                        }
                        break;
                    case System.Reflection.MemberTypes.Field:
                        {
                            var f = mi as System.Reflection.FieldInfo;
                            if (f.IsSpecialName) continue;

                            if (VariantReference.AcceptableType(f.FieldType)) yield return f;
                        }
                        break;
                    case System.Reflection.MemberTypes.Property:
                        {
                            var p = mi as System.Reflection.PropertyInfo;
                            if (p.IsSpecialName) continue;
                            if (!p.CanRead && bRead) continue;
                            if (!p.CanWrite && bWrite) continue;
                            if (p.GetIndexParameters().Length > 0) continue; //indexed properties are not allowed

                            if (VariantReference.AcceptableType(p.PropertyType)) yield return p;
                        }
                        break;
                }

            }
        }

        public static IEnumerable<System.Reflection.MemberInfo> GetEasilySerializedMembersFromType(System.Type tp, MemberTypes mask = MemberTypes.All, DynamicMemberAccess access = DynamicMemberAccess.ReadWrite)
        {
            if (tp == null) yield break;

            bool bRead = (access & DynamicMemberAccess.Read) != 0;
            bool bWrite = (access & DynamicMemberAccess.Write) != 0;
            var members = com.spacepuppy.Dynamic.DynamicUtil.GetMembersFromType(tp, false);
            foreach (var mi in members)
            {
                if ((mi.MemberType & mask) == 0) continue;

                if ((mi.DeclaringType.IsAssignableFrom(typeof(UnityEngine.MonoBehaviour)) ||
                     mi.DeclaringType.IsAssignableFrom(typeof(SPComponent))) && mi.Name != "enabled") continue;

                switch (mi.MemberType)
                {
                    case System.Reflection.MemberTypes.Method:
                        {
                            var m = mi as System.Reflection.MethodInfo;
                            if (m.IsSpecialName) continue;
                            if (m.IsGenericMethod) continue;

                            var parr = m.GetParameters();
                            if (parr.Length == 0)
                            {
                                yield return m;
                            }
                            else
                            {
                                bool pass = true;
                                foreach (var p in parr)
                                {
                                    if (!(VariantReference.AcceptableType(p.ParameterType) || p.ParameterType == typeof(object)))
                                    {
                                        pass = false;
                                        break;
                                    }
                                }
                                if (pass) yield return m;
                            }
                        }
                        break;
                    case System.Reflection.MemberTypes.Field:
                        {
                            var f = mi as System.Reflection.FieldInfo;
                            if (f.IsSpecialName) continue;

                            if (VariantReference.AcceptableType(f.FieldType)) yield return f;
                        }
                        break;
                    case System.Reflection.MemberTypes.Property:
                        {
                            var p = mi as System.Reflection.PropertyInfo;
                            if (p.IsSpecialName) continue;
                            if (!p.CanRead && bRead) continue;
                            if (!p.CanWrite && bWrite) continue;
                            if (p.GetIndexParameters().Length > 0) continue; //indexed properties are not allowed

                            if (VariantReference.AcceptableType(p.PropertyType)) yield return p;
                        }
                        break;
                }

            }
        }

        /// <summary>
        /// Returns true if the type is either System.Object or Variant.
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public static bool TypeIsVariantSupported(System.Type tp)
        {
            return tp == typeof(object) || tp == typeof(Variant);
        }

        #endregion

        #region Tokens

        /// <summary>
        /// Returns a state token with a shallow copy of all public properties/fields of 'obj'.
        /// The state token is possibly serializable but not guaranteed. 
        /// If 'obj' implements ITokenizable, then the object itself controls if the token is serializable.
        /// Otherwise a StateToken is returned which is serializable, but not all of the values of the copied members are serializable. That's up to them
        /// 
        /// Note - by serializable, this refers to .net serialization or any engine that supports the ISerialable interface. Not the unity serialization engine.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object CreateStateToken(object obj)
        {
            if (obj == null)
                return null;
            else if (obj is ITokenizable)
            {
                try
                {
                    return (obj as ITokenizable).CreateStateToken();
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                var token = StateToken.GetToken();
                token.CopyFrom(obj);
                return token;
            }
        }

        /// <summary>
        /// Restores the state of 'obj' based on 'token'. Token should be a state object that was returned by 'CreateStateToken'. 
        /// Respects ITokenizable interface on 'obj'.
        /// If the type of 'token' is mismatched, then this may likely fail.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        public static void RestoreFromStateToken(object obj, object token)
        {
            if (obj is ITokenizable)
            {
                try
                {
                    (obj as ITokenizable).RestoreFromStateToken(token);
                }
                catch
                {
                }
            }
            else
                CopyState(obj, token);
        }

        /// <summary>
        /// Like RestoreFromStateToken, but ignores ITokenizable interface.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        public static void CopyState(object obj, object source)
        {
            if (obj is IToken)
                (obj as IToken).SyncFrom(source);
            else if (source is IToken)
                (source as IToken).CopyTo(obj);
            else if (source != null)
            {
                foreach (var m in GetMembers(source, false, MemberTypes.Property | MemberTypes.Field))
                {
                    SetValue(obj, m.Name, GetValue(source, m));
                }
            }
        }

        /// <summary>
        /// Sync's obj and source's state for members that overlap.
        /// If obj is an IToken it respect's IToken.SyncFrom.
        /// If source is an IToken it respect's IToken.CopyTo.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        public static void SyncState(object obj, object source)
        {
            if (obj is IToken)
                (obj as IToken).SyncFrom(source);
            else if (source is IToken)
                (source as IToken).CopyTo(obj);
            else if (source != null)
            {
                foreach (var m in GetMembers(source, false, MemberTypes.Property | MemberTypes.Field))
                {
                    SetValue(obj, m.Name, GetValue(source, m));
                }
            }
        }

        #endregion


        #region Some Minor Helpers

        private static object ReduceSubObject(object obj, string sprop, out string lastProp)
        {
            if (obj == null)
            {
                lastProp = null;
                return null;
            }

            var arr = sprop.Split('.');
            lastProp = arr[arr.Length - 1];
            for (int i = 0; i < arr.Length - 1; i++)
            {
                obj = DynamicUtil.GetValue(obj, arr[i]);
                if (obj == null) return null;
            }
            
            return obj;
        }

        private static System.Type ReduceSubType(System.Type tp, string sprop, bool includeNonPublic, out string lastProp)
        {
            if (tp == null)
            {
                lastProp = null;
                return null;
            }

            var arr = sprop.Split('.');
            lastProp = arr[arr.Length - 1];
            for (int i = 0; i < arr.Length - 1; i++)
            {
                var member = DynamicUtil.GetValueMemberFromType(tp, arr[i], includeNonPublic);
                if (member == null) return null;

                tp = GetReturnType(member);
                if (tp == null) return null;
            }

            return tp;
        }

        private static bool ParameterSignatureMatches(object[] args, ParameterInfo[] paramInfos, bool convertToParamTypeIfCan)
        {
            if (args.Length != paramInfos.Length) return false;

            for (int i = 0; i < paramInfos.Length; i++)
            {
                if (args[i] == null)
                {
                    if (convertToParamTypeIfCan) args[i] = paramInfos[i].ParameterType.GetDefaultValue();
                    continue;
                }
                if (args[i].GetType().IsAssignableFrom(paramInfos[i].ParameterType))
                {
                    continue;
                }
                if (convertToParamTypeIfCan)
                {
                    //if (ConvertUtil.IsNumericType(paramInfos[i].ParameterType) && ConvertUtil.IsNumeric(args[i]))
                    //{
                    //    args[i] = ConvertUtil.ToPrim(args[i], paramInfos[i].ParameterType);
                    //    continue;
                    //}
                    if (ConvertUtil.IsNumericType(paramInfos[i].ParameterType) && args[i] is IConvertible)
                    {
                        args[i] = ConvertUtil.ToPrim(args[i], paramInfos[i].ParameterType);
                        continue;
                    }
                }

                return false;
            }

            return true;
        }

        private static IEnumerable<MemberInfo> FilterMembers(IEnumerable<MemberInfo> members, MemberTypes mask)
        {
            foreach (var m in members)
            {
                if ((m.MemberType & mask) != 0) yield return m;
            }
        }

        private static bool IsValidValueMember(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case System.Reflection.MemberTypes.Field:
                    return true;

                case System.Reflection.MemberTypes.Property:
                    {
                        var prop = member as System.Reflection.PropertyInfo;
                        if (prop.CanRead && prop.GetIndexParameters().Length == 0) return true;
                        break;
                    }
                case System.Reflection.MemberTypes.Method:
                    {
                        var meth = member as System.Reflection.MethodInfo;
                        if (meth.GetParameters().Length == 0) return true;
                        break;
                    }
            }
            return false;
        }

        private static bool IsValidValueSetterMember(MemberInfo member, System.Type valueType)
        {
            switch (member.MemberType)
            {
                case System.Reflection.MemberTypes.Field:
                    var field = member as System.Reflection.FieldInfo;
                    if (valueType == null || field.FieldType == valueType)
                    {
                        return true;
                    }

                    break;
                case System.Reflection.MemberTypes.Property:
                    var prop = member as System.Reflection.PropertyInfo;
                    if (prop.CanWrite && (valueType == null || prop.PropertyType.IsAssignableFrom(valueType)) && prop.GetIndexParameters().Length == 0)
                    {
                        return true;
                    }
                    break;
                case System.Reflection.MemberTypes.Method:
                    {
                        var meth = member as System.Reflection.MethodInfo;
                        var paramInfos = meth.GetParameters();
                        if (paramInfos.Length == 1 && paramInfos[0].ParameterType.IsAssignableFrom(valueType)) return true;
                    }
                    break;
            }
            return false;
        }

        #endregion

    }

}
