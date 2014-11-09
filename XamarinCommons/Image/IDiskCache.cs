using System;
using System.IO;
using System.Threading.Tasks;

namespace XamarinCommons.Image
{
	public interface IDiskCache
	{
		[Obsolete]
		ImageWrapper Get (Uri uri);

		Task<ImageWrapper> GetAsync (Uri uri);

		void Put (Uri uri, Stream image);
	}
}