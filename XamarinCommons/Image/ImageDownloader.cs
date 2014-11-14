using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace XamarinCommons.Image
{
	public class ImageDownloader
	{
		public static readonly object InvalideImage = new object ();

		const int MaxAttempts = 5;
		const int BaseAttemptDelay = 500;

		public IMemoryCache MemoryCache { get; set; }

		public IDiskCache DiskCache  { get; set; }

		public ImageDecoder Decoder { get; set; }

		HttpClient webClient = new HttpClient ();
		IDictionary<object, Uri> lockedImages = new Dictionary<object, Uri> ();

		public async Task<object> LoadAsync (object token, Uri imageUri)
		{
			LazyInitalize ();

			if (imageUri == null) {
				lockedImages.Remove (token);
				return InvalideImage;
			}

			var image = MemoryCache.Get (imageUri);
			if (image != null)
				return image;

			lockedImages [token] = imageUri;
			image = await DiskCache.GetAsync (imageUri);
			if (image != null) {
				MemoryCache.Put (imageUri, image);
				if (lockedImages.Any (s => s.Key == token && s.Value == imageUri)) {
					lockedImages.Remove (token);
					return image;
				}
				return InvalideImage;
			}

			image = await DownloadImage (imageUri);
			if (lockedImages.Any (s => s.Key == token && s.Value == imageUri)) {
				lockedImages.Remove (token);
				return image;
			}
			return InvalideImage;
		}

		void LazyInitalize ()
		{
			MemoryCache.Decoder = Decoder;
			DiskCache.Decoder = Decoder;
		}

		async Task<object> DownloadImage (Uri uri, int index = 0)
		{
			try {
				using (var ins = await webClient.GetStreamAsync (uri)) {
					await DiskCache.PutAsync (uri, ins);
					var image = await DiskCache.GetAsync (uri);
					MemoryCache.Put (uri, image);
					return image;
				}
			} catch (HttpRequestException) {
				// Ignore exception
			} catch (Exception e) {
				throw new Exception ("URL = " + uri, e);
			}

			if (index >= MaxAttempts)
				return null;

			await Task.Delay (BaseAttemptDelay << index);
			return await DownloadImage (uri, index + 1);
		}
	}
}