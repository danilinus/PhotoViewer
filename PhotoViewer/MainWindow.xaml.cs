using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;

namespace PhotoViewer
{
	public partial class MainWindow : Window
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref Win32Point pt);

		[StructLayout(LayoutKind.Sequential)]
		internal struct Win32Point { public int X, Y; };

		public static Point GetMousePosition()
		{
			Win32Point w32Mouse = new Win32Point();
			GetCursorPos(ref w32Mouse);
			return new Point(w32Mouse.X, w32Mouse.Y);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		private readonly BitmapImage image;

		public MainWindow(string path)
		{
			InitializeComponent();
			image = new BitmapImage(new Uri(path));
			(Width, Height) = (image.Width, image.Height);
			(Photo.Background, Icon) = (new ImageBrush(image), image);
			string[] name = path.Split('\\');
			Title = name[name.Length - 1] + " • PhotoViewer";
		}

		private Point offset = new Point(), WinPosition = new Point(), WinSize = new Point();

		private void TitleButton_MouseEnter(object sender, MouseEventArgs e) => ((Border)sender).Child.Visibility = Visibility.Visible;

		private void TitleButton_MouseLeave(object sender, MouseEventArgs e) => ((Border)sender).Child.Visibility = Visibility.Collapsed;

		private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) => Close();

		private void Window_Activated(object sender, EventArgs e) => ShowInTaskbar = false;

		private void TurnButton_MouseUp(object sender, MouseButtonEventArgs e)
		{
			ShowInTaskbar = true;
			WindowState = WindowState.Minimized;
		}

		MouseGrab grab = MouseGrab.None;

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.GetPosition(this).X < 10 && e.GetPosition(this).Y < 10)
				(offset, WinPosition, WinSize, grab) = (GetMousePosition(), new Point(Left, Top), new Point(Width, Height), MouseGrab.LeftTop);
			else
				if (e.GetPosition(this).X > Width - 10 && e.GetPosition(this).Y < 10)
				(offset, WinPosition, WinSize, grab) = (GetMousePosition(), new Point(Left, Top), new Point(Width, Height), MouseGrab.RightTop);
			else
				if (e.GetPosition(this).X < 10 && e.GetPosition(this).Y > Height - 10)
				(offset, WinPosition, WinSize, grab) = (GetMousePosition(), new Point(Left, Top), new Point(Width, Height), MouseGrab.LeftBottom);
			else
				if (e.GetPosition(this).X > Width - 10 && e.GetPosition(this).Y > Height - 10)
				(offset, WinPosition, WinSize, grab) = (GetMousePosition(), new Point(Left, Top), new Point(Width, Height), MouseGrab.RightBottom);
			else
				(offset, WinPosition, WinSize, grab) = (GetMousePosition(), new Point(Left, Top), new Point(Width, Height), MouseGrab.Move);
			new Thread(() =>
			{
				while (IsKeyDown(WinForms.Keys.LButton))
				{
					switch (grab)
					{
						case MouseGrab.Move:
							Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)(WinPosition.X + GetMousePosition().X - offset.X), (int)(WinPosition.Y + GetMousePosition().Y - offset.Y), (int)Width, (int)Height, true));
							break;
						case MouseGrab.RightBottom:
							if (IsKeyDown(WinForms.Keys.ShiftKey))
								Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)WinPosition.X, (int)(WinPosition.Y), (int)(WinSize.X + GetMousePosition().X - offset.X), (int)(WinSize.Y + GetMousePosition().Y - offset.Y), true));
							else
								Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)WinPosition.X, (int)(WinPosition.Y), (int)(WinSize.X + GetMousePosition().X - offset.X), (int)((WinSize.X + GetMousePosition().X - offset.X) / image.Width * image.Height), true));
							break;
						case MouseGrab.RightTop:
							if (IsKeyDown(WinForms.Keys.ShiftKey))
								Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)WinPosition.X, (int)(WinPosition.Y + GetMousePosition().Y - offset.Y), (int)(WinSize.X + GetMousePosition().X - offset.X), (int)(WinSize.Y - GetMousePosition().Y + offset.Y), true));
							else
								Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)WinPosition.X, (int)(WinPosition.Y + GetMousePosition().Y - offset.Y), (int)(WinSize.X + GetMousePosition().X - offset.X), (int)((WinSize.X + GetMousePosition().X - offset.X) / image.Width * image.Height), true));
							break;
						case MouseGrab.LeftTop:
							if (IsKeyDown(WinForms.Keys.ShiftKey))
								Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)(WinPosition.X + GetMousePosition().X - offset.X), (int)(WinPosition.Y + GetMousePosition().Y - offset.Y), (int)(WinSize.X - GetMousePosition().X + offset.X), (int)(WinSize.Y - GetMousePosition().Y + offset.Y), true));
							else
								Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)(WinPosition.X + GetMousePosition().X - offset.X), (int)(WinPosition.Y + GetMousePosition().Y - offset.Y), (int)(WinSize.X - GetMousePosition().X + offset.X), (int)((WinSize.X - GetMousePosition().X + offset.X) / image.Width * image.Height), true));
							break;
						case MouseGrab.LeftBottom:
							if (IsKeyDown(WinForms.Keys.ShiftKey))
								Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)(WinPosition.X + GetMousePosition().X - offset.X), (int)(WinPosition.Y), (int)(WinSize.X - GetMousePosition().X + offset.X), (int)(WinSize.Y + GetMousePosition().Y - offset.Y), true));
							else
								Dispatcher.Invoke(() => MoveWindow(GetForegroundWindow(), (int)(WinPosition.X + GetMousePosition().X - offset.X), (int)(WinPosition.Y), (int)(WinSize.X - GetMousePosition().X + offset.X), (int)((WinSize.X - GetMousePosition().X + offset.X) / image.Width * image.Height), true));
							break;
					}
					Thread.Sleep(3);
				}
				grab = MouseGrab.None;
			}).Start();
		}

		private double Distance(Point from, Point to) => Math.Sqrt(Math.Pow(from.X - to.X, 2) + Math.Pow(from.Y - to.Y, 2));

		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			CloseButton.Opacity = TurnButton.Opacity = 1 - Distance(e.GetPosition(CloseButton), new Point(CloseButton.Margin.Left, CloseButton.Margin.Top)) / 300f;

			if (e.GetPosition(this).X < 10 && e.GetPosition(this).Y < 10)
				Cursor = Cursors.SizeNWSE;
			else
			if (e.GetPosition(this).X > Width - 10 && e.GetPosition(this).Y < 10)
				Cursor = Cursors.SizeNESW;
			else
			if (e.GetPosition(this).X < 10 && e.GetPosition(this).Y > Height - 10)
				Cursor = Cursors.SizeNESW;
			else
			if (e.GetPosition(this).X > Width - 10 && e.GetPosition(this).Y > Height - 10)
				Cursor = Cursors.SizeNWSE;
			else
			if (IsKeyDown(WinForms.Keys.LButton))
				Cursor = Cursors.SizeAll;
			else
				Cursor = Cursors.Arrow;
		}

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern IntPtr GetForegroundWindow();

		[Flags]
		private enum KeyStates
		{
			None = 0,
			Down = 1,
			Toggled = 2
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern short GetKeyState(int keyCode);

		private static KeyStates GetKeyState(WinForms.Keys key)
		{
			KeyStates state = KeyStates.None;

			short retVal = GetKeyState((int)key);

			if ((retVal & 0x8000) == 0x8000)
				state |= KeyStates.Down;

			if ((retVal & 1) == 1)
				state |= KeyStates.Toggled;

			return state;
		}

		public static bool IsKeyDown(WinForms.Keys key) => KeyStates.Down == (GetKeyState(key) & KeyStates.Down);

		private void Window_MouseLeave(object sender, MouseEventArgs e) => CloseButton.Opacity = TurnButton.Opacity = 0;

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.OemPlus)
				Opacity += Opacity < 0.9f ? 0.1f : 1f - Opacity;
			else
			if (e.Key == Key.OemMinus)
				Opacity -= Opacity > 0.1f ? 0.1f : 0;
		}

		public static bool IsKeyToggled(WinForms.Keys key) => KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);

		enum MouseGrab
		{
			None,
			LeftTop,
			RightTop,
			LeftBottom,
			RightBottom,
			Move
		}
	}
}