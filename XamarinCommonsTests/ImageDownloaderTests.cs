using System;
using NUnit.Framework;
using PCLStorage;
using XamarinCommons.Image;

namespace XamarinCommonsTests
{
	[TestFixture]
	public class ImageDownloaderTests
	{
		ImageDownloader module;

		[SetUp]
		public void SetUp ()
		{
			SetupNewImageDownloader ();
		}

		[Test]
		public async void TestLoad ()
		{
			var r = await module.LoadAsync (new object (), new Uri ("https://upload.wikimedia.org/wikipedia/en/b/bc/Wiki.png"));
			Assert.AreNotSame (ImageWrapper.Invalide, r);
			Assert.NotNull (r.Image);
		}

		[Test]
		public async void TestTwoThread ()
		{
			var token = new object ();
			var t1 = module.LoadAsync (token, new Uri ("https://upload.wikimedia.org/wikipedia/en/b/bc/Wiki.png"));
			var t2 = module.LoadAsync (token, new Uri ("https://upload.wikimedia.org/wikipedia/ru/b/bc/Wiki.png"));

			var r1 = await t1;
			var r2 = await t2;

			Assert.AreSame (ImageWrapper.Invalide, r1);
			Assert.AreNotSame (ImageWrapper.Invalide, r2);
			Assert.NotNull (r2.Image);
		}

		[Test]
		public async void TestMemoryCache ()
		{
			await module.LoadAsync (new object (), new Uri ("https://upload.wikimedia.org/wikipedia/en/b/bc/Wiki.png"));
			await ((DefaultDiskCache)module.DiskCache).PurgeAsync ();

			var now = DateTime.Now;
			var r = await module.LoadAsync (new object (), new Uri ("https://upload.wikimedia.org/wikipedia/en/b/bc/Wiki.png"));
			var duration = DateTime.Now - now;

			Assert.AreNotSame (ImageWrapper.Invalide, r);
			Assert.NotNull (r.Image);
			Assert.IsTrue (duration.TotalMilliseconds < 1, "Duration = " + duration);
		}

		[Test]
		public async void TestDiskCache ()
		{
			await module.LoadAsync (new object (), new Uri ("https://upload.wikimedia.org/wikipedia/en/b/bc/Wiki.png"));
			((DefaultMemoryCache)module.MemoryCache).Purge ();

			var now = DateTime.Now;
			var r = await module.LoadAsync (new object (), new Uri ("https://upload.wikimedia.org/wikipedia/en/b/bc/Wiki.png"));
			var duration = DateTime.Now - now;

			Assert.AreNotSame (ImageWrapper.Invalide, r);
			Assert.NotNull (r.Image);
			Assert.IsTrue (duration.TotalMilliseconds < 1, "Duration = " + duration);
		}

		void SetupNewImageDownloader ()
		{
			var diskCache = new DefaultDiskCache ();
			diskCache.PurgeAsync ().Wait ();
			module = new ImageDownloader {
				Decoder = new MockDecoder (),
				MemoryCache = new DefaultMemoryCache (),
				DiskCache = diskCache,
			};
		}

		public class MockDecoder : ImageDecoder
		{
			public object Decode (IFile file)
			{
				using (var stream = file.OpenAsync (FileAccess.Read).Result)
					return (int)stream.Length;
			}

			public int GetImageSize (ImageWrapper commonImage)
			{
				return (int)commonImage.Image;
			}
		}
	}
}