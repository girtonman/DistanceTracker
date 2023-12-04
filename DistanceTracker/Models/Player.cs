using DistanceTracker.DALs;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.Models
{
	public class Player
	{
		public uint ID { get; set; }
		public ulong SteamID { get; set; }
		public string Name { get; set; }
		public string SteamAvatar { get; set; }

		public async Task<string> GetSteamAvatar(SteamDAL steamDAL, PlayerDAL playerDAL, string suffix = null)
		{
			if (SteamAvatar == "Unknown")
			{
				return "https://steamuserimages-a.akamaihd.net/ugc/868480752636433334/1D2881C5C9B3AD28A1D8852903A8F9E1FF45C2C8/";
			}

			if (SteamAvatar == null)
			{
				var players = await steamDAL.GetPlayerSummaries(SteamID);
				var player = players.FirstOrDefault();
				if(player == null)
				{
					SteamAvatar = "Unknown";
				}
				else
				{
					SteamAvatar = player.Avatar;
					await playerDAL.UpdateSteamAvatar(SteamID, SteamAvatar);
				}
			}

			if(string.IsNullOrEmpty(suffix))
			{
				return SteamAvatar;
			}

			// Apply suffix if given
			var trimmedURL = SteamAvatar.TrimEnd(".jpg".ToCharArray());
			return trimmedURL += suffix + ".jpg";
		}
	}
}
