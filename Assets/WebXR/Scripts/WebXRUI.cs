using System.Runtime.InteropServices;

namespace WebXR
{
	public class WebXRUI
	{

		[DllImport("__Internal")]
		public static extern void displayXRElementId(string id);
	}
}