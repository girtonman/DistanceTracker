namespace DistanceTracker.Models.Steam
{
	public class SteamPlayer
	{
		public ulong SteamID { get; set; }
		public int CommunityVisibilityState { get; set; }
		public int ProfileState { get; set; }
		public string PersonaName { get; set; }
		public string ProfileURL { get; set; }
		public string Avatar { get; set; }
		public string AvatarMedium { get; set; }
		public string AvatarFull { get; set; }
		public string AvatarHash { get; set; }
		public uint LastLogOff { get; set; }
		public int PersonaState { get; set; }
		public ulong PrimaryClanID { get; set; }
		public uint TimeCreated { get; set; }
		public int PersonaStateFlags { get; set; }
	}
}
