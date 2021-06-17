using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using System.Linq;
using Android;
using Xamarin.Forms;
using Android.Database;
using Android.Provider;

namespace Image_Padder.Droid
{
    [Activity(Label = "Image Padder",
              Icon = "@drawable/icon",
              Theme = "@style/MainTheme",
              MainLauncher = true,
              HardwareAccelerated = true,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    [IntentFilter(new[] { Android.Content.Intent.ActionSend },
                  DataMimeType = "image/*",
                  Categories = new[] { Android.Content.Intent.CategoryDefault })]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            checkPermissions();

            string filepath = null;
            ImageSource imageSource = null;
            SixLabors.ImageSharp.Image sharpImage = null;

            if (Intent.ClipData != null)
            {
                // Get the clip item
                var clipItem = Intent.ClipData.GetItemAt(0);

                // Just in case we need it later, get the text of the image uri.
                var imageURI = clipItem.Uri;

                // Get the original file name
                ICursor returnCursor = ContentResolver.Query(imageURI, null, null, null, null);
                int nameIndex = returnCursor.GetColumnIndex(IOpenableColumns.DisplayName);
                returnCursor.MoveToFirst();
                filepath = returnCursor.GetString(nameIndex);

                // Open our input stream and create an image source from it.
                var inputStream = ContentResolver.OpenInputStream(imageURI);
                imageSource = ImageSource.FromStream(() => inputStream);

                // Initialize sharedImage now that we know the byte length of the image
                byte[] imageByteArray = new byte[inputStream.Length];

                // Read in the bytes of the image and reset the stream for later usage
                inputStream.Read(imageByteArray, 0, imageByteArray.Length);
                inputStream.Seek(0, System.IO.SeekOrigin.Begin);

                // Convert byte array to ImageSharp Image format
                sharpImage = SixLabors.ImageSharp.Image.Load(imageByteArray);
            }

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            App app = new App();
            LoadApplication(app);

            // Get the main page instance and send the information to it.
            MainPage mainPage = (MainPage)app.MainPage;
            mainPage.setImage(filepath, imageSource, sharpImage);
        }
        private void checkPermissions()
        {
            String[] requiredPermissions = new String[] { Manifest.Permission.WriteExternalStorage,
                                                     Manifest.Permission.ReadExternalStorage,
                                                     Manifest.Permission.AccessMediaLocation};

            foreach (var requiredPermission in requiredPermissions)
            {
                if (ContextCompat.CheckSelfPermission(this, requiredPermission) == (int)Permission.Granted)
                {
                    // Remove from list of ones to request
                    requiredPermissions = requiredPermissions.Where(value => value != requiredPermission).ToArray();
                }
            }

            if (requiredPermissions.Length != 0)
            {
                ActivityCompat.RequestPermissions(this, requiredPermissions, 1);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}