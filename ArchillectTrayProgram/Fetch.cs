using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ArchillectTrayProgram
{
    class Fetch
    {
        public const string UriArchillect = "http://archillect.com/";
        public static string ImgTempPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "ArchillectWallpaper\\tmp.jpg");

        public const string ContainerElement = "<div id=\"container\">";
        public const string ImgElement = "<img id=\"ii\" src=";

        public const int SPI_SETDESKWALLPAPER = 20;
        public const int SPIF_UPDATEINIFILE = 1;
        public const int SPIF_SENDCHANGE = 2;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private static string GetUriArchillectImgagePage(string body)
        {
            if (body.Length == 0) return "";
            int firstAElementIndex = body.IndexOf("<a ", body.IndexOf(ContainerElement, StringComparison.Ordinal),
                StringComparison.Ordinal);
            int hrefIndex = body.IndexOf("/", firstAElementIndex, StringComparison.Ordinal) + 1;
            int closingHrefIndex = body.IndexOf(">", hrefIndex, StringComparison.Ordinal);
            if (hrefIndex == -1 || closingHrefIndex == -1) return "";
            return UriArchillect + body.Substring(hrefIndex, closingHrefIndex - hrefIndex);
        }

        private static string GetImgFullPath(string body)
        {
            if (body.Length == 0) return "";
            int srcIndex = body.IndexOf(ImgElement, StringComparison.Ordinal) + ImgElement.Length;
            int srcEndIndex = body.IndexOf("/>", srcIndex, StringComparison.Ordinal);
            if (srcIndex == -1 || srcEndIndex == -1) return "";
            return body.Substring(srcIndex, srcEndIndex - srcIndex);
        }

        private static string DownloadFile(string remoteUri)
        {
            WebClient client = new WebClient();

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            string content;
            try
            {
                content = client.DownloadString(remoteUri);
            }
            catch (Exception)
            {
                return "";
            }
            return content;
        }

        private static bool DownloadRemoteImageFile(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                 response.StatusCode == HttpStatusCode.Moved ||
                 response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                // if the remote file was found, download it
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
                return true;
            }
            return false;
        }

        private static int changeWallpaper(string fullPath, Style style)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Fill)
            {
                key.SetValue(@"WallpaperStyle", 10.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Fit)
            {
                key.SetValue(@"WallpaperStyle", 6.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Span) // Windows 8 or newer only!
            {
                key.SetValue(@"WallpaperStyle", 22.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Stretch)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Tile)
            {
                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }
            if (style == Style.Center)
            {
                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            return SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, fullPath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        public enum Style : int
        {
            Fill,
            Fit,
            Span,
            Stretch,
            Tile,
            Center
        }

        public static void TaskToDo()
        {
            string imgUriPath = GetImgFullPath(DownloadFile(GetUriArchillectImgagePage(DownloadFile(UriArchillect))));
            DownloadRemoteImageFile(imgUriPath, ImgTempPath);
            changeWallpaper(ImgTempPath, Style.Center);
        }

        public static void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            TaskToDo();
        }
    }
}
