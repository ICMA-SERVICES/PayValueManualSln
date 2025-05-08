using System;
using System.Text;

namespace PayValueManualSln.Infrastructure.Persistence.Helpers
{
    public class NumberToWordsConverter
    {
        private static string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
        private static string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        private static string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        public static string ConvertToWords(decimal number)
        {
            if (number == 0)
                return "Zero";

            StringBuilder words = new StringBuilder();

            if (number < 0)
            {
                words.Append("Negative ");
                number = Math.Abs(number);
            }

            // Convert the integer part to words
            words.Append(ConvertToWordsHelper((long)Math.Floor(number)));

            // Deal with the fractional part
            if (number % 1 != 0)
            {
                words.Append(" Naira and ");

                // Extract the fractional part and convert it to words
                decimal fractionalPart = number % 1 * 100; // Assuming fractional part represents the kobo amount
                words.Append(ConvertToWordsHelper((long)Math.Floor(fractionalPart)) + " Kobo");
            }
            else
            {
                words.Append(" Naira");
            }

            return words.ToString();
        }

        private static string ConvertToWordsHelper(long number)
        {
            if (number < 10)
            {
                return units[number];
            }
            else if (number < 20)
            {
                return teens[number - 10];
            }
            else if (number < 100)
            {
                return tens[number / 10] + ((number % 10 > 0) ? " " + ConvertToWordsHelper(number % 10) : "");
            }
            else if (number < 1000)
            {
                return units[number / 100] + " Hundred" + ((number % 100 > 0) ? " and " + ConvertToWordsHelper(number % 100) : "");
            }
            else if (number < 1000000)
            {
                return ConvertToWordsHelper(number / 1000) + " Thousand" + ((number % 1000 > 0) ? ", " + ConvertToWordsHelper(number % 1000) : "");
            }
            else if (number < 1000000000)
            {
                return ConvertToWordsHelper(number / 1000000) + " Million" + ((number % 1000000 > 0) ? ", " + ConvertToWordsHelper(number % 1000000) : "");
            }
            else if (number < 1000000000000)
            {
                return ConvertToWordsHelper(number / 1000000000) + " Billion" + ((number % 1000000000 > 0) ? ", " + ConvertToWordsHelper(number % 1000000000) : "");
            }
            else
            {
                throw new NotSupportedException("Numbers greater than or equal to 1 trillion are not supported.");
            }
        }
    }


    //public class ConvertToWords
    //{
    //    public static class AmountInWords
    //    {
    //        private static String Ones(String Number)
    //        {
    //            int _Number = Convert.ToInt32(Number);
    //            String name = "";
    //            switch (_Number)
    //            {

    //                case 1:
    //                    name = "One";
    //                    break;
    //                case 2:
    //                    name = "Two";
    //                    break;
    //                case 3:
    //                    name = "Three";
    //                    break;
    //                case 4:
    //                    name = "Four";
    //                    break;
    //                case 5:
    //                    name = "Five";
    //                    break;
    //                case 6:
    //                    name = "Six";
    //                    break;
    //                case 7:
    //                    name = "Seven";
    //                    break;
    //                case 8:
    //                    name = "Eight";
    //                    break;
    //                case 9:
    //                    name = "Nine";
    //                    break;
    //            }
    //            return name;
    //        }

    //        private static String Tens(String Number)
    //        {
    //            int _Number = Convert.ToInt32(Number);
    //            String name = null;
    //            switch (_Number)
    //            {
    //                case 10:
    //                    name = "Ten";
    //                    break;
    //                case 11:
    //                    name = "Eleven";
    //                    break;
    //                case 12:
    //                    name = "Twelve";
    //                    break;
    //                case 13:
    //                    name = "Thirteen";
    //                    break;
    //                case 14:
    //                    name = "Fourteen";
    //                    break;
    //                case 15:
    //                    name = "Fifteen";
    //                    break;
    //                case 16:
    //                    name = "Sixteen";
    //                    break;
    //                case 17:
    //                    name = "Seventeen";
    //                    break;
    //                case 18:
    //                    name = "Eighteen";
    //                    break;
    //                case 19:
    //                    name = "Nineteen";
    //                    break;
    //                case 20:
    //                    name = "Twenty";
    //                    break;
    //                case 30:
    //                    name = "Thirty";
    //                    break;
    //                case 40:
    //                    name = "Forty";
    //                    break;
    //                case 50:
    //                    name = "Fifty";
    //                    break;
    //                case 60:
    //                    name = "Sixty";
    //                    break;
    //                case 70:
    //                    name = "Seventy";
    //                    break;
    //                case 80:
    //                    name = "Eighty";
    //                    break;
    //                case 90:
    //                    name = "Ninety";
    //                    break;
    //                default:
    //                    if (_Number > 0)
    //                    {
    //                        name = Tens(Number.Substring(0, 1) + "0") + " " + Ones(Number.Substring(1));
    //                    }
    //                    break;
    //            }
    //            return name;
    //        }

