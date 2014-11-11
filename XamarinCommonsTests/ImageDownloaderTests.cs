using System;
using NUnit.Framework;
using PCLStorage;
using XamarinCommons.Image;

namespace XamarinCommonsTests
{
	[TestFixture]
	public class ImageDownloaderTests
	{
		[Test]
		public async void Test ()
		{
			var module = new ImageDownloader {
				MemoryCache = new DefaultMemoryCache { Decoder = new MockDecoder () },
				DiskCache = new DefaultDiskCache { Decoder = new MockDecoder () },
			};

			var r = await module.Load ("test", new Uri ("https://upload.wikimedia.org/wikipedia/en/b/bc/Wiki.png"));
			Assert.AreNotSame (ImageWrapper.Invalide, r);
			Assert.NotNull (r.Image);
		}

		[Test]
		public void TestSimpleTasks ()
		{
			var module = new ImageDownloader {
				MemoryCache = new DefaultMemoryCache { Decoder = new MockDecoder () },
				DiskCache = new DefaultDiskCache { Decoder = new MockDecoder () },
			};

			var t = module.Load ("test", new Uri ("https://upload.wikimedia.org/wikipedia/en/b/bc/Wiki.png"));
			var r = t.Result;
			Assert.AreNotSame (ImageWrapper.Invalide, r);
			Assert.NotNull (r.Image);
		}

		public class MockDecoder : IImageDecoder
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