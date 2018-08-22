﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Serializer;
using Foundatio.Utility;

namespace Foundatio.Storage {
    public interface IFileStorage : IHaveSerializer, IDisposable {
        Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);
        Task<FileSpec> GetFileInfoAsync(string path);
        Task<bool> ExistsAsync(string path);
        Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);
        Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default);
        Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);
        Task DeleteFilesAsync(string searchPattern = null, CancellationToken cancellation = default);
        Task<IEnumerable<FileSpec>> GetFileListAsync(string searchPattern = null, int? limit = null, int? skip = null, CancellationToken cancellationToken = default);
    }

    [DebuggerDisplay("Path = {Path}, Created = {Created}, Modified = {Modified}, Size = {Size} bytes")]
    public class FileSpec {
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        /// <summary>
        /// In Bytes
        /// </summary>
        public long Size { get; set; }
        // TODO: Add metadata object for custom properties
    }

    public static class FileStorageExtensions {
        public static Task<bool> SaveObjectAsync<T>(this IFileStorage storage, string path, T data, CancellationToken cancellationToken = default) {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var bytes = storage.Serializer.SerializeToBytes(data);
            return storage.SaveFileAsync(path, new MemoryStream(bytes), cancellationToken);
        }

        public static async Task<T> GetObjectAsync<T>(this IFileStorage storage, string path, CancellationToken cancellationToken = default) {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            using (var stream = await storage.GetFileStreamAsync(path, cancellationToken).AnyContext()) {
                if (stream != null)
                    return storage.Serializer.Deserialize<T>(stream);
            }

            return default;
        }

        public static async Task DeleteFilesAsync(this IFileStorage storage, IEnumerable<FileSpec> files) {
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            foreach (var file in files)
                await storage.DeleteFileAsync(file.Path).AnyContext();
        }

        public static async Task<string> GetFileContentsAsync(this IFileStorage storage, string path) {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            using (var stream = await storage.GetFileStreamAsync(path).AnyContext()) {
                if (stream != null)
                    return await new StreamReader(stream).ReadToEndAsync().AnyContext();
            }

            return null;
        }

        public static async Task<byte[]> GetFileContentsRawAsync(this IFileStorage storage, string path) {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            using (var stream = await storage.GetFileStreamAsync(path).AnyContext()) {
                if (stream == null)
                    return null;

                var buffer = new byte[16 * 1024];
                using (var ms = new MemoryStream()) {
                    int read;
                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length).AnyContext()) > 0) {
                        await ms.WriteAsync(buffer, 0, read).AnyContext();
                    }

                    return ms.ToArray();
                }
            }
        }

        public static Task<bool> SaveFileAsync(this IFileStorage storage, string path, string contents) {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return storage.SaveFileAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(contents ?? String.Empty)));
        }
    }
}
