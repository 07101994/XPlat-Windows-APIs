﻿namespace XamKit.Core.Storage
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Windows.Storage;

    using XamKit.Core.Common.Serialization;
    using XamKit.Core.Common.Storage;
    using XamKit.Core.Extensions;
    using XamKit.Core.Serialization;

    public class AppFile : IAppFile
    {
        private readonly IStorageFile file;

        public AppFile(IAppFolder folder, IStorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            this.file = file;
            this.ParentFolder = folder;
        }

        /// <summary>
        /// Gets the name of the current file store item.
        /// </summary>
        public string Name
        {
            get
            {
                return this.file.Name;
            }
        }

        /// <summary>
        /// Gets the full path of the current file store item.
        /// </summary>
        public string Path
        {
            get
            {
                return this.file.Path;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the file store item exists.
        /// </summary>
        public bool Exists
        {
            get
            {
                return this.file != null;
            }
        }

        /// <summary>
        /// Gets the parent folder.
        /// </summary>
        public IAppFolder ParentFolder { get; private set; }

        /// <summary>
        /// Opens a stream containing the file's data.
        /// </summary>
        /// <param name="option">
        /// Optional, specifies the type of access to allow for the file. Default to read only.
        /// </param>
        /// <returns>
        /// Returns a Stream that contains the requested file's data.
        /// </returns>
        public async Task<Stream> OpenAsync(FileAccessOption option = FileAccessOption.ReadOnly)
        {
            var iras = await this.file.OpenAsync(option.ToFileAccessMode());
            return iras.AsStream();
        }

        /// <summary>
        /// Moves the file to the specified folder.
        /// </summary>
        /// <param name="destinationFolder">
        /// The destination folder.
        /// </param>
        /// <param name="newName">
        /// Optional, specifies the new name for the file. If null or empty, will use existing name.
        /// </param>
        /// <param name="option">
        /// Optional, specifies how to handle moving the file when there is a conflict. Default to replace if already exists.
        /// </param>
        /// <returns>
        /// Returns an await-able task.
        /// </returns>
        public async Task MoveAsync(
            IAppFolder destinationFolder,
            string newName = "",
            FileNameCreationOption option = FileNameCreationOption.ReplaceIfExists)
        {
            if (destinationFolder == null)
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }

            if (string.IsNullOrWhiteSpace(destinationFolder.Path))
            {
                throw new ArgumentException("The provided destination folder doesn't have a file path.");
            }

            var folder =
                await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(destinationFolder.Path));

            if (string.IsNullOrWhiteSpace(newName))
            {
                await this.file.MoveAsync(folder, this.file.Name, option.ToNameCollisionOption());
            }
            else
            {
                await this.file.MoveAsync(folder, newName, option.ToNameCollisionOption());
            }

            this.ParentFolder = destinationFolder;
        }

        /// <summary>
        /// Renames the file.
        /// </summary>
        /// <param name="fileName">
        /// The new file name.
        /// </param>
        /// <param name="option">
        /// Optional, specifies how to handle renaming the file when there is a conflict. Default to throw exception if already exists.
        /// </param>
        /// <returns>
        /// Returns an await-able task.
        /// </returns>
        public async Task RenameAsync(
            string fileName,
            FileNameCreationOption option = FileNameCreationOption.ThrowExceptionIfExists)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            await this.file.RenameAsync(fileName, option.ToNameCollisionOption());
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <returns>
        /// Returns an await-able task.
        /// </returns>
        public async Task DeleteAsync()
        {
            await this.file.DeleteAsync();
        }

        /// <summary>
        /// Serializes an object to the file as a string. Will overwrite any data already stored in the file.
        /// </summary>
        /// <param name="dataToSerialize">
        /// The data to serialize.
        /// </param>
        /// <param name="serializationService">
        /// The service for serialization. If null, will use JSON.
        /// </param>
        /// <typeparam name="T">
        /// The type of object to save.
        /// </typeparam>
        /// <returns>
        /// Returns an await-able task.
        /// </returns>
        public async Task SaveDataToFileAsync<T>(T dataToSerialize, ISerializationService serializationService)
        {
            if (!this.Exists)
            {
                throw new NotSupportedException("Cannot save data to a file that does not exist.");
            }

            if (serializationService == null)
            {
                serializationService = SerializationService.Json;
            }

            var serializedData = serializationService.Serialize(dataToSerialize);

            await this.WriteTextAsync(serializedData);
        }

        /// <summary>
        /// Deserializes an object from the file.
        /// </summary>
        /// <param name="serializationService">
        /// The service for seialization. If null, will use JSON.
        /// </param>
        /// <typeparam name="T">
        /// The type of object to load.
        /// </typeparam>
        /// <returns>
        /// Returns the deserialized data.
        /// </returns>
        public async Task<T> LoadDataFromFileAsync<T>(ISerializationService serializationService)
        {
            if (!this.Exists)
            {
                throw new NotSupportedException("Cannot read data from a file that does not exist.");
            }

            if (serializationService == null)
            {
                serializationService = SerializationService.Json;
            }

            var serializedData = await this.ReadTextAsync();

            var deserializedData = serializationService.Deserialize<T>(serializedData);
            return deserializedData;
        }

        /// <summary>
        /// Writes text to the file.
        /// </summary>
        /// <param name="text">
        /// The text to write out.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task WriteTextAsync(string text)
        {
            await FileIO.WriteTextAsync(this.file, text);
        }

        /// <summary>
        /// Reads text from the file.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<string> ReadTextAsync()
        {
            var text = await FileIO.ReadTextAsync(this.file);
            return text;
        }
    }
}