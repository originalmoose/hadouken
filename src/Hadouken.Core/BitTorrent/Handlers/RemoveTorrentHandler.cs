﻿using System;
using Hadouken.Common.BitTorrent;
using Hadouken.Common.Messaging;
using Ragnar;

namespace Hadouken.Core.BitTorrent.Handlers
{
    internal class RemoveTorrentHandler : IMessageHandler<RemoveTorrentMessage>
    {
        private readonly ISession _session;

        public RemoveTorrentHandler(ISession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            _session = session;
        }

        public void Handle(RemoveTorrentMessage message)
        {
            using (var handle = _session.FindTorrent(message.InfoHash))
            {
                if (handle == null) return;
                _session.RemoveTorrent(handle);
            }
        }
    }
}
