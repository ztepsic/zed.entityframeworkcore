using NUnit.Framework;
using System.Threading.Tasks;
using Zed.EntityFrameworkCore.Tests.Model;
using Zed.EntityFrameworkCore.Tests.Test;

namespace Zed.EntityFrameworkCore.Tests {
    public class EfCoreUnitOfWorkManagerTests : SqliteEfCoreTestFixture {

        [Test]
        public void Start_NestedScopeRollbackThenRootCommit_BothChangesArePersisted() {
            // Arrange
            Tag tag1 = Tag.CreateBaseTag("tag1");
            Tag tag2 = Tag.CreateBaseTag("tag2");
            Tag? result1;
            Tag? result2;

            // Act
            var unitOfWork = EfCoreUnitOfWorkManager.Create(TestDbContext);
            using (var unitOfWorkRootScope = unitOfWork.Start()) {
                TestDbContext.Add(tag1);

                using (var unitOfWorkScope = unitOfWork.Start()) {
                    TestDbContext.Add(tag2);
                    unitOfWorkScope.Rollback();
                }

                unitOfWorkRootScope.Commit();
            }

            using (var unitOfWorkRootScope = unitOfWork.Start()) {
                result1 = TestDbContext.Tags.Find(1);
                result2 = TestDbContext.Tags.Find(2);
            }


            // Assert

            using (Assert.EnterMultipleScope()) {
                Assert.That(result1, Is.Not.Null);
                Assert.That(result1, Is.EqualTo(tag1));

                Assert.That(result2, Is.Not.Null);
                Assert.That(result2, Is.EqualTo(tag2));
            }

        }

        [Test]
        public async Task StartAsync_NestedScopeRollbackThenRootCommit_BothChangesArePersisted() {
            // Arrange
            Tag tag1 = Tag.CreateBaseTag("tag1");
            Tag tag2 = Tag.CreateBaseTag("tag2");
            Tag? result1;
            Tag? result2;

            // Act
            var unitOfWorkManager = EfCoreUnitOfWorkManager.Create(TestDbContext);
            using (var unitOfWork = await unitOfWorkManager.StartAsync()) {
                TestDbContext.Add(tag1);

                using (var unitOfWorkScope = await unitOfWorkManager.StartAsync()) {
                    TestDbContext.Add(tag2);
                    await unitOfWorkScope.RollbackAsync();
                }

                await unitOfWork.CommitAsync();
            }

            using (var unitOfWork = await unitOfWorkManager.StartAsync()) {
                result1 = TestDbContext.Tags.Find(1);
                result2 = TestDbContext.Tags.Find(2);
            }


            // Assert
            using (Assert.EnterMultipleScope()) {
                Assert.That(result1, Is.Not.Null);
                Assert.That(result1, Is.EqualTo(tag1));

                Assert.That(result2, Is.Not.Null);
                Assert.That(result2, Is.EqualTo(tag2));
            }

        }

        [Test]
        public async Task StartAsync_CommitFollowedByRollbackInSingleScope_OnlyCommittedChangePersists() {
            // Arrange
            Tag tag1 = Tag.CreateBaseTag("tag1");
            Tag tag2 = Tag.CreateBaseTag("tag2");
            Tag result1;
            Tag result2;

            // Act
            var unitOfWorkManager = EfCoreUnitOfWorkManager.Create(TestDbContext);
            using (var unitOfWork = await unitOfWorkManager.StartAsync()) {
                TestDbContext.Add(tag1);
                await unitOfWork.CommitAsync();

                TestDbContext.Add(tag2);
                unitOfWork.RollbackAsync();

            }

            using (var unitOfWork = unitOfWorkManager.Start()) {
                result1 = TestDbContext.Tags.Find(1);
                result2 = TestDbContext.Tags.Find(2);
            }

            // Assert
            using (Assert.EnterMultipleScope()) {
                Assert.That(result1, Is.Not.Null);
                Assert.That(result1, Is.EqualTo(tag1));

                Assert.That(result2, Is.Null);
            }

        }

    }
}
