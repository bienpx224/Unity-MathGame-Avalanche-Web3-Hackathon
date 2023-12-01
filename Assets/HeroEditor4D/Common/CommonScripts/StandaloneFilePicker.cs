using System;
using System.Collections;

namespace Assets.HeroEditor4D.Common.CommonScripts
{
	public static class StandaloneFilePicker
	{
		#if UNITY_EDITOR

		public static IEnumerator OpenFile(string title, string directory, string extension, Action<bool, string, byte[]> callback)
		{
			var path = UnityEditor.EditorUtility.OpenFilePanel(title, directory, extension);

			if (!string.IsNullOrEmpty(path))
			{
				var bytes = System.IO.File.ReadAllBytes(path);

				callback(true, path, bytes);
			}
			else
			{
				callback(false, null, null);
			}

			yield break;
		}

		public static IEnumerator SaveFile(string title, string directory, string defaultName, string extension, byte[] bytes, Action<bool, string> callback)
		{
			var path = UnityEditor.EditorUtility.SaveFilePanel(title, directory, defaultName, extension);

			if (!string.IsNullOrEmpty(path))
			{
				System.IO.File.WriteAllBytes(path, bytes);
				callback(true, path);
			}
			else
			{
				callback(false, null);
			}

			yield break;
		}
	
		#elif UNITY_WSA

		public static IEnumerator OpenFile(string title, string directory, string extension, Action<bool, string, byte[]> callback)
		{
			bool opened = false;
			string path = null;
			byte[] bytes = null;
			bool pickerClosed = false;

			UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
			{
				var filePicker = new Windows.Storage.Pickers.FileOpenPicker
				{
					SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
				}; // If you have an error on this line, just switch Unity to Android and then switch back to UWP. This helped me for Unity 2018.1.

				foreach (var fileType in extension.Split(','))
				{
					filePicker.FileTypeFilter.Add('.' + fileType);
				}

				var file = await filePicker.PickSingleFileAsync();

				if (file != null)
				{
					path = file.Path;

					var buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);

					bytes = new byte[buffer.Length];

					using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
					{
						dataReader.ReadBytes(bytes);
					}

					opened = true;
				}

				pickerClosed = true;
			}, false);

			while (!pickerClosed)
			{
				yield return null;
			}

			callback(opened, path, bytes);
		}

		public static IEnumerator SaveFile(string title, string directory, string defaultName, string extension, byte[] bytes, Action<bool, string> callback)
		{
			var saved = false;
			string path = null;
			var pickerClosed = false;

			UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
			{
				var savePicker = new Windows.Storage.Pickers.FileSavePicker(); // If you have an error on this line, just switch Unity to Android and then switch back to UWP. This helped me for Unity 2018.1.

				savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
				savePicker.SuggestedFileName = defaultName;
				savePicker.FileTypeChoices.Add("Image", new System.Collections.Generic.List<string>() { '.' + extension });

				var file = await savePicker.PickSaveFileAsync();

				if (file != null)
				{
					path = file.Path;
					Windows.Storage.CachedFileManager.DeferUpdates(file);

					var buffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(bytes);

					await Windows.Storage.FileIO.WriteBufferAsync(file, buffer);

					var status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

					saved = status == Windows.Storage.Provider.FileUpdateStatus.Complete;
				}

				pickerClosed = true;
			}, false);

			while (!pickerClosed)
			{
				yield return null;
			}

			callback(saved, path);
		}

		#elif UNITY_STANDALONE

		public static IEnumerator OpenFile(string title, string directory, string extension, Action<bool, string, byte[]> callback)
		{
			var extensions = new[] { new SFB.ExtensionFilter("Image Files", extension.Split(',')) };

            // Import StandaloneFileBrowser asset for standalone builds!
			SFB.StandaloneFileBrowser.OpenFilePanelAsync(title, directory, extensions, false, paths =>
			{
				if (paths != null && paths.Length == 1)
				{
					callback(true, paths[0], System.IO.File.ReadAllBytes(paths[0]));
				}
			});

			yield break;
		}

		public static IEnumerator SaveFile(string title, string directory, string defaultName, string extension, byte[] bytes, Action<bool, string> callback)
		{
            // Import StandaloneFileBrowser asset for standalone builds!
			SFB.StandaloneFileBrowser.SaveFilePanelAsync(title, directory, defaultName, extension, path =>
			{
				if (string.IsNullOrEmpty(path))
				{
					callback(false, path);
				}
				else
				{
					System.IO.File.WriteAllBytes(path, bytes);
					callback(true, path);
				}
			});

			yield break;
		}

		#else

		public static IEnumerator OpenFile(string title, string directory, string extension, Action<bool, string, byte[]> callback)
		{
			throw new NotSupportedException();
		}

		public static IEnumerator SaveFile(string title, string directory, string defaultName, string extension, byte[] bytes, Action<bool, string> callback)
		{
			throw new NotSupportedException();
		}

		#endif
	}
}