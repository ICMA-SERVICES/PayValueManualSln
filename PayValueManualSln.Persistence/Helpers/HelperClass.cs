using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayValueManualSln.Persistence.Helpers
{
    public static class HelpersClasses
    {
        public static string ToDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static TEnum ParseEnumOrDefault<TEnum>(string value, TEnum defaultValue) where TEnum : struct
        {
            return Enum.TryParse(value, true, out TEnum result) ? result : defaultValue;
        }

        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            var emailRegex = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

            return Regex.IsMatch(email, emailRegex, RegexOptions.IgnoreCase);
        }

        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return false;
            var phoneNumberRegex = @"^\d{11}$";
            bool isEmailValid = Regex.IsMatch(phoneNumber, phoneNumberRegex, RegexOptions.IgnoreCase);
            return Regex.IsMatch(phoneNumber, phoneNumberRegex, RegexOptions.IgnoreCase);
        }
    }
}
