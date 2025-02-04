﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;
using UIKit;

namespace NativeMedia
{
    partial class MediaFile
    {
        protected virtual Task<Stream> PlatformOpenReadAsync()
           => Task.FromResult<Stream>(null);

        protected virtual void PlatformDispose() { }

        protected string GetExtension(string identifier)
            => UTType.CopyAllTags(identifier, UTType.TagClassFilenameExtension)?.FirstOrDefault();

        protected string GetMIMEType(string identifier)
            => UTType.CopyAllTags(identifier, UTType.TagClassMIMEType)?.FirstOrDefault();
    }

    class PHPickerFile : MediaFile
    {
        readonly string typeIdentifier;
        NSItemProvider provider;

        internal PHPickerFile(NSItemProvider provider, string assetIdentifier)
        {
            this.provider = provider;
            NameWithoutExtension = provider?.SuggestedName;
            var identifiers = provider?.RegisteredTypeIdentifiers;

            typeIdentifier = (identifiers?.Any(i => i.StartsWith(UTType.LivePhoto)) ?? false) && (identifiers?.Contains(UTType.JPEG) ?? false)
                ? identifiers?.FirstOrDefault(i => i == UTType.JPEG)
                : identifiers?.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(typeIdentifier))
                return;

            Extension = GetExtension(typeIdentifier);
            ContentType = GetMIMEType(typeIdentifier);
            MediaIdentifier = assetIdentifier;
        }

        protected override async Task<Stream> PlatformOpenReadAsync()
            => (await provider?.LoadDataRepresentationAsync(typeIdentifier))?.AsStream();

        protected override void PlatformDispose()
        {
            provider?.Dispose();
            provider = null;
            base.PlatformDispose();
        }
    }

    class UIDocumentFile : MediaFile
    {
        UIDocument document;

        internal UIDocumentFile(NSUrl assetUrl, string fileName)
        {
            document = new UIDocument(assetUrl);
            Extension = document.FileUrl.PathExtension;
            ContentType = GetMIMEType(document.FileType);
            NameWithoutExtension = !string.IsNullOrWhiteSpace(fileName)
                ? Path.GetFileNameWithoutExtension(fileName)
                : null;
            MediaIdentifier = assetUrl.Path;
        }

        protected override Task<Stream> PlatformOpenReadAsync()
            => Task.FromResult<Stream>(File.OpenRead(document.FileUrl.Path));

        protected override void PlatformDispose()
        {
            document?.Dispose();
            document = null;
            base.PlatformDispose();
        }
    }
}
