using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using API.Interfaces;
using API.Entities;
using API.Services;
using NUnit.Framework.Legacy;

namespace API.Tests
{
    public class PhotoApprovalTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPhotoRepository> _photoRepoMock;
        private Mock<IUserRepository> _userRepoMock;
        private Mock<IPhotoService> _photoServiceMock;
        private Mock<UserManager<AppUser>> _userManagerMock;
        private Mock<ILogger<AdminService>> _loggerMock;
        private AdminService _adminService;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _photoRepoMock = new Mock<IPhotoRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _photoServiceMock = new Mock<IPhotoService>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null
            );
            _loggerMock = new Mock<ILogger<AdminService>>();

            _unitOfWorkMock.Setup(u => u.PhotosRepository).Returns(_photoRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepoMock.Object);

            _adminService = new AdminService(
                _unitOfWorkMock.Object,
                _photoServiceMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object
            );
        }

        [Test]
        public async Task ApprovePhotoAsync_ApprovesPhoto_WhenPhotoExists()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 1,
                AppUserId = 2,
                isApproved = false,
                Url = "https://test.com/photo.jpg"
            };

            var user = new AppUser
            {
                Id = 2,
                UserName = "testuser",
                KnownAs = "Test",
                Gender = "male",
                City = "TestCity",
                Country = "TestCountry",
                Photos = new List<Photo> { photo }
            };

            _photoRepoMock.Setup(r => r.GetPhotoById(1)).ReturnsAsync(photo);
            _userRepoMock.Setup(r => r.GetUserByIdAsync(2)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(true);

            // Act
            await _adminService.ApprovePhotoAsync(1);

            // Assert
            ClassicAssert.IsTrue(photo.isApproved);
        }

        [Test]
        public async Task ApprovePhotoAsync_SetAsMain_WhenNoMainPhotoExists()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 1,
                AppUserId = 2,
                isApproved = false,
                Url = "https://test.com/photo.jpg"
            };

            var user = new AppUser
            {
                Id = 2,
                UserName = "testuser",
                KnownAs = "Test",
                Gender = "male",
                City = "TestCity",
                Country = "TestCountry",
                Photos = new List<Photo> { photo }
            };

            _photoRepoMock.Setup(r => r.GetPhotoById(1)).ReturnsAsync(photo);
            _userRepoMock.Setup(r => r.GetUserByIdAsync(2)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(true);

            // Act
            await _adminService.ApprovePhotoAsync(1);

            // Assert
            ClassicAssert.IsTrue(photo.IsMain);
        }

        [Test]
        public void ApprovePhotoAsync_ThrowsError_ResponsePhotoNotFound()
        {
            // Arrange
            _photoRepoMock.Setup(r => r.GetPhotoById(1)).ReturnsAsync((Photo)null!);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _adminService.ApprovePhotoAsync(1));
            Assert.That(ex!.Message, Is.EqualTo("Photo not found."));
        }

        [Test]
        public void ApprovePhotoAsync_ThrowsError_ResponseProblemApproving()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 1,
                AppUserId = 2,
                isApproved = false,
                Url = "https://test.com/photo.jpg"
            };

            var user = new AppUser
            {
                Id = 2,
                UserName = "testuser",
                KnownAs = "Test",
                Gender = "male",
                City = "TestCity",
                Country = "TestCountry",
                Photos = new List<Photo> { photo }
            };

            _photoRepoMock.Setup(r => r.GetPhotoById(1)).ReturnsAsync(photo);
            _userRepoMock.Setup(r => r.GetUserByIdAsync(2)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _adminService.ApprovePhotoAsync(1));
            Assert.That(ex!.Message, Is.EqualTo("Problem approving photo."));
        }
    }
}
