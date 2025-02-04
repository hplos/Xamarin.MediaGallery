﻿using System.IO;
using System.Threading.Tasks;
using Android.Webkit;
using Uri = Android.Net.Uri;

namespace NativeMedia
{
    partial class MediaFile
    {
        readonly Uri uri;

        internal MediaFile(string fileName, Uri uri)
        {
            NameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            Extension = Path.GetExtension(fileName);
            ContentType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(Extension);
            MediaIdentifier = uri.ToString();
            this.uri = uri;
        }

        Task<Stream> PlatformOpenReadAsync()
            => Task.FromResult(Platform.AppActivity.ContentResolver.OpenInputStream(uri));

        void PlatformDispose() { }
    }
}
