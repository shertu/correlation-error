using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SkiaSharp;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Zukte.Utilities {
	public static class IFormFileExtensions {
		public const uint MAX_FILE_SIZE = 8388608; //bytes

		public static async Task<Uri?> ProcessImageFile(this IFormFile file, ModelStateDictionary modelState, string directory, BlobContainerClient container) {
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (modelState == null)
				throw new ArgumentNullException(nameof(modelState));
			if (directory == null)
				throw new ArgumentNullException(nameof(directory));
			if (container is null)
				throw new ArgumentNullException(nameof(container));

			/* HtmlEncode the result in case it must be returned in an error message.
				   */

			string encodedFileName = WebUtility.HtmlEncode(file.FileName);

			/* Check the file length. This check doesn't catch files 
				   * that only contain byte encoding as their content.
				   */
			if (file.Length > MAX_FILE_SIZE) {
				modelState.AddModelError(file.Name,
				  $"The file ({encodedFileName}) exceeds 8 MiB.");
			} else {
				using Stream fs = file.OpenReadStream();
				using MemoryStream ms = new MemoryStream((int)fs.Length);
				await fs.CopyToAsync(ms);

				bool isEmptyImage = IsEmptyImage(ms);

				if (isEmptyImage) {
					modelState.AddModelError(file.Name,
					  $"The file ({encodedFileName}) is an invalid image file.");
				} else {
					string replacementFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string path = Path.Combine(directory, replacementFileName);

					try {
						return await UploadFileToBlobStorageContainer(ms, path, container);
					} catch (Exception ex) {
						modelState.AddModelError(file.Name,
						  $"The file ({encodedFileName}) upload failed with error: {ex.Message}");
					}
				}
			}

			return default;
		}

		/// <summary>
		/// Checks if a stream is actually an image which is empty.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private static bool IsEmptyImage(Stream stream) {
			_ = stream.Seek(0, SeekOrigin.Begin);
			SKData data = SKData.Create(stream);
			return SKBitmap.DecodeBounds(data).IsEmpty;
		}

		/// <summary>
		/// Uploads a stream to an Azure blob storage container.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="name"></param>
		/// <param name="container"></param>
		/// <returns></returns>
		public static async Task<Uri> UploadFileToBlobStorageContainer(Stream stream, string name, BlobContainerClient container) {
			// Move to the start of the stream
			_ = stream.Seek(0, SeekOrigin.Begin);

			// Get the reference to the block blob from the container
			BlobClient bc = container.GetBlobClient(name);
			_ = await bc.UploadAsync(stream);

			// Return the uri of the blob
			return bc.Uri;
		}
	}
}
