using PCLStorage;

namespace XamarinCommons.Image
{
	public interface IImageDecoder
	{
		object Decode(IFile file);

		int GetImageSize(ImageWrapper commonImage);
	}
}