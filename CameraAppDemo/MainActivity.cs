using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Provider;
using Android.Graphics;
using Java.IO;
using Android.Net;

namespace CameraAppDemo
{
	[Activity (Label = "CameraAppDemo", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			ImageView img = (ImageView)FindViewById (Resource.Id.imageView1);
			img.SetImageResource (Resource.Drawable.click1);

			// Get our button from the layout resource,
			// and attach an event to it
			if (IsThereAnAppToTakePictures()) {
				CreateDirectoryForPictures ();
				Button button = FindViewById<Button> (Resource.Id.myButton);
				ImageView _imageView = FindViewById<ImageView> (Resource.Id.imageView1);

				if (App.bitmap != null) {
					_imageView.SetImageBitmap (App.bitmap);
					App.bitmap = null;
				}
				button.Click += TakeAPicture;
			}
		}
		private bool IsThereAnAppToTakePictures()
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities (intent, Android.Content.PM.PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}
		private void CreateDirectoryForPictures ()
		{
			App._dir = new File (Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "CameraAppDemo");

		}
		private void TakeAPicture(object sender, EventArgs eventArgs)
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);

			App._file = new File(App._dir, String.Format("myPhoto_"+Guid.NewGuid()+".jpg"));

			intent.PutExtra (MediaStore.ExtraOutput, Android.Net.Uri.FromFile (App._file));

			StartActivityForResult(intent, 0);
		}
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			// make it available in the gallery
			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App._file);
			mediaScanIntent.SetData(contentUri);
			SendBroadcast(mediaScanIntent);

			// display in ImageView. We will resize the bitmap to fit the display
			// Loading the full sized image will consume to much memory 
			// and cause the application to crash.
			int height = Resources.DisplayMetrics.HeightPixels;
			int width = Resources.DisplayMetrics.WidthPixels;
			App.bitmap = App._file.Path.LoadAndResizeBitmap (width, height);
		}

		}
	public static class App {
		public static File _file;
		public static File _dir;
		public static Bitmap bitmap;
	}
	public static class BitmapHelpers
	{
		public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
		{
		// First we get the the dimensions of the file on disk
		BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
		BitmapFactory.DecodeFile(fileName, options);

		// Next we calculate the ratio that we need to resize the image by
		// in order to fit the requested dimensions.
		int outHeight = options.OutHeight;
		int outWidth = options.OutWidth;
		int inSampleSize = 1;

		if (outHeight > height || outWidth > width)
		{
			inSampleSize = outWidth > outHeight
				? outHeight / height
				: outWidth / width;
		}

		// Now we will load the image and have BitmapFactory resize it for us.
		options.InSampleSize = inSampleSize;
		options.InJustDecodeBounds = false;
		Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

		return resizedBitmap;
	}

	
}
}
