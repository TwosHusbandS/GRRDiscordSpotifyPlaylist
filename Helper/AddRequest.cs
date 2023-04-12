using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist.Helper
{
    internal class AddRequest
    {
        public string Uri = "";
        public string SearchString = "";

        public AddRequest(string pUri = "", string pSearchString = "")
        {
            this.Uri = pUri; 
            this.SearchString = pSearchString;
        }


        /// <summary>
        /// Returns same list without null, empty, whitespace and duplicate strings
        /// </summary>
        /// <param name="MyList"></param>
        public static void RemoveNullEmptyWhitespaceDuplicateStringList(ref List<AddRequest> MyList)
        {
            try
            {
                MyList = MyList.Where(s => !string.IsNullOrWhiteSpace(s.Uri)).Distinct().ToList();
            }
            catch (Exception ex)
            {
                Helper.Logger.Log(ex);
                MyList = new List<AddRequest>();
            }
        }
    }
}
