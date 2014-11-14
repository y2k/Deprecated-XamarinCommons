using PCLStorage;
using System;
using System.IO;

namespace XamarinCommons.Image
{
	public abstract class ImageDecoder
	{
		public virtual object Decode (IFile file)
		{
			using (var stream = file.OpenAsync (FileAccess.Read).Result) {
				return DecoderStream (stream);
			}
		}

		public abstract int GetImageSize (object commonImage);

		public virtual object DecoderStream (Stream stream)
		{
			throw new NotImplementedException ();
		}
	}
}