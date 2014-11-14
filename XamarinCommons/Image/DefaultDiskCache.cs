using System;
using System.IO;
using System.Threading.Tasks;
using PCLStorage;

namespace XamarinCommons.Image
{
	public class DefaultDiskCache : IDiskCache
	{
		IFolder root;

		public DefaultDiskCache ()
		{
			root = FileSystem.Current.LocalStorage;
		}

		public async Task PurgeAsync ()
		{
			var list = await root.GetFilesAsync ();
			foreach (var file in list) {
				await file.DeleteAsync ();
			}
		}

		#region IDiskCache implementation

		public ImageDecoder Decoder { get; set; }

		public async Task<object> GetAsync (Uri uri)
		{
			var name = ConvertUriToFilename (uri);
			if (await root.CheckExistsAsync (name) == ExistenceCheckResult.NotFound)
				return null;
			var f = await root.GetFileAsync (name);
			var image = await Task.Run (() => Decoder.Decode (f));
			return image ;
		}

		public async Task PutAsync (Uri uri, Stream image)
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