using System;

namespace XamarinCommons.Image
{
	public interface IMemoryCache
	{
		object Get (Uri uri);

		void Put (Uri uri, object image);

		ImageDecoder Decoder { get; set; }
	}
}