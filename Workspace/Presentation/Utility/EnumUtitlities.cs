using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace GPAS.Workspace.Presentation
{
    internal class EnumUtitlities
    {
        private EnumUtitlities()
        { }

        /// <summary>
        /// توضیحات کاربرپسند تعریف شده برای اجزای انواع شمارش پذیر را برمی گرداند
        /// </summary>
        /// <remarks>
        /// ویژگی System.ComponentModel.Description را برای مقادیر Enum های خود پیاده کنید
        /// </remarks>
        /// <returns>در صورت عدم تعریف توضیحات برای جزء شمارش، عنوان نوع موردنظر را تبدیل به رشته کرده و برمی گرداند </returns>
        internal static string GetUserFriendlyDescriptionOfAnEnumInstance(Enum value)
        {
            try
            {
                // Code Source (with some changes): http://stackoverflow.com/questions/1415140/can-my-enums-have-friendly-names

                Type type = value.GetType();
                string name = Enum.GetName(type, value);
                if (name != null)
                {
                    FieldInfo field = type.GetField(name);
                    if (field != null)
                    {
                        DescriptionAttribute attr =
                               Attribute.GetCustomAttribute(field,
                                 typeof(DescriptionAttribute)) as DescriptionAttribute;
                        if (attr != null)
                        {
                            return attr.Description;
                        }
                    }
                }
                return value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }

        internal static IEnumerable<T> GetEnumElements<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
