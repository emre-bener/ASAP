using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using SAM.API;
using System.Security.Cryptography;
using System.IO;
using APITypes = SAM.API.Types;
using SAA.Core.Random;
using System.Threading.Tasks;
using SAM.Game.Stats;

namespace SAA.Core
{
    internal class Program
    {
        #region PROPERTIES
        private static SAM.API.Client client;

        public static List<AchievementDefinition> AllAchivements = new List<AchievementDefinition>();
        public static List<AchievementDefinition> UnearnedAchivements = new List<AchievementDefinition>();

        public static List<StatDefinition> FullStatList = new List<StatDefinition>(); // i dont even care

        // public static PlannedUnlockSequence CurrentSequence = new PlannedUnlockSequence();

        public static int NUDGE_VALUE = 5; // TODO
        #endregion

        // MAIN METHOD
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments given, terminating...");
                return;
            }

            foreach (var gameId in args)
            {
                try
                {
                    #region REINITIALIZE STUFF
                    initClientForGame(gameId); // init steam client
                    Utils.loadUserGameStatsSchema( // init achivement list
                        gameId: gameId,
                        client: client,
                        _AchievementDefinitions: AllAchivements,
                        _StatDefinitions: FullStatList
                        );
                    // CurrentSequence = new PlannedUnlockSequence { AchivementChain = new List<AchivementStep>() }; // reset sequence
                    #endregion

                    // PREPARE UNEARNED ACHIVEMENTS LIST
                    foreach (var achiv in AllAchivements)
                    {
                        if (string.IsNullOrEmpty(achiv.Id))
                            continue;

                        bool isAchieved;
                        if (client.SteamUserStats.GetAchievementState(achiv.Id, out isAchieved) == false)
                            continue;

                        if (!isAchieved)
                            UnearnedAchivements.Add(achiv);

                        //CurrentSequence.AchivementChain.Add(new AchivementStep
                        //{
                        //    Achivement = achiv,
                        //    RequiredTime = TimeSpan.FromMinutes(5) + TimeSpan.FromMinutes(Utils.GetRandomDoubleInRange(NUDGE_VALUE)),
                        //    IsDone = false
                        //});
                    }



                    // TimeSpan.FromSeconds(5)
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Initializes (or re-initializes) steam client
        /// </summary>
        static ResponseModel initClientForGame(string gameId)
        {
            //Process.Start("SAM.Game.exe", gameId.ToString(CultureInfo.InvariantCulture));

            long appId;
            if (long.TryParse(gameId, out appId) == false)
                return new ResponseModel
                {
                    Success = false,
                    ErrorMessage = "Invalid game id."
                };

            try
            {
                // reinitialize steam client for this specific game
                client = new SAM.API.Client();
                client.Initialize(appId);
            }
            catch (SAM.API.ClientInitializeException e)
            {
                if (e.Failure == SAM.API.ClientInitializeFailure.ConnectToGlobalUser)
                {
                    return new ResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Steam is not running. Please start Steam then run this tool again.\\n\\n\" +\r\n                            \"If you have the game through Family Share, the game may be locked due to\\n\\n\" +\r\n                            \"the Family Share account actively playing a game.\\n\\n\" +\r\n                            \"(\" + e.Message + \")\",\r\n                            \"Error\""
                    };
                }
                else if (string.IsNullOrEmpty(e.Message) == false)
                {
                    return new ResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Steam is not running. Please start Steam then run this tool again.\n\n" +
                        "(" + e.Message + ")"
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Steam is not running. Please start Steam then run this tool again."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }

            return new ResponseModel
            {
                Success = true
            };
        }




    }
}


// get curent game name
// client.SteamApps001.GetAppData((uint)appId, "name");


// get status of a specific achivement
// _SteamClient.SteamUserStats.GetAchievementState(def.Id, out isAchieved)