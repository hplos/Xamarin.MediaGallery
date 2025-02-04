﻿using System.IO;
using System.Threading.Tasks;

namespace NativeMedia
{
    partial class MediaFile : IMediaFile
    {
        private string extension;

        public string NameWithoutExtension { get; protected internal set; }

        public string Extension
        {
            get => extension;
            protected internal set
                => extension = value?.TrimStart('.')?.ToLower();
        }

        public string ContentType { get; protected internal set; }

        public MediaFileType? Type => ContentType.StartsWith("image")
                ? MediaFileType.Image
                : ContentType.StartsWith("video")
                    ? MediaFileType.Video
                    : (MediaFileType?)null;

        public string MediaIdentifier { get; protected internal set; }

        public Task<Stream> OpenReadAsync()
            => PlatformOpenReadAsync();

        public void Dispose()
            => PlatformDispose();
    }
}
