using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    [TestFixture]
    public class FileListerTests : ITempDirectoryTest
    {
        public string TemporaryDirectoryToStoreTestData { get; set; }

        private string[] filesToCreate = new[] {"test1.docx", "test2.docx", "test1.exe"};

        [Test]
        public void Ctor_InvalidExtension_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new FileLister(TemporaryDirectoryToStoreTestData, "doc"));
        }

        [Test]
        public void Files_UnknownDirectory_ReturnsEmptyList()
        {
            var lister = new FileLister(@"c:\foo\bar\baz", ".doc");

            var files = lister.Files;

            files.Should().BeEmpty();
        }

        [Test]
        public void Files_ExistingDirectory_ListsAllEntries()
        {
            foreach (var file in filesToCreate)
            {
                File.WriteAllText(Path.Combine(TemporaryDirectoryToStoreTestData, file), "content");
            }
            var lister = new FileLister(TemporaryDirectoryToStoreTestData, ".doc");

            var files = lister.Files;

            files.Should().NotBeEmpty();
            files.Select(f => f.Name).Should().Contain("test1.docx");
            files.Select(f => f.Name).Should().Contain("test2.docx");
            files.Select(f => f.Name).Should().NotContain("test1.exe");
        }

        // specs/010 FR-002, Edge Cases (sub-folders such as Meningo/ and includes/ are never listed)
        [Test]
        public void Files_SubFolderWithMatchingFiles_ListsTopLevelEntriesOnly()
        {
            File.WriteAllText(Path.Combine(TemporaryDirectoryToStoreTestData, "top.docx"), "content");
            var subFolder = Path.Combine(TemporaryDirectoryToStoreTestData, "includes");
            Directory.CreateDirectory(subFolder);
            File.WriteAllText(Path.Combine(subFolder, "nested.docx"), "content");
            var lister = new FileLister(TemporaryDirectoryToStoreTestData, ".docx");

            var files = lister.Files;

            files.Select(f => f.Name).Should().BeEquivalentTo("top.docx");
        }
    }
}