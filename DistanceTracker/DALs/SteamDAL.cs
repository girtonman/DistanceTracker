using DistanceTracker.Models.Steam;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class SteamDAL
	{
		public SteamDAL() { }

		public async Task<List<SteamPlayer>> GetPlayerSummaries(ulong steamID)
		{
			var url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={Settings.SteamAPIKey}&steamids={steamID}";
			var wr = WebRequest.Create(url);

			//var myProxy = new WebProxy("myproxy", 80);
			//myProxy.BypassProxyOnLocal = true;
			//wr.Proxy = WebProxy.GetDefaultProxy();

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
