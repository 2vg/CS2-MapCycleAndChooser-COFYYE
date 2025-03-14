using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using Mappen.Variables;
using System.Collections.Generic;
using System.Linq;

namespace Mappen.Utils
{
    public static class PlayerUtils
    {
        public static Mappen Instance => Mappen.Instance;

        public static bool IsValidPlayer(CCSPlayerController? p)
        {
            return p != null && p.IsValid && !p.IsBot && !p.IsHLTV && p.Connected == PlayerConnectedState.PlayerConnected;
        }

        public static void NotifyAllPlayers(string localizerKey, Dictionary<string, string>? replacements = null)
        {
            var players = Utilities.GetPlayers().Where(p => IsValidPlayer(p));
            foreach (var player in players)
            {
                string message = Instance?.Localizer.ForPlayer(player, localizerKey) ?? "";
                if (replacements != null)
                {
                    foreach (var replacement in replacements)
                    {
                        message = message.Replace(replacement.Key, replacement.Value);
                    }
                }
                player.PrintToChat(message);
            }
        }
    }
}
