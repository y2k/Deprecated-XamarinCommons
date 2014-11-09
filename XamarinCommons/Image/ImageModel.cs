using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace XamarinCommons.Image
{
	public class ImageModel
	{
		const int MaxAttempts = 5;
		const int BaseAttemptDelay = 500;

		public IMemoryCache MemoryCache { get; set; }

		public IDiskCache DiskCache  { get; set; }

		HttpClient webClient = new HttpClient ();
		Dictionary<object, Uri> lockedImages = new Dictionary<object, Uri> ();

		public async void Load (object token, Uri originalUri, int maxWidth, Action<ImageWrapper> originalImageCallback)
		{
			if (originalUri == null) {
				originalImageCallback (new ImageWrapper ());
				lockedImages.Remove (token);
				return;
			}

			lockedImages [token] = originalUri;
			Action<ImageWrapper> imageCallback = image => {
				if (lockedImages.Any (s => s.Key == token && s.Value == originalUri)) {
					originalImageCallback (image);
					lockedImages.Remove (token);
				}
			};

			var uri = CreateThumbnailUrl (originalUri, maxWidth);

			// Поиск картинки в кэше памяти
			var mi = MemoryCache.Get (uri);
			if (mi != null) {
				imageCallback (mi);
				return;
			}

#if ACCESS_TO_DISK_IN_MAIN
			// Поиск картинки в кэше на диске
			if (Math.Abs(1) == 0) { // FIXME
				// Запрос к диску в главном потоке 
				var i = diskCachge.Get (uri);
				if (i != null) {
					memoryCache.Put (uri, i);
					imageCallback(i);
					return;
				}
			} else {
#endif
			// Запрос к диску в фоновом потоке
			var i = await DiskCache.GetAsync (uri);
			if (i != null) {
				MemoryCache.Put (uri, i);
				imageCallback (i);
				return;
			}
#if ACCESS_TO_DISK_IN_MAIN
			}
#endif

			DownloadImage (0, uri, s => imageCallback (s));
		}

		public string CreateThumbnailUrl (string url, int px)
		{
			return "" + CreateThumbnailUrl (new Uri (url), px);
		}

		#region Private methods

		private async void DownloadImage (int index, Uri uri, Action<ImageWrapper> callback)
		{
			if (index > 0)
				await Task.Delay (BaseAttemptDelay << index);

			ImageWrapper mi = null;
			await Task.Run (async () => {
				try {
					using (var ins = await webClient.GetStreamAsync (uri)) {
						DiskCache.Put (uri, ins);
						mi = await DiskCache.GetAsync (uri);
						MemoryCache.Put (uri, mi);
					}
				} catch (HttpRequestException) {
					// Ignore exception
				} catch (Exception e) {
					throw new Exception ("URL = " + uri, e);
				}
			});

			if (mi == null && ++index < MaxAttempts)
				DownloadImage (index, uri, callback);
			else
				callback (mi);
		}

		private Uri CreateThumbnailUrl (Uri url, int px)
		{
			if (px == 0)
				return url;

			var s = string.Format (
				        "http://remote-cache.api-i-twister.net/Cache/Get?maxHeight=500&width={0}&url={1}",
				        px, Uri.EscapeDataString ("" + url));
			return new Uri (s);
		}

		#endregion
	}
}