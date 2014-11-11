using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace XamarinCommons.Image
{
	public class ImageDownloader
	{
		public static readonly ImageWrapper Invalid = new ImageWrapper ();

		const int MaxAttempts = 5;
		const int BaseAttemptDelay = 500;

		public IMemoryCache MemoryCache { get; set; }

		public IDiskCache DiskCache  { get; set; }

		HttpClient webClient = new HttpClient ();
		IDictionary<object, Uri> lockedImages = new Dictionary<object, Uri> ();

		public async Task<ImageWrapper> Load (object token, Uri imageUri)
		{
			if (imageUri == null) {
				lockedImages.Remove (token);
				return new ImageWrapper ();
			}

			var image = MemoryCache.Get (imageUri);
			if (image != null)
				return image;

			lockedImages [token] = imageUri;
			image = await DiskCache.GetAsync (imageUri);
			if (image != null) {
				if (lockedImages.Any (s => s.Key == token && s.Value == imageUri)) {
					lockedImages.Remove (token);
					return image;
				}
				return ImageWrapper.Invalide;
			}

			image = await DownloadImage (imageUri);
			if (lockedImages.Any (s => s.Key == token && s.Value == imageUri)) {
				lockedImages.Remove (token);
				return image;
			}
			return ImageWrapper.Invalide;
		}

		async Task<ImageWrapper> DownloadImage (Uri uri, int index = 0)
		{
			try {
				using (var ins = await webClient.GetStreamAsync (uri)) {
					await DiskCache.Put (uri, ins);
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
				return new ImageWrapper ();

			await Task.Delay (BaseAttemptDelay << index);
			return await DownloadImage (uri, index + 1);
		}
	}
}