using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
	public partial class PeoneImage
	{
		[Key]
		public string TwitterUrl { get; set; }
	}
}
