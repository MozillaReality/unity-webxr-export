using System.Runtime.InteropServices;

public class WebXRUI {

	[DllImport("__Internal")]
	public static extern void displayXRElementId(string id);
}
