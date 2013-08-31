﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Framework.Rpc
{
    public abstract class Transport : ITransport
    {
        private readonly object _callbackLock = new object();
        private Action<IRequest> _callback;

        public void SetRequestCallback(Action<IRequest> callback)
        {
            lock (_callbackLock)
                _callback = callback;
        }

        protected virtual void OnRequest(IRequest request)
        {
            lock (_callbackLock)
            {
                _callback(request);
            }
        }

        public abstract bool SupportsScheme(string scheme);

        public abstract void Open();

        public abstract void Close();
    }
}