    //        private static String ConvertWholeNumber(String Number)
    //        {
    //            string word = "";
    //            try
    //            {
    //                bool beginsZero = false;//tests for 0XX  
    //                bool isDone = false;//test if already translated  
    //                double dblAmt = (Convert.ToDouble(Number));
    //                //if ((dblAmt > 0) && number.StartsWith("0"))  
    //                if (dblAmt > 0)
    //                {//test for zero or digit zero in a nuemric  
    //                    beginsZero = Number.StartsWith("0");

    //                    int numDigits = Number.Length;
    //                    int pos = 0;//store digit grouping  
    //                    String place = "";//digit grouping name:hundres,thousand,etc...  
    //                    switch (numDigits)
    //                    {
    //                        case 1://ones' range  

    //                            word = Ones(Number);
    //                            isDone = true;
    //                            break;
    //                        case 2://tens' range  
    //                            word = Tens(Number);
    //                            isDone = true;
    //                            break;
    //                        case 3://hundreds' range  
    //                            pos = (numDigits % 3) + 1;
    //                            place = " Hundred and ";
    //                            break;
    //                        case 4://thousands' range  
    //                        case 5:
    //                        case 6:
    //                            pos = (numDigits % 4) + 1;
    //                            place = " Thousand, ";
    //                            break;
    //                        case 7://millions' range  
    //                        case 8:
    //                        case 9:
    //                            pos = (numDigits % 7) + 1;
    //                            place = " Million, ";
    //                            break;
    //                        case 10://Billions's range  
    //                        case 11:
    //                        case 12:

    //                            pos = (numDigits % 10) + 1;
    //                            place = " Billion, ";
    //                            break;
    //                        //add extra case options for anything above Billion...  
    //                        default:
    //                            isDone = true;
    //                            break;
    //                    }
    //                    if (!isDone)
    //                    {//if transalation is not done, continue...(Recursion comes in now!!)  
    //                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
    //                        {
    //                            try
    //                            {
    //                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
    //                            }
    //                            catch { }
    //                        }
    //                        else
    //                        {
    //                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
    //                        }

    //                        //check for trailing zeros  
    //                        //if (beginsZero) word = " and " + word.Trim();  
    //                    }
    //                    //ignore digit grouping names  
    //                    if (word.Trim().Equals(place.Trim())) word = "";
    //                }
    //            }
    //            catch { }
    //            return word.Trim();
    //        }

    //        public static String ConvertToWords(String numb)
    //        {
    //            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
    //            String endStr = " Naira";
    //            //String endStr = " Naira Only";
    //            //String endStr = "";
    //            try
    //            {
    //                int decimalPlace = numb.IndexOf(".");
    //                if (decimalPlace > 0)
    //                {
    //                    wholeNo = numb.Substring(0, decimalPlace);
    //                    points = numb.Substring(decimalPlace + 1);
    //                    if (Convert.ToInt32(points) > 0)
    //                    {
    //                        andStr = " Naira and ";// just to separate whole numbers from points/cents  
    //                        endStr = "Kobo" + " " + endStr;//Cents  
    //                        pointStr = ConvertWholeNumber(points);
    //                    }
    //                }
    //                val = String.Format("{0} {1}{2} {3}", ConvertWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
    //                val = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val.ToLower());
    //            }
    //            catch { }
    //            //return val.ToUpper();
    //            if (val.Contains(" Kobo"))
    //            {
    //                val = val.Remove(val.Length - 5);
    //                return val;
    //            }
    //            //val = val.Replace("Kobo", "Naira");
    //            return val;
    //        }

    //        private static String ConvertDecimals(String number)
    //        {
    //            String cd = "", digit = "", engOne = "";
    //            for (int i = 0; i < number.Length; i++)
    //            {
    //                digit = number[i].ToString();
    //                if (digit.Equals("0"))
    //                {
    //                    engOne = "Zero";
    //                }
    //                else
    //                {
    //                    engOne = Ones(digit);
    //                }
    //                cd += " " + engOne;
    //            }
    //            return cd;
    //        }
    //    }
    //}
}
