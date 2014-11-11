using System;
using System.IO;
using System.Threading.Tasks;
using PCLStorage;

namespace XamarinCommons.Image
{
	public class DefaultDiskCache : IDiskCache
	{
		public IImageDecoder Decoder { get; set; }

		IFolder root;

		public DefaultDiskCache ()
		{
			root = FileSystem.Current.LocalStorage;
		}

		#region IDiskCache implementation

		public async Task<ImageWrapper> GetAsync (Uri uri)
		{
			var name = ConvertUriToFilename (uri);
			if (await root.CheckExistsAsync (name) == ExistenceCheckResult.NotFound)
				return null;
			var f = await root.GetFileAsync (name);
			var image = await Task.Run (() => Decoder.Decode (f));
			return new ImageWrapper { Image = image };
		}

		public ImageWrapper Get (Uri uri)
		{
			var name = ConvertUriToFilename (uri);
			if (root.CheckExistsAsync (name).Result == ExistenceCheckResult.NotFound)
				return null;
			var f = root.GetFileAsync (name).Result;

			//using (var stream = f.OpenAsync (FileAccess.Read).Result) {
			//    var i = decoder.Decode (f.Path, stream);
			//    return new ImageWrapper { Image = i };
			//}
			return new ImageWrapper { Image = Decoder.Decode (f) };
		}

		public async Task Put (Uri uri, Stream image)
		{
			var file = ConvertUriToFilename (uri);
			if (await root.CheckExistsAsync (file) == ExistenceCheckResult.FileExists)
				return;

			var tn = Guid.NewGuid () + ".tmp";
			var tmp = await root.CreateFileAsync (tn, CreationCollisionOption.ReplaceExisting);
			using (var outs = await tmp.OpenAsync (FileAccess.ReadAndWrite)) {
				await Task.Run (() => {
					var buf = new byte[4 * 1024];
					int count;

					while ((count = image.Read (buf, 0, buf.Length)) > 0)
						outs.Write (buf, 0, count);
				});
			}

			try {
				await tmp.RenameAsync (file);
			} catch {
				await tmp.DeleteAsync ();
			}
		}

		#endregion

		string ConvertUriToFilename (Uri uri)
		{
			return uri.GetHashCode () + ".bin";
		}
	}
}