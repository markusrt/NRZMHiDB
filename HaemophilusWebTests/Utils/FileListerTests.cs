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
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_InvalidExtension_ThrowsException()
        {
            new FileLister(TemporaryDirectoryToStoreTestData, "doc");
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
    }
}