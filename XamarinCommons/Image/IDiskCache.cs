using System;
using System.IO;
using System.Threading.Tasks;

namespace XamarinCommons.Image
{
	public interface IDiskCache
	{
		Task<object> GetAsync (Uri uri);

		Task PutAsync (Uri uri, Stream image);

		ImageDecoder Decoder { get; set; }
	}
}