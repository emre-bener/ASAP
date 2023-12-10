using SAM.API;
using SAM.Game;
using SAM.Game.Stats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SAA.Core;
using APITypes = SAM.API.Types;

namespace SAA.Core
{
    public static class Utils
    {
        static System.Random rnd = new System.Random();

        public static double GetRandomDoubleInRange(int i)
            => 
            (rnd.NextDouble() - 0.5) * i;

        public static bool loadUserGameStatsSchema(
            string gameId,
            SAM.API.Client client,
            List<SAM.Game.Stats.AchievementDefinition> _AchievementDefinitions, 
            List<SAM.Game.Stats.StatDefinition> _StatDefinitions
            )
        {
            string path;
            
            try
            {
                path = Steam.GetInstallPath();
                path = Path.Combine(path, "appcache");
                path = Path.Combine(path, "stats");
                path = Path.Combine(path, string.Format(
                    CultureInfo.InvariantCulture,
                    "UserGameStatsSchema_{0}.bin",
                    gameId));

                if (File.Exists(path) == false)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            var kv = SAM.Game.KeyValue.LoadAsBinary(path);

            if (kv == null)
            {
                return false;
            }

            var currentLanguage = client.SteamApps008.GetCurrentGameLanguage();
            //var currentLanguage = "german";

            _AchievementDefinitions.Clear();
            _StatDefinitions.Clear();

            var stats = kv[gameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false ||
                stats.Children == null)
            {
                return false;
            }

            foreach (var stat in stats.Children)
            {
                if (stat.Valid == false)
                {
                    continue;
                }

                var rawType = stat["type_int"].Valid
                                  ? stat["type_int"].AsInteger(0)
                                  : stat["type"].AsInteger(0);
                var type = (APITypes.UserStatType)rawType;
                switch (type)
                {
                    case APITypes.UserStatType.Invalid:
                        {
                            break;
                        }

                    case APITypes.UserStatType.Integer:
                        {
                            var id = stat["name"].AsString("");
                            string name = Utils.GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                            _StatDefinitions.Add(new SAM.Game.Stats.IntegerStatDefinition()
                            {
                                Id = stat["name"].AsString(""),
                                DisplayName = name,
                                MinValue = stat["min"].AsInteger(int.MinValue),
                                MaxValue = stat["max"].AsInteger(int.MaxValue),
                                MaxChange = stat["maxchange"].AsInteger(0),
                                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                DefaultValue = stat["default"].AsInteger(0),
                                Permission = stat["permission"].AsInteger(0),
                            });
                            break;
                        }

                    case APITypes.UserStatType.Float:
                    case APITypes.UserStatType.AverageRate:
                        {
                            var id = stat["name"].AsString("");
                            string name = Utils.GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                            _StatDefinitions.Add(new SAM.Game.Stats.FloatStatDefinition()
                            {
                                Id = stat["name"].AsString(""),
                                DisplayName = name,
                                MinValue = stat["min"].AsFloat(float.MinValue),
                                MaxValue = stat["max"].AsFloat(float.MaxValue),
                                MaxChange = stat["maxchange"].AsFloat(0.0f),
                                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                DefaultValue = stat["default"].AsFloat(0.0f),
                                Permission = stat["permission"].AsInteger(0),
                            });
                            break;
                        }

                    case APITypes.UserStatType.Achievements:
                    case APITypes.UserStatType.GroupAchievements:
                        {
                            if (stat.Children != null)
                            {
                                foreach (var bits in stat.Children.Where(
                                    b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                                {
                                    if (bits.Valid == false ||
                                        bits.Children == null)
                                    {
                                        continue;
                                    }

                                    foreach (var bit in bits.Children)
                                    {
                                        string id = bit["name"].AsString("");
                                        string name = Utils.GetLocalizedString(bit["display"]["name"], currentLanguage, id);
                                        string desc = Utils.GetLocalizedString(bit["display"]["desc"], currentLanguage, "");

                                        _AchievementDefinitions.Add(new SAM.Game.Stats.AchievementDefinition()
                                        {
                                            Id = id,
                                            Name = name,
                                            Description = desc,
                                            IconNormal = bit["display"]["icon"].AsString(""),
                                            IconLocked = bit["display"]["icon_gray"].AsString(""),
                                            IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                            Permission = bit["permission"].AsInteger(0),
                                        });
                                    }
                                }
                            }

                            break;
                        }

                    default:
                        {
                            throw new InvalidOperationException("invalid stat type");
                        }
                }
            }

            return true;
        }
        public static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            if (language != "english")
            {
                name = kv["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false)
                {
                    return name;
                }
            }

            name = kv.AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            return defaultValue;
        }
    }
}
