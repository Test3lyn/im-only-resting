﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net;
using Swensen.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Swensen.Ior.Core {
    /// <summary>
    /// A light wrapper around our underlying http client.
    /// </summary>
    public class IorClient {
        private readonly string defaultRequestContentType;
        private readonly string proxyServer;

        public IorClient(string defaultRequestContentType, string proxyServer) {
            this.defaultRequestContentType = defaultRequestContentType;
            this.proxyServer = proxyServer;
        }

        public CancellationTokenSource ExecuteAsync(RequestModel requestModel, Action<ResponseModel> callback) {
            //todo: add using statements for disposible objects

            var handler = new HttpClientHandler();
            if (!proxyServer.IsBlank()) {
                handler.Proxy = new WebProxy(proxyServer, false); //make second arg a config option.
                handler.UseProxy = true;
            }

            var client = new HttpClient(handler);
            var request = new HttpRequestMessage();

            request.RequestUri = requestModel.Url;
            request.Method = requestModel.Method;

            foreach (var header in requestModel.Headers)
                request.Headers.Add(header.Key, header.Value);

            var content = new StringContent(requestModel.Body);
            content.Headers.Add("Content-Type", requestModel.GetContentType(defaultRequestContentType));

            var start = DateTime.Now;

            var ctokenSource = new CancellationTokenSource();
            var ctoken = ctokenSource.Token;
            
            client.SendAsync(request,ctoken).ContinueWith(responseTask => {
                var response = responseTask.Result;
                var end = DateTime.Now;
                var responseModel = new ResponseModel(response, start, end);
                callback(responseModel);
            });

            return ctokenSource;
        }

    }
}
