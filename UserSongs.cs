using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasIchHoerePlaylist
{
    /// <summary>
    /// Class for how many Songs a User added to the playlist.
    /// </summary>
    internal class UserSongs
    {
        /// <summary>
        /// Amount of Songs a User has added.
        /// </summary>
        public int SongCount { get; set; }

        /// <summary>
        /// UserID of a User.
        /// </summary>
        public ulong UserID { get; set; }

        /// <summary>
        /// Static List of all Users, User get added to this on Constructor
        /// </summary>
        public static List<UserSongs> ListOfUserSongs = new List<UserSongs>();

        /// <summary>
        /// Constructor, will add to static List automatically
        /// </summary>
        /// <param name="pUserID"></param>
        /// <param name="pSongCount"></param>
        private UserSongs(ulong pUserID, int pSongCount = 0)
        {
            this.UserID = pUserID;
            this.SongCount = pSongCount;
            ListOfUserSongs.Add(this);
        }


        /// <summary>
        /// Get the amount of Songs a User can add
        /// </summary>
        /// <param name="pUserID"></param>
        /// <returns></returns>
        public static int SongsUserCanAdd(ulong pUserID)
        {
            for (int i = 0; i <= ListOfUserSongs.Count - 1; i++)
            {
                // Match User ID
                if (ListOfUserSongs[i].UserID == pUserID)
                {
                    // If SongCount is LESS THAN daily limit
                    if (ListOfUserSongs[i].SongCount >= Options.USER_DAILY_LIMIT)
                    {
                        // We cant add anything
                        return 0;
                    }
                    else
                    {
                        // return difference between limits and addedSongs
                        return Options.USER_DAILY_LIMIT - ListOfUserSongs[i].SongCount;
                    }

                }
            }

            // If User does not exist in List, return Daily Limit
            return Options.USER_DAILY_LIMIT;
        }


        /// <summary>
        /// Check if a User can add a Song
        /// </summary>
        /// <param name="pUserID"></param>
        /// <returns></returns>
        public static bool CanUserAddSong(ulong pUserID)
        {
            // Loop through all Objects
            for (int i = 0; i <= ListOfUserSongs.Count - 1; i++)
            {
                // Match User ID
                if (ListOfUserSongs[i].UserID == pUserID)
                {
                    // If SongCount is LESS THAN daily limit
                    //Globals.DebugPrint("Me: '{0}', Daily Limit: '{1}'", ListOfUserSongs[i].SongCount, Options.USER_DAILY_LIMIT);
                    if (ListOfUserSongs[i].SongCount >= Options.USER_DAILY_LIMIT)
                    {
                        // return false since user cant add more
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Adds a Song to the UserSong if allowed
        /// </summary>
        /// <param name="pUserID"></param>
        /// <returns></returns>
        public static bool AddSongIfAllowed(ulong pUserID)
        {
            if (CanUserAddSong(pUserID))
            {
                AddUserSongs(pUserID, 1);
            }
            return true;
        }

        /// <summary>
        /// Adds
        /// </summary>
        /// <param name="pUserID"></param>
        /// <param name="pCounter"></param>
        public static void AddUserSongs(ulong pUserID, int pCounter = 1)
        {
            for (int i = 0; i <= ListOfUserSongs.Count - 1; i++)
            {
                if (ListOfUserSongs[i].UserID == pUserID)
                {
                    ListOfUserSongs[i].SongCount = ListOfUserSongs[i].SongCount + pCounter;
                    return;
                }
            }
            new UserSongs(pUserID, 1);
        }

        /// <summary>
        /// Gets how many Songs a User has added
        /// </summary>
        /// <param name="pUserID"></param>
        /// <returns></returns>
        public static int GetUserAdds(ulong pUserID)
        {
            for (int i = 0; i <= ListOfUserSongs.Count - 1; i++)
            {
                if (ListOfUserSongs[i].UserID == pUserID)
                {
                    return ListOfUserSongs[i].SongCount;
                }
            }
            return 0;
        }


        /// <summary>
        /// Sets User Songs
        /// </summary>
        /// <param name="pUserID"></param>
        /// <param name="pSongCount"></param>
        public static void SetUserAdds(ulong pUserID, int pSongCount)
        {
            bool FoundUser = false;
            for (int i = 0; i <= ListOfUserSongs.Count - 1; i++)
            {
                if (ListOfUserSongs[i].UserID == pUserID)
                {
                    FoundUser = true;
                    ListOfUserSongs[i].SongCount = pSongCount;
                }
            }

            if (!FoundUser)
            {
                if (pSongCount != 0)
                {
                    new UserSongs(pUserID, pSongCount);
                }
                // user not in list, and SongCount we set to ist 0 anywas...so do nothing
            }
        }

        /// <summary>
        /// Resets all User and their daily limits
        /// </summary>
        public static void Reset(ulong UserId = 0)
        {
            if (UserId == 0)
            {
                ListOfUserSongs.Clear();
            }
            else
            {
                SetUserAdds(UserId, 0);
            }
        }

    }
}
