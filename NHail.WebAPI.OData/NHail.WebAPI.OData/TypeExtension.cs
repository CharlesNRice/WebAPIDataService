using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NHail.WebAPI.OData
{

    // "Borrowed" from http://code.msdn.microsoft.com/Entity-Framework-a958cffb

    /// <summary> 
    /// Extension method for type 
    /// </summary> 
    static class TypeExtension
    {
        /// <summary> 
        /// Get query element type 
        /// </summary> 
        /// <param name="type">The type of the query</param> 
        /// <returns>The element type</returns> 
        public static Type GetQueryElementType(this Type type)
        {
            var ienum = FindIEnumerable(type);

            //If type is IEnumerable<T>, then return T 
            return ienum == null ? type : ienum.GetGenericArguments()[0];

        }

        /// <summary> 
        /// Find the element type if seqType is IEnumerable<T> 
        /// </summary> 
        /// <param name="seqType">The type</param> 
        /// <returns>T if seqType is IEnumerable<T></returns> 
        public static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqType.IsGenericType)
            {
                foreach (
                    var ienum in
                        seqType.GetGenericArguments()
                               .Select(arg => typeof (IEnumerable<>).MakeGenericType(arg))
                               .Where(ienum => ienum.IsAssignableFrom(seqType)))
                {
                    return ienum;
                }
            }

            var ifaces = seqType.GetInterfaces();
            if (ifaces.Length > 0)
            {
                foreach (var ienum in ifaces.Select(FindIEnumerable).Where(ienum => ienum != null))
                {
                    return ienum;
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }

            return null;
        }
    } 

}