﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using Platform.StreamStorage;

namespace Platform.StreamClients
{
    [Serializable]
    public class PlatformClientException : Exception
    {
        public PlatformClientException(string message, Exception ex) : base(message, ex) {}
        public PlatformClientException(string message) : base(message) {}

    }


    /// <summary>
    /// Provides raw byte-level access to the storage and messaging of
    /// Data platform
    /// </summary>
    public interface IRawEventStoreClient
    {
        /// <summary>
        /// Returns lazy enumeration over all events in a given record range. 
        /// </summary>
        IEnumerable<RetrievedEventWithMetaData> ReadAllEvents(
            StorageOffset startOffset = default (StorageOffset),
            int maxRecordCount = int.MaxValue);
        /// <summary>
        /// Writes a single event to the storage under the given key. Use this method
        /// for high-concurrency and low latency operations
        /// </summary>
        /// <param name="streamName">Name of the stream to upload to</param>
        /// <param name="eventData">Event Data to upload</param>
        void WriteEvent(string streamName, byte[] eventData);
        /// <summary>
        /// Writes events to server in a batch by first uploading it to the staging ground
        /// (near the server) and then issuing an import request. This method has more
        /// latency but is optimized for really high throughput.
        /// </summary>
        /// <param name="streamName">Name of the stream to upload to</param>
        /// <param name="eventData">Enumeration of the events to upload (can be lazy)</param>
        void WriteEventsInLargeBatch(string streamName, IEnumerable<byte[]> eventData);
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct StorageOffset
    {
        public readonly long OffsetInBytes;

        public static readonly StorageOffset Zero = new StorageOffset(0);

        public override string ToString()
        {
            return string.Format("Offset {0}b", OffsetInBytes);
        }

        public StorageOffset(long offsetInBytes)
        {
            Ensure.Nonnegative(offsetInBytes, "offsetInBytes");
            OffsetInBytes = offsetInBytes;
        }

        public static   bool operator >(StorageOffset x , StorageOffset y)
        {
            return x.OffsetInBytes > y.OffsetInBytes;
        }
        public static bool operator <(StorageOffset x , StorageOffset y)
        {
            return x.OffsetInBytes < y.OffsetInBytes;
        }
        public static bool operator >= (StorageOffset left, StorageOffset right)
        {
            return left.OffsetInBytes >= right.OffsetInBytes;
        }
        public static bool operator <=(StorageOffset left, StorageOffset right)
        {
            return left.OffsetInBytes <= right.OffsetInBytes;
        }


    }
}