using Android.Graphics;
using ScatterView.Shared.Helper;
using ScatterView.Shared.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ScatterView.Shared.ViewModel
{
    /// <summary>
    /// Class to generate the model values for scatter views
    /// </summary>
    public class ViewGeneratorViewModel
    {
        #region Fields
        /// <summary>
        ///  Backfield for ScatterViews list
        /// </summary>
        private List<ScatterModel> scatterViews;
        #endregion

        #region Properties
        /// <summary>
        /// Property will holds the model value for Scatter views created
        /// </summary>
        public List<ScatterModel> ScatterViews
        {
            get { return scatterViews; }
            set { scatterViews = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the sactter views with the values either online or offline
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            scatterViews = new List<ScatterModel>();
            // For example loading 50 scatter models
            for (int i = 0; i < 50; i++)
                scatterViews.Add(await GetScatterView());
        }

        /// <summary>
        /// Method will return random color
        /// </summary>
        /// <returns></returns>
        private Android.Graphics.Color GetRandomColor()
        {
            var randomGen = new Random();
            KnownColor[] names = (KnownColor[])System.Enum.GetValues(typeof(KnownColor));
            KnownColor randomColorName = names[randomGen.Next(names.Length)];
            var randomColor = System.Drawing.Color.FromKnownColor(randomColorName);
            Android.Graphics.Color color = new Android.Graphics.Color() { R = randomColor.R, G = randomColor.G, B = randomColor.B, A = randomColor.A };
            return color;
        }

        /// <summary>
        /// Method will return the model value for satter view
        /// </summary>
        /// <returns></returns>
        private async Task<ScatterModel> GetScatterView()
        {
            var scatterModel = new ScatterModel();
            if (ConnectionHelper.IsNetworkConnected) // check for connection
            {
                // Based on the shape, loading the url
                var root = XmlResolveHelper.GetXmlElement(scatterModel.IsSquareShape ? 
                    "http://www.colourlovers.com/api/patterns/random" : "http://www.colourlovers.com/api/colors/random");
                if (root != null && root.GetElementsByTagName("imageUrl").Count != 0)
                {
                    scatterModel.Title = root.GetElementsByTagName("title")[0].InnerText;
                    scatterModel.Image = await DownloadImageAsync(root.GetElementsByTagName("imageUrl")[0].InnerText, scatterModel.Size, !scatterModel.IsSquareShape);
                }
            }
            else // loading offline shapes model with random colors
            {
                var randomColor = GetRandomColor();
                scatterModel.Image = scatterModel.IsSquareShape ? CreateBitmap(randomColor) : GetRoundedShape(CreateBitmap(randomColor));
                scatterModel.Title = randomColor.ToString();
            }

            return scatterModel;
        }

        /// <summary>
        /// Method will download the image Async
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="size"></param>
        /// <param name="canConvertCircleShape"></param>
        /// <returns></returns>
        private static async Task<Bitmap> DownloadImageAsync(string imageUrl, Size size, bool canConvertCircleShape)
        {
            Bitmap downloadedImage = null;
            using (WebClient webClient = new WebClient())
            {
                var url = new Uri(imageUrl);
                var bytes = await webClient.DownloadDataTaskAsync(url);
                string documentsPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string localFilename = "downloaded.png";
                string localPath = System.IO.Path.Combine(documentsPath, localFilename);

                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                await fs.WriteAsync(bytes, 0, bytes.Length);

                Console.WriteLine("localPath:" + localPath);
                fs.Close();

                BitmapFactory.Options options = new BitmapFactory.Options()
                {
                    InJustDecodeBounds = true
                };
                await BitmapFactory.DecodeFileAsync(localPath, options);

                options.InSampleSize = options.OutWidth > options.OutHeight ? options.OutHeight / size.Height : options.OutWidth / size.Width;
                options.InJustDecodeBounds = false;

                Bitmap bitmap = await BitmapFactory.DecodeFileAsync(localPath, options);

                downloadedImage = canConvertCircleShape ? GetRoundedShape(bitmap, size.Width, size.Height) : bitmap;
            }
            return downloadedImage;
        }
        
        /// <summary>
        /// Method will recreate the image in round shape
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        private static Bitmap GetRoundedShape(Bitmap bitmap, int targetWidth = 200, int targetHeight = 200)
        {
            Bitmap targetBitmap = Bitmap.CreateBitmap(targetWidth,
                targetHeight, Bitmap.Config.Argb8888);

            Canvas canvas = new Canvas(targetBitmap);
            Android.Graphics.Path path = new Android.Graphics.Path();
            path.AddCircle(((float)targetWidth - 1) / 2,
                ((float)targetHeight - 1) / 2,
                (Math.Min(targetWidth,
                    ((float)targetHeight)) / 2),
                Android.Graphics.Path.Direction.Ccw);

            canvas.ClipPath(path);
            Bitmap sourceBitmap = bitmap;
            canvas.DrawBitmap(sourceBitmap,
                new Rect(0, 0, sourceBitmap.Width,
                    sourceBitmap.Height),
                new Rect(0, 0, targetWidth, targetHeight), null);
            return targetBitmap;
        }

        /// <summary>
        /// Method will create new bitmap image with random color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        private static Bitmap CreateBitmap(Android.Graphics.Color color, int targetWidth = 200, int targetHeight = 200)
        {
            Bitmap bitmap = Bitmap.CreateBitmap(targetWidth, targetHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap);

            Paint paint = new Paint();
            paint.SetARGB(color.A, color.R, color.G, color.B);
            paint.SetStyle(Paint.Style.Fill);
            canvas.DrawPaint(paint);

            return bitmap;
        }
        #endregion
    }
}
