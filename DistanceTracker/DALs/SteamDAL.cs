using DistanceTracker.Models.Steam;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class SteamDAL
	{
		public SteamDAL(Settings settings)
		{
			SteamAPIKey = settings.SteamAPIKey;
		}

		private string SteamAPIKey { get; set; }

		public async Task<List<SteamPlayer>> GetPlayerSummaries(ulong steamID)
		{
			var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={SteamAPIKey}&steamids={steamID}";
			var wr = WebRequest.Create(url);

			var response = await wr.GetResponseAsync();
			var stream = response.GetResponseStream();
			var reader = new StreamReader(stream);
			var responseMessage = reader.ReadLine();

			var json = JObject.Parse(responseMessage);
			var players = json["response"]["players"].ToObject<List<SteamPlayer>>();

			return players;
		}

		public async Task<string> GetProfileBackground(ulong steamID)
		{
			var url = $"https://api.steampowered.com/IPlayerService/GetProfileBackground/v1/?key={SteamAPIKey}&steamid={steamID}";
			var wr = WebRequest.Create(url);

			var response = await wr.GetResponseAsync();
			var stream = response.GetResponseStream();
			var reader = new StreamReader(stream);
			var responseMessage = reader.ReadLine();

			var json = JObject.Parse(responseMessage);
			var bgImage = json["response"]["profile_background"]["image_large"]?.ToString();
			var bgAnimated = json["response"]["profile_background"]["movie_webm"]?.ToString();
			var background = string.IsNullOrEmpty(bgAnimated) ? bgImage : bgAnimated;

			var bgURL = string.IsNullOrEmpty(background) ? null : $"https://cdn.akamai.steamstatic.com/steamcommunity/public/images/{background}";

			return bgURL;
		}
	}
}
