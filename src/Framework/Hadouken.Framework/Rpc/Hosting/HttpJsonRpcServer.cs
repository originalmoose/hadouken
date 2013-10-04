﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hadouken.Framework.Rpc.Hosting
{
    public class HttpJsonRpcServer : IJsonRpcServer
    {
        private readonly IJsonRpcHandler _rpcHandler;
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly Task _workerTask;

        public HttpJsonRpcServer(string listenUri, IJsonRpcHandler rpcHandler)
        {
            _rpcHandler = rpcHandler;
            _httpListener.Prefixes.Add(listenUri);
            _workerTask = new Task(ct => Run(_cancellationToken.Token), _cancellationToken.Token);
        }

        public void Open()
        {
            _workerTask.Start();
        }

        public void Close()
        {
            _cancellationToken.Cancel();
            _workerTask.Wait();
        }

        private async void Run(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _httpListener.Start();

            while (!_cancellationToken.IsCancellationRequested)
            {
                var context = await _httpListener.GetContextAsync();

                if (cancellationToken.IsCancellationRequested)
                    break;

                ProcessContext(context);
            }

            _httpListener.Close();
        }

        private async void ProcessContext(HttpListenerContext context)
        {
            using (var reader = new StreamReader(context.Request.InputStream))
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                var content = reader.ReadToEnd();

                try
                {
                    var response = await _rpcHandler.HandleAsync(content);

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 200;

                    writer.Write(response);
                }
                catch (Exception e)
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.StatusCode = 500;

                    writer.Write(e.ToString());
                }
            }

            context.Response.OutputStream.Close();
            context.Response.Close();
        }
    }
}