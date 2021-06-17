using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Forms;

using AndroidX.Core.Content;
using Android;
using AndroidX.Core.App;
using System.Linq;
using Android.Database;
using Android.Provider;
using Android.Content;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace Image_Padder
{
    public partial class MainPage : ContentPage
    {
        private string filepath;
        private ImageSource imageSource;
        private SixLabors.ImageSharp.Image sharpImage;

        public MainPage()
        {
            InitializeComponent();
        }

        public void setImage(string filepath, ImageSource imageSource, SixLabors.ImageSharp.Image sharpImage)
        {
            // If the app is opened from the launcher, don't do anything.
            if (sharpImage == null)
            {
                imageBuffer.Text = "No image found. You must share the image to this app through the share sheet.";
                shareButton.IsVisible = false;
                widthEntry.IsVisible = false;
                heightEntry.IsVisible = false;
                padButton.IsVisible = false;
                return;
            }

            // Store values in class
            this.filepath = filepath;
            this.imageSource = imageSource;
            this.sharpImage = sharpImage;

            filepath = buildFilePath(filepath);

            // We need to save a copy of the file so we can pass it on to the next app.
            sharpImage.Save(filepath);

            imageBuffer.IsVisible = false;
            sharedImage.Source = ImageSource.FromFile(filepath);
            this.filepath = filepath;
        }

        private string buildFilePath(string filepath)
        {
            // Build up the file name.
            Android.Content.Context context = Android.App.Application.Context;
            Java.IO.File imagePath = new Java.IO.File(context.FilesDir, "images");
            imagePath.Mkdirs();
            filepath = "twitter_decropper" + "." + filepath.Split('.').Last();
            Java.IO.File newFile = new Java.IO.File(imagePath, filepath);
            filepath = newFile.AbsolutePath;
            return filepath;
        }

        private void shareButton_Clicked(object sender, EventArgs e)
        {
            Java.IO.File file = new Java.IO.File(filepath);

            Android.Net.Uri contentUri = FileProvider.GetUriForFile(Android.App.Application.Context,
                                                       "org.lakeofburningfire.fileprovider", file);

            var sharingIntent = new Intent();

            sharingIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
            sharingIntent.AddFlags(ActivityFlags.NewTask);

            sharingIntent.SetAction(Intent.ActionSend);
            sharingIntent.SetType("image/*");
            sharingIntent.SetData(contentUri);
            sharingIntent.PutExtra(Intent.ExtraStream, contentUri);
            Android.App.Application.Context.StartActivity(sharingIntent);
        }

        private void padButton_Clicked(object sender, EventArgs e)
        {
            // Get data from form
            double widthRatio;
            try
            {
                widthRatio = double.Parse(widthEntry.Text);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("There was an error: {0}", Ex.Message);
                widthRatio = 1;
                widthEntry.Text = 1.ToString();
            }

            double heightRatio;
            try
            {
                heightRatio = double.Parse(heightEntry.Text);
            }
            catch (Exception Ex)
            {
                heightRatio = 1;
                heightEntry.Text = 1.ToString();
            }

            // Store possible target ratios
            int heightFromWidth = (int)Math.Ceiling(sharpImage.Width / widthRatio * heightRatio);
            int widthFromHeight = (int)Math.Ceiling(sharpImage.Height / heightRatio * widthRatio);

            // Find smallest change
            int heightDifference = heightFromWidth - sharpImage.Height;
            int widthDifference = widthFromHeight - sharpImage.Width;
            List<int> differences = new List<int>{ heightDifference, widthDifference };
            int minDifference = differences.Where(value => value > 0).Min();

            // Set up minimum width and height
            int minWidth = sharpImage.Width;
            int minHeight = sharpImage.Height;

            if (minDifference == heightDifference)
            {
                minHeight = heightFromWidth;
            }
            else if (minDifference == widthDifference)
            {
                minWidth = widthFromHeight;
            }

            //Pad it out to the smallest ratio
            var cloneImage = sharpImage.Clone(c => c.Resize(new ResizeOptions
            {
                Mode = ResizeMode.BoxPad,
                Position = AnchorPositionMode.Center,
                Size = new SixLabors.ImageSharp.Size(minWidth, minHeight)
            }
                ));

            filepath = buildFilePath(filepath);

            // We need to save a copy of the file so we can pass it on to the next app.
            cloneImage.Save(filepath);
            sharedImage.Source = ImageSource.FromFile(filepath);
        }
    }
}
