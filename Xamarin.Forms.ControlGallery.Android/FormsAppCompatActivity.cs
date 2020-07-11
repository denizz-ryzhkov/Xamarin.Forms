#if !FORMS_APPLICATION_ACTIVITY && !PRE_APPLICATION_CLASS

using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Java.Interop;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppLinks;
using System.Linq;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Views;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.ControlGallery.Android
{
	// This is the AppCompat version of Activity1

	[Activity(Label = "Control Gallery", Icon = "@drawable/icon", Theme = "@style/MyTheme",
		MainLauncher = true, HardwareAccelerated = true, 
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
	[IntentFilter(new[] { Intent.ActionView },
		Categories = new[]
		{
			Intent.ActionView,
			Intent.CategoryDefault,
			Intent.CategoryBrowsable
		},
		DataScheme = "http", DataHost = App.AppName, DataPathPrefix = "/gallery/"
		)
	]
	public partial class Activity1 : FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			Profile.Start();

			ToolbarResource = Resource.Layout.Toolbar;
			TabLayoutResource = Resource.Layout.Tabbar;

			// Uncomment the next line to run this as a full screen app (no status bar)
			//Window.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TurnScreenOn);

			base.OnCreate(bundle);

#if TEST_EXPERIMENTAL_RENDERERS
#else
			Forms.SetFlags("UseLegacyRenderers", "SwipeView_Experimental", "MediaElement_Experimental", "AppTheme_Experimental");
#endif
			Forms.Init(this, bundle);

			FormsMaps.Init(this, bundle);
			FormsMaterial.Init(this, bundle);
			AndroidAppLinks.Init(this);
			Forms.ViewInitialized += (sender, e) => {
				//				if (!string.IsNullOrWhiteSpace(e.View.StyleId)) {
				//					e.NativeView.ContentDescription = e.View.StyleId;
				//				}
			};

			// uncomment to verify turning off title bar works. This is not intended to be dynamic really.
			//Forms.SetTitleBarVisibility (AndroidTitleBarVisibility.Never);

			if (RestartAppTest.App != null)
			{
				_app = (App)RestartAppTest.App;
				RestartAppTest.Reinit = true;
			}
			else
			{
				_app = new App();
			}

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
			MessagingCenter.Subscribe<NativeBindingGalleryPage>(this, NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);

			// Listen for the message from the status bar color toggle test
			MessagingCenter.Subscribe<AndroidStatusBarColor>(this, AndroidStatusBarColor.Message, color => SetStatusBarColor(global::Android.Graphics.Color.Red));

			SetUpForceRestartTest();

			// Make the activity accessible to platform unit tests
			DependencyResolver.ResolveUsing((t) => {
				if (t == typeof(Context))
				{
					return this;
				}

				return null;
			});

			DependencyService.Register<IMultiWindowService, MultiWindowService>();
			
			LoadApplication(_app);
			this.Window.SetSoftInputMode(SoftInput.AdjustResize);
			var rootView = Window.DecorView.RootView;


#if !TEST_EXPERIMENTAL_RENDERERS
			if ((int)Build.VERSION.SdkInt >= 21)
			{
				// Show a purple status bar if we're looking at legacy renderers
				Window.SetStatusBarColor(Color.MediumPurple.ToAndroid());
			}
#endif
		}

		public void ReloadApplication()
		{
			LoadApplication(_app);
		}

		protected override void OnResume()
		{
			base.OnResume();
			Profile.Stop();
			var listenr = new GlobalLayoutListenr();
			listenr.ContentView = Window.DecorView.RootView;
			listenr.OnKeyboardChanged += ListenrOnOnKeyboardChanged;
			Window.DecorView.RootView.ViewTreeObserver.AddOnGlobalLayoutListener(listenr);
		}


		[Export("IsPreAppCompat")]
		public bool IsPreAppCompat()
		{
			return false;
		}

		[Java.Interop.Export("BackgroundApp")]
		public void BackgroundApp()
		{
			Intent intent = new Intent();
			intent.SetAction(Intent.ActionMain);
			intent.AddCategory(Intent.CategoryHome);
			this.StartActivity(intent);
		}

		[Java.Interop.Export("ForegroundApp")]
		public void ForegroundApp()
		{
			// this only works pre API 29
			Intent intent = new Intent(ApplicationContext, typeof(Activity1));
			intent.SetAction(Intent.ActionMain);
			intent.AddCategory(Intent.CategoryLauncher);
			this.ApplicationContext.StartActivity(intent);
		}

		private BottomNavigationView _bottomNavigationView;
		void ListenrOnOnKeyboardChanged(object sender, LayoutListenrEventArgs e)
		{
			if (_bottomNavigationView == null)
			{
				var result = TryGetBottomNavigation();
				if (result == false)
					return;
			}

			if (e.KeyboardVisible)
			{
				_bottomNavigationView.Visibility = ViewStates.Gone;
			}
			else
			{
				_bottomNavigationView.Visibility = ViewStates.Visible;
			}

		}

		bool TryGetBottomNavigation()
		{
			if (Window.DecorView.RootView is ViewGroup viewGroup)
			{
				if (viewGroup.GetChildAt(0) is ViewGroup viewGroup1)
				{
					if (viewGroup1.GetChildAt(1) is ViewGroup viewGroup2)
					{
						if (viewGroup2.GetChildAt(0) is ViewGroup viewGroup3)
						{
							if (viewGroup3.GetChildAt(0) is ViewGroup viewGroup4)
							{
								if (viewGroup4.GetChildAt(0) is ViewGroup viewGroup5)
								{
									if (viewGroup5.GetChildAt(0) is ViewGroup viewGroup6)
									{
										if (viewGroup6.GetChildAt(0) is ViewGroup viewGroup7)
										{
											if (viewGroup7.GetChildAt(0) is ViewGroup viewGroup8)
											{
												if (viewGroup8.GetChildAt(1) is ViewGroup viewGroup9)
												{
													if (viewGroup9 is BottomNavigationView bottomNavigation)
													{
														_bottomNavigationView = bottomNavigation;
														_bottomNavigationView.Background = new ColorDrawable(global::Android.Graphics.Color.DarkRed);
														return true;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return false;
		}
	}

	class GlobalLayoutListenr : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
	{
		public global::Android.Views.View ContentView { get; set; }
		public event EventHandler<LayoutListenrEventArgs> OnKeyboardChanged = delegate { };
		public void OnGlobalLayout()
		{
			var rect = new global::Android.Graphics.Rect();
			ContentView?.GetWindowVisibleDisplayFrame(rect);
			var scrennHeight = ContentView.RootView.Height;
			var kepadheing = scrennHeight - rect.Bottom;
			var keyBoadVisible = kepadheing > scrennHeight * 0.15;

			OnKeyboardChanged(this, new LayoutListenrEventArgs()
			{
				KeyboardVisible = keyBoadVisible,
				Height = rect.Height()
			});
		}
	}

	class LayoutListenrEventArgs : EventArgs
	{
		public bool KeyboardVisible { get; set; }
		public int Height { get; set; }
	}
}

#endif