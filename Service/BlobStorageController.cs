// using System;
// using System.ComponentModel.DataAnnotations;
// using System.Linq;
// using System.Threading.Tasks;
// using Azure;
// using Azure.Storage.Blobs;
// using Azure.Storage.Blobs.Models;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Zukte.Models;
// using Zukte.Responses;
// using Zukte.Utilities;

// namespace Zukte.Service {
// 	[Route("api/[controller]")]
// 	[ApiController]
// 	public class BlobStorageController : ControllerBase {
// 		private readonly BlobServiceClient _blobServiceClient;

// 		public BlobStorageController(BlobServiceClient blobServiceClient) {
// 			_blobServiceClient = blobServiceClient;
// 		}

// 		private async Task<BlobContainerClient> GetImageShareContainer() {
// 			BlobContainerClient container = _blobServiceClient.GetBlobContainerClient("image-share");
// 			_ = await container.CreateIfNotExistsAsync();
// 			return container;
// 		}

// 		// GET: api/ImageShare
// 		[HttpGet("List")]
// 		public async Task<ActionResult<ListResponse<string>>> ListImages([Required, FromQuery] ListRequest request) {
// 			BlobContainerClient container = await GetImageShareContainer();

// 			int? pageSizeHint = (int?)request.MaxResults;

// 			Page<BlobItem> page = container.GetBlobs()
// 			  .AsPages(request.PageToken, pageSizeHint)
// 			  .First();

// 			string[] items = page.Values
// 			  .Select(e => container.Uri.AbsoluteUri + '/' + e.Name)
// 			  .ToArray();

// 			ListResponse<string> response = new ListResponse<string> {
// 				Items = items,
// 				NextPageToken = page.ContinuationToken ?? string.Empty,
// 			};

// 			return response;
// 		}

// 		// POST: api/ImageShare
// 		[HttpPost]
// 		// public async Task<ActionResult<string>> UploadImage([Required, OpenApiEncodingContentType("image/*")] IFormFile file) {
// 		public async Task<ActionResult<string>> UploadImage([Required] IFormFile file) {
// 			BlobContainerClient containerClient = await GetImageShareContainer();

// 			if (containerClient == null) {
// 				throw new ArgumentNullException();
// 			}

// 			Uri uri = await file.ProcessImageFile(ModelState, string.Empty, containerClient);

// 			if (!ModelState.IsValid) {
// 				return BadRequest(ModelState);
// 			}

// 			string absoluteUri = uri == null ? string.Empty : uri.AbsoluteUri;
// 			return new ActionResult<string>(absoluteUri);
// 		}
// 	}
// }
