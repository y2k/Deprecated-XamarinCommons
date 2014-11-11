using System;
using System.IO;
using System.Threading.Tasks;

namespace XamarinCommons.Image
{
	public interface IDiskCache
	{
		Task<ImageWrapper> GetAsync (Uri uri);

		Task PutAsync (Uri uri, Stream image);

		IImageDecoder Decoder { get; set; }
	}
}