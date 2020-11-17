#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	internal static class DisposeHelpers
	{
		internal static void DisposeModalAndChildRenderers(this Element view)
		{
			IVisualElementRenderer renderer;
			foreach (Element child in view.Descendants())
			{
				if (child is VisualElement ve)
				{
					renderer = Platform.GetRenderer(ve);
					child.ClearValue(Platform.RendererProperty);

					if (renderer != null)
					{
						renderer.NativeView.RemoveFromSuperview();
						renderer.Dispose();
					}
				}
			}

			if (view is VisualElement visualElement)
			{
				renderer = Platform.GetRenderer(visualElement);
				if (renderer != null)
				{
#if __MOBILE__
					if (renderer.ViewController != null)
					{
						if (renderer.ViewController.ParentViewController is ModalWrapper modalWrapper)
							modalWrapper.Dispose();
					}
#endif
					renderer.NativeView.RemoveFromSuperview();
					renderer.Dispose();
				}
				view.ClearValue(Platform.RendererProperty);
#if __MOBILE__

				//reset layout size so we can re-layout the layer if we are reusing the same page.
				//fixes a issue on split apps on iPad and iOS13
				if (Forms.IsiOS13OrNewer && visualElement is Page)
					visualElement.Layout(new Rectangle(0, 0, -1, -1));
#endif
			}
		}

		internal static void DisposeRendererAndChildren(this IVisualElementRenderer rendererToRemove)
		{
			if (rendererToRemove == null)
				return;

			if (rendererToRemove.Element != null && Platform.GetRenderer(rendererToRemove.Element) == rendererToRemove)
				rendererToRemove.Element.ClearValue(Platform.RendererProperty);

			if (rendererToRemove.NativeView != null)
			{
				var subviews = rendererToRemove.NativeView.Subviews;
				for (var i = 0; i < subviews.Length; i++)
				{
					if (subviews[i] is IVisualElementRenderer childRenderer)
						DisposeRendererAndChildren(childRenderer);
				}
				rendererToRemove.NativeView.RemoveFromSuperview();
			}
			rendererToRemove.Dispose();
		}
	}
}