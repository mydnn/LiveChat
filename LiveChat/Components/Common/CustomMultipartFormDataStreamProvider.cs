using DotNetNuke.Entities.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace MyDnn.Modules.Support.LiveChat.Components.Common
{
    public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public CustomMultipartFormDataStreamProvider(string path) : base(path) { }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (headers.ContentDisposition.FileName == null) return base.GetStream(parent, headers);

            var filename = headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            var fileExtension = filename.Substring(filename.LastIndexOf("."));

            return Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(fileExtension) ? base.GetStream(parent, headers) : Stream.Null;
        }

        public override string GetLocalFileName(System.Net.Http.Headers.HttpContentHeaders headers)
        {
            return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
        }
    }

}