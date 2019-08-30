using System.Windows;

namespace PhotoViewer
{
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (e.Args.Length > 0)
				new MainWindow(e.Args[0]).Show();
			else
				Shutdown();
		}
	}
}
