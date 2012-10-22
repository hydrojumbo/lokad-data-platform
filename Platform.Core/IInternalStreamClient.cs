﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Platform.Storage;
using Platform.Storage.Azure;

namespace Platform
{
    /// <summary>
    /// Provides raw byte-level access to the storage and messaging of
    /// Data platform
    /// </summary>
    public interface IInternalStreamClient
    {
        /// <summary>
        /// Returns lazy enumeration over all events in a given record range. 
        /// </summary>
        IEnumerable<RetrievedDataRecord> ReadAll(StorageOffset startOffset = default (StorageOffset),
            int maxRecordCount = int.MaxValue);

        void WriteEvent(string streamName, byte[] data);
        void WriteEventsInLargeBatch(string streamName, IEnumerable<RecordForStaging> records);
    }

    public class InternalPlatformClient
    {
        public readonly IInternalStreamClient Streams;
        public readonly IViewContainer Views;


        public InternalPlatformClient(IInternalStreamClient streams,IViewContainer views)
        {
            Streams = streams;
            Views = views;
        }

        public static InternalPlatformClient ForFiles(string connection, string root)
        {
            return new InternalPlatformClient(new FilePlatformClient(root, connection), new FileViewContainer(Path.Combine(root, "views")));
        }
        public static InternalPlatformClient ForAzure(string connection, AzureStoreConfiguration config)
        {
                       var account = CloudStorageAccount.Parse(config.ConnectionString);
            var client = account.CreateCloudBlobClient();
            var dir = client.GetBlobDirectoryReference(config.Container + "-views");
            return new InternalPlatformClient(new AzurePlatformClient(config, connection), new BlobStreamingContainer(dir));
        }
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

    public struct RecordForStaging
    {
        public readonly byte[] Data;
        public RecordForStaging(byte[] data)
        {
            Data = data;
        }
    }


}