﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MetadataExtractor;
using NativeMedia;
using Sample.Helpers;

namespace Sample.ViewModels
{
    public class MediaFileInfoVM : BaseVM
    {
        public MediaFileInfoVM(IMediaFile file)
        {
            File = file;
            IsImage = file?.Type == MediaFileType.Image;
            IsVideo = file?.Type == MediaFileType.Video;
        }

        public IMediaFile File { get; }

        public string Path { get; private set; }

        public string Metadata { get; private set; }

        public bool IsImage { get; }

        public bool IsVideo { get; }

        public bool IsBusy { get; private set; }

        public override void OnAppearing()
        {
            base.OnAppearing();

            Task.Run(async () =>
            {
                IsBusy = true;
                try
                {
                    using var stream = await File.OpenReadAsync();

                    var name = (string.IsNullOrWhiteSpace(File.NameWithoutExtension)
                        ? Guid.NewGuid().ToString()
                        : File.NameWithoutExtension)
                        + $".{File.Extension}";

                    Path = await FilesHelper.SaveToCacheAsync(stream, name);
                    stream.Position = 0;
                    await ReadMeta(stream);
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync(ex.Message);
                }
                IsBusy = false;
            });      
        }

        async Task ReadMeta(Stream stream)
        {
            var metaSB = new StringBuilder();

            try
            {
                var directories = ImageMetadataReader.ReadMetadata(stream);

                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                        metaSB.AppendLine(tag.ToString());

                    foreach (var error in directory.Errors)
                        metaSB.AppendLine("ERROR: " + error);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(ex.Message);
            }
            Metadata = metaSB.ToString();
        }

    }
}
