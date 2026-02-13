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
    public class EfCoreReadOnlyRepositoryTests : BaseConsolidatedTest {
        private EfCoreReadOnlyRepository<Tag> _repository = null!;

        public EfCoreReadOnlyRepositoryTests(Func<EfCoreTestFixture> fixtureFactory) : base(fixtureFactory) {
        }

        public static IEnumerable<TestFixtureData> FixtureSources() => GetFixtureSources();

        protected override void OnTestSetup() {
            _repository = new EfCoreReadOnlyRepository<Tag>(TestDbContext);
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
        public void GetAll_ReturnsUntrackedEntities() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            TestDbContext.SaveChanges();
            TestDbContext.Entry(tag).State = EntityState.Detached;

            // Act
            var result = _repository.GetAll().First();

            // Assert
            var entry = TestDbContext.Entry(result);
            Assert.That(entry.State, Is.EqualTo(EntityState.Detached));
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
        public async Task GetAllAsync_ReturnsUntrackedEntities() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            await TestDbContext.SaveChangesAsync();
            TestDbContext.Entry(tag).State = EntityState.Detached;

            // Act
            var result = (await _repository.GetAllAsync(CancellationToken.None)).First();

            // Assert
            var entry = TestDbContext.Entry(result);
            Assert.That(entry.State, Is.EqualTo(EntityState.Detached));
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

        #region GetById Tests

        [Test]
        public void GetById_WhenEntityExists_ReturnsEntity() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            TestDbContext.SaveChanges();
            var id = tag.Id;

            // Act
            var result = _repository.GetById(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(id));
            Assert.That(result.Name, Is.EqualTo("tag1"));
        }

        [Test]
        public void GetById_WhenEntityDoesNotExist_ReturnsNull() {
            // Act
            var result = _repository.GetById(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_WhenEntityExists_ReturnsEntity() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            await TestDbContext.SaveChangesAsync();
            var id = tag.Id;

            // Act
            var result = await _repository.GetByIdAsync(id, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(id));
            Assert.That(result.Name, Is.EqualTo("tag1"));
        }

        [Test]
        public async Task GetByIdAsync_WhenEntityDoesNotExist_ReturnsNull() {
            // Act
            var result = await _repository.GetByIdAsync(999, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetByIdAsync_WhenCancellationRequested_ThrowsOperationCanceledException() {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await _repository.GetByIdAsync(1, cts.Token)
            );
        }

        #endregion

        #region Load Tests

        [Test]
        public void Load_WhenEntityExists_ReturnsEntity() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            TestDbContext.SaveChanges();
            var id = tag.Id;

            // Act
            var result = _repository.Load(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(id));
            Assert.That(result.Name, Is.EqualTo("tag1"));
        }

        [Test]
        public void Load_WhenEntityDoesNotExist_ThrowsInvalidOperationException() {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => _repository.Load(999)
            );
            Assert.That(ex.Message, Does.Contain("Tag"));
            Assert.That(ex.Message, Does.Contain("999"));
            Assert.That(ex.Message, Does.Contain("was not found"));
        }

        [Test]
        public async Task LoadAsync_WhenEntityExists_ReturnsEntity() {
            // Arrange
            var tag = Tag.CreateBaseTag("tag1");
            TestDbContext.Tags.Add(tag);
            await TestDbContext.SaveChangesAsync();
            var id = tag.Id;

            // Act
            var result = await _repository.LoadAsync(id, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(id));
            Assert.That(result.Name, Is.EqualTo("tag1"));
        }

        [Test]
        public void LoadAsync_WhenEntityDoesNotExist_ThrowsInvalidOperationException() {
            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _repository.LoadAsync(999, CancellationToken.None)
            );
            Assert.That(ex.Message, Does.Contain("Tag"));
            Assert.That(ex.Message, Does.Contain("999"));
            Assert.That(ex.Message, Does.Contain("was not found"));
        }

        [Test]
        public void LoadAsync_WhenCancellationRequested_ThrowsOperationCanceledException() {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await _repository.LoadAsync(1, cts.Token)
            );
        }

        #endregion
    }
}
