using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

using WebApplication;

namespace WebApplication.Controllers
{
    public class AttachmentController : ApiController
    {
        private readonly AttachmentService _service = new AttachmentService();

        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var memoryProvider = new MultipartMemoryStreamProvider();
            await request.Content.ReadAsMultipartAsync(memoryProvider);

            var result = await _service.UploadInstanceAttachmentAsync(memoryProvider);

            return Ok(result);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> DownloadAttachment(int attachmentId)
        {
            var attachment = await _service.DownloadAttachment(attachmentId);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(attachment.FileStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = attachment.FileName;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentLength = attachment.FileStream.Length;

            return response;
        }
    }
}
