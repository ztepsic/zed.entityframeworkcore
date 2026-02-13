using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zed.EntityFrameworkCore.Test;
using Zed.EntityFrameworkCore.Tests.InitDb;
using Zed.EntityFrameworkCore.Tests.Model;

namespace Zed.EntityFrameworkCore.Tests {
    [TestFixtureSource(nameof(FixtureSources))]
    public class EfCoreCrudRepositoryTests : BaseConsolidatedTest {
        private EfCoreCrudRepository<Tag> _repository = null!;

        public EfCoreCrudRepositoryTests(Func<EfCoreTestFixture> fixtureFactory) : base(fixtureFactory) {
        }

        public static IEnumerable<TestFixtureData> FixtureSources() => GetFixtureSources();

        protected override void OnTestSetup() {
            _repository = new EfCoreCrudRepository<Tag>(TestDbContext);
        }

        #region GetAll Tests

        [Test]
        public void GetAll_WhenNoEntities_ReturnsEmptyCollection() {
            // Act
            var result = _repository.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAll_WhenEntitiesExist_ReturnsAllEntities() {
            // Arrange
            var tag1 = Tag.CreateBaseTag("tag1");
            var tag2 = Tag.CreateBaseTag("tag2");
            var tag3 = Tag.CreateBaseTag("tag3");
            TestDbContext.Tags.AddRange(tag1, tag2, tag3);
            TestDbContext.SaveChanges();

            // Act
            var result = _repository.GetAll().ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Contains.Item(tag1));
            Assert.That(result, Contains.Item(tag2));
            Assert.That(result, Contains.Item(tag3));
        }

        [Test]
        public void GetAll_ReturnsTrackedEntities() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            TestDbContext.SaveChanges();
            TestDbContext.Entry(tag).State = EntityState.Detached;

            // Act
            var result = _repository.GetAll().First();

            // Assert
            var entry = TestDbContext.Entry(result);
            Assert.That(entry.State, Is.EqualTo(EntityState.Unchanged));
        }

        [Test]
        public async Task GetAllAsync_WhenNoEntities_ReturnsEmptyCollection() {
            // Act
            var result = await _repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllAsync_WhenEntitiesExist_ReturnsAllEntities() {
            // Arrange
            var tag1 = Tag.CreateBaseTag("tag1");
            var tag2 = Tag.CreateBaseTag("tag2");
            var tag3 = Tag.CreateBaseTag("tag3");
            TestDbContext.Tags.AddRange(tag1, tag2, tag3);
            await TestDbContext.SaveChangesAsync();

            // Act
            var result = (await _repository.GetAllAsync(CancellationToken.None)).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Contains.Item(tag1));
            Assert.That(result, Contains.Item(tag2));
            Assert.That(result, Contains.Item(tag3));
        }

        [Test]
        public async Task GetAllAsync_ReturnsTrackedEntities() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            await TestDbContext.SaveChangesAsync();
            TestDbContext.Entry(tag).State = EntityState.Detached;

            // Act
            var result = (await _repository.GetAllAsync(CancellationToken.None)).First();

