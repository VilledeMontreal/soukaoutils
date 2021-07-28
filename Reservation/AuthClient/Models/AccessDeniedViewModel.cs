using System;
using System.Net.Http;

namespace AuthClient.Models
{
    public class AccessDeniedViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string APIResourceUrl { get; set; }

        public HttpRequestException Exception { get; set; }
    }
}
