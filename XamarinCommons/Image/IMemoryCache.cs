using System;

namespace XamarinCommons.Image
{
	public interface IMemoryCache
	{
		ImageWrapper Get(Uri uri);

		void Put(Uri uri, ImageWrapper image);
	}
}