            // Assert
            var entry = TestDbContext.Entry(result);
            Assert.That(entry.State, Is.EqualTo(EntityState.Unchanged));
        }

        [Test]
        public void GetAllAsync_WhenCancellationRequested_ThrowsOperationCanceledException() {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await _repository.GetAllAsync(cts.Token)
            );
        }

        #endregion

        #region SaveOrUpdate Tests

        [Test]
        public void SaveOrUpdate_WhenEntityIsDetached_AddsEntityToContext() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");

            // Act
            _repository.SaveOrUpdate(tag);

            // Assert
            var entry = TestDbContext.Entry(tag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Added));
        }

        [Test]
        public void SaveOrUpdate_WhenEntityIsDetached_SavesEntityToDatabase() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");

            // Act
            _repository.SaveOrUpdate(tag);
            TestDbContext.SaveChanges();

            // Assert
            var result = TestDbContext.Tags.Find(tag.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("tag1"));
        }

        [Test]
        public void SaveOrUpdate_WhenEntityIsModified_UpdatesEntity() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            TestDbContext.SaveChanges();
            TestDbContext.Entry(tag).State = EntityState.Detached;

            var retrievedTag = TestDbContext.Tags.Find(tag.Id);
            retrievedTag!.Name = "modified";

            // Act
            _repository.SaveOrUpdate(retrievedTag);

            // Assert
            var entry = TestDbContext.Entry(retrievedTag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Modified));
        }

        [Test]
        public void SaveOrUpdate_WhenEntityIsUnchanged_DoesNothing() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            TestDbContext.SaveChanges();

            // Act
            _repository.SaveOrUpdate(tag);

            // Assert
            var entry = TestDbContext.Entry(tag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Unchanged));
        }

        [Test]
        public void SaveOrUpdate_WhenEntityIsAdded_KeepsAddedState() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);

            // Act
            _repository.SaveOrUpdate(tag);

            // Assert
            var entry = TestDbContext.Entry(tag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Added));
        }

        [Test]
        public async Task SaveOrUpdateAsync_WhenEntityIsDetached_AddsEntityToContext() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");

            // Act
            await _repository.SaveOrUpdateAsync(tag, CancellationToken.None);

            // Assert
            var entry = TestDbContext.Entry(tag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Added));
        }

        [Test]
        public async Task SaveOrUpdateAsync_WhenEntityIsDetached_SavesEntityToDatabase() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");

            // Act
            await _repository.SaveOrUpdateAsync(tag, CancellationToken.None);
            await TestDbContext.SaveChangesAsync();

            // Assert
            var result = await TestDbContext.Tags.FindAsync(tag.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("tag1"));
        }

        [Test]
        public async Task SaveOrUpdateAsync_WhenEntityIsModified_UpdatesEntity() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            await TestDbContext.SaveChangesAsync();
            TestDbContext.Entry(tag).State = EntityState.Detached;

            var retrievedTag = await TestDbContext.Tags.FindAsync(tag.Id);
            retrievedTag!.Name = "modified";

            // Act
            await _repository.SaveOrUpdateAsync(retrievedTag, CancellationToken.None);

            // Assert
            var entry = TestDbContext.Entry(retrievedTag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Modified));
        }

        [Test]
        public async Task SaveOrUpdateAsync_WhenEntityIsUnchanged_DoesNothing() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            await TestDbContext.SaveChangesAsync();

            // Act
            await _repository.SaveOrUpdateAsync(tag, CancellationToken.None);

            // Assert
            var entry = TestDbContext.Entry(tag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Unchanged));
        }

        [Test]
        public async Task SaveOrUpdateAsync_WhenEntityIsAdded_KeepsAddedState() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);

            // Act
            await _repository.SaveOrUpdateAsync(tag, CancellationToken.None);

            // Assert
            var entry = TestDbContext.Entry(tag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Added));
        }

        [Test]
        public void SaveOrUpdateAsync_WhenCancellationRequested_ThrowsOperationCanceledException() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await _repository.SaveOrUpdateAsync(tag, cts.Token)
            );
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_MarksEntityAsDeleted() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            TestDbContext.SaveChanges();

            // Act
            _repository.Delete(tag);

            // Assert
            var entry = TestDbContext.Entry(tag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Deleted));
        }

        [Test]
        public void Delete_RemovesEntityFromDatabase() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            TestDbContext.SaveChanges();
            var id = tag.Id;

            // Act
            _repository.Delete(tag);
            TestDbContext.SaveChanges();

            // Assert
            var result = TestDbContext.Tags.Find(id);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_MarksEntityAsDeleted() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            await TestDbContext.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(tag, CancellationToken.None);

            // Assert
            var entry = TestDbContext.Entry(tag);
            Assert.That(entry.State, Is.EqualTo(EntityState.Deleted));
        }

        [Test]
        public async Task DeleteAsync_RemovesEntityFromDatabase() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            await TestDbContext.SaveChangesAsync();
            var id = tag.Id;

            // Act
            await _repository.DeleteAsync(tag, CancellationToken.None);
            await TestDbContext.SaveChangesAsync();

            // Assert
            var result = await TestDbContext.Tags.FindAsync(id);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void DeleteAsync_WhenCancellationRequested_ThrowsOperationCanceledException() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await _repository.DeleteAsync(tag, cts.Token)
            );
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_SaveUpdateAndDelete_WorksCorrectly() {
            // Arrange & Act - Save
            var tag = Tag.CreateBaseTag("tag1");
            _repository.SaveOrUpdate(tag);
            TestDbContext.SaveChanges();
            var id = tag.Id;

            // Assert - Save
            var savedTag = TestDbContext.Tags.Find(id);
            Assert.That(savedTag, Is.Not.Null);
            Assert.That(savedTag.Name, Is.EqualTo("tag1"));

            // Act - Update
            savedTag.Name = "updatedTag";
            _repository.SaveOrUpdate(savedTag);
            TestDbContext.SaveChanges();

            // Assert - Update
            TestDbContext.Entry(savedTag).State = EntityState.Detached;
            var updatedTag = TestDbContext.Tags.Find(id);
            Assert.That(updatedTag!.Name, Is.EqualTo("updatedTag"));

            // Act - Delete
            _repository.Delete(updatedTag);
            TestDbContext.SaveChanges();

            // Assert - Delete
            var deletedTag = TestDbContext.Tags.Find(id);
            Assert.That(deletedTag, Is.Null);
        }

        [Test]
        public async Task Integration_SaveUpdateAndDeleteAsync_WorksCorrectly() {
            // Arrange & Act - Save
            var tag = Tag.CreateBaseTag("tag1");
            await _repository.SaveOrUpdateAsync(tag, CancellationToken.None);
            await TestDbContext.SaveChangesAsync();
            var id = tag.Id;

            // Assert - Save
            var savedTag = await TestDbContext.Tags.FindAsync(id);
            Assert.That(savedTag, Is.Not.Null);
            Assert.That(savedTag.Name, Is.EqualTo("tag1"));

            // Act - Update
            savedTag.Name = "updatedTag";
            await _repository.SaveOrUpdateAsync(savedTag, CancellationToken.None);
            await TestDbContext.SaveChangesAsync();

            // Assert - Update
            TestDbContext.Entry(savedTag).State = EntityState.Detached;
            var updatedTag = await TestDbContext.Tags.FindAsync(id);
            Assert.That(updatedTag!.Name, Is.EqualTo("updatedTag"));

            // Act - Delete
            await _repository.DeleteAsync(updatedTag, CancellationToken.None);
            await TestDbContext.SaveChangesAsync();

            // Assert - Delete
            var deletedTag = await TestDbContext.Tags.FindAsync(id);
            Assert.That(deletedTag, Is.Null);
        }

        #endregion
    }
}
