﻿using System;
#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif IPHONE
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace <SampleNamespace>
{
#if MONOMAC
	class Program
	{
		static void Main (string[] args)
		{
			NSApplication.Init ();

			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new AppDelegate ();

				// Set our Application Icon
				NSImage appIcon = NSImage.ImageNamed ("monogameicon.png");
				NSApplication.SharedApplication.ApplicationIconImage = appIcon;
				
				NSApplication.Main (args);
			}
		}
	}

	class AppDelegate : NSApplicationDelegate
	{
		private <SampleGameName> game;

		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			game = new <SampleGameName>();
			game.Run();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}
#elif IPHONE
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		private <SampleGameName> game;

		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			game = new <SampleGameName>();
			game.Run();
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
#else
	#if !WINDOWS_PHONE
		/// <summary>
		/// The main class.
		/// </summary>
		public static class Program
		{
			/// <summary>
			/// The main entry point for the application.
			/// </summary>
			static void Main()
			{
	#if WINDOWS || LINUX || PSM
				using (var game = new <SampleGameName>())
					game.Run();

	#else
				var factory = new MonoGame.Framework.GameFrameworkViewSource<<SampleGameName>>();
				Windows.ApplicationModel.Core.CoreApplication.Run(factory);
	#endif
			}
		}
	#endif
#endif

}
