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
			var url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={SteamAPIKey}&steamids={steamID}";
			var wr = WebRequest.Create(url);

			var response = await wr.GetResponseAsync();
			var stream = response.GetResponseStream();
			var reader = new StreamReader(stream);
			var responseMessage = reader.ReadLine();

			var json = JObject.Parse(responseMessage);
			var players = json["response"]["players"].ToObject<List<SteamPlayer>>();

			return players;
		}
	}
}
