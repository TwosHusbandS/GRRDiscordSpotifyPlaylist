using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.Helper
{
    internal class ListHelper
    {
        /// <summary>
        /// Returns same list without null, empty, whitespace and duplicate strings
        /// </summary>
        /// <param name="MyList"></param>
        public static void RemoveNullEmptyWhitespaceDuplicateStringList(ref List<string> MyList)
        {
            try
            {
                MyList = MyList.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
                MyList = new List<string>();
            }
        }


        /// <summary>
        /// Manipulates list (no null, empty, whitespace, duplicate strings) and returns if we have members in list
        /// </summary>
        /// <param name="MyList"></param>
        /// <returns></returns>
        public static bool CanContinueWithList(ref List<string> MyList)
        {
            RemoveNullEmptyWhitespaceDuplicateStringList(ref MyList);
            if (MyList.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
