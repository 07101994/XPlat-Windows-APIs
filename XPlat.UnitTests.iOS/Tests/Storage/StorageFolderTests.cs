﻿namespace XPlat.UnitTests.iOS.Tests.Storage
{
    using NUnit.Framework;

    using XPlat.Storage;
    using XPlat.UnitTests.iOS.Helpers;

    [TestFixture]
    public class StorageFolderTests
    {
        [Test]
        public void StorageFolder_ReturnsBasicProperties()
        {
            const string Text = "Hello, World!";

            var folder = StorageHelper.CreateStorageFolder(ApplicationData.Current.LocalFolder, "FolderTest", CreationCollisionOption.GenerateUniqueName);

            var props = StorageHelper.GetBasicProperties(folder);

            Assert.AreEqual(0, props.Size);

            var file1 = StorageHelper.CreateStorageFile(folder, "TestFile1.txt");

            StorageHelper.WriteTextToFile(file1, Text);

            var updatedProps = StorageHelper.GetBasicProperties(folder);

            Assert.AreEqual(Text.Length, updatedProps.Size);
        }

        [Test]
        public void StorageFolder_GetItemsAsync_ReturnsSubsetOfItems()
        {
            const int ItemsToRetrieve = 20;

            for (var i = 0; i < 5; i++)
            {
                StorageHelper.CreateStorageFolder(
                    ApplicationData.Current.LocalFolder,
                    $"Folder{i}",
                    CreationCollisionOption.ReplaceExisting);
            }

            for (var i = 0; i < 5; i++)
            {
                StorageHelper.CreateStorageFile(
                    ApplicationData.Current.LocalFolder,
                    $"File{i}.txt",
                    CreationCollisionOption.ReplaceExisting);
            }

            var items = StorageHelper.GetFolderItems(ApplicationData.Current.LocalFolder, 0, ItemsToRetrieve);

            Assert.AreEqual(ItemsToRetrieve, items.Count);
        }

        [Test]
        public void StorageFolder_TryGetItemAsync_ReturnsExistingItem()
        {
            const string FileName = "Test.txt";

            StorageHelper.CreateStorageFile(
                ApplicationData.Current.LocalFolder,
                FileName,
                CreationCollisionOption.ReplaceExisting);

            var item = StorageHelper.TryGetItem(ApplicationData.Current.LocalFolder, FileName);

            Assert.IsNotNull(item);
        }

        [Test]
        public void StorageFolder_TryGetItemAsync_ReturnsNullForNonExistingItem()
        {
            const string FileName = "DefinitelyNotAFile.txt";

            var item = StorageHelper.TryGetItem(ApplicationData.Current.LocalFolder, FileName);

            Assert.IsNull(item);
        }
    }
}