using Moq;
using API.Interfaces;
using API.Entities;
using API.Services;
using Microsoft.Extensions.Logging;
using AutoMapper;
using CloudinaryDotNet.Actions;

namespace API.Tests
{
    public class PhotoDeleteTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPhotoService> _photoServiceMock;
        private Mock<ILogger<UserService>> _loggerMock;
        private Mock<IMapper> _mapperMock;
        private UserService _userService;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _photoServiceMock = new Mock<IPhotoService>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _mapperMock = new Mock<IMapper>();

            _userService = new UserService(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _photoServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task DeletePhotoAsync_ShouldDeletePhoto_WhenValid()
        {
            var photo = new Photo
            {
                Id = 1,
                Url = "http://test.com",
                PublicId = "cloud123",
                IsMain = false,
                AppUserId = 2
            };

            var user = new AppUser
            {
                Id = 2,
                UserName = "reuf",
                KnownAs = "Reuf",
                Gender = "male",
                City = "Kakanj",
                Country = "Bosnia",
                Photos = new List<Photo> { photo }
            };

            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByUsernameAsync("reuf")).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.PhotosRepository.GetPhotoById(1)).ReturnsAsync(photo);
            _photoServiceMock.Setup(p => p.DeletePhotoAsync("cloud123")).ReturnsAsync(new DeletionResult { Result = "ok" });
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(true);

            await _userService.DeletePhotoAsync("reuf", 1);

            Assert.That(user.Photos.Contains(photo), Is.False);
        }
        [Test]
        public void DeletePhotoAsync_ShouldThrow_WhenPhotoDoesNotBelongToUser()
        {
            var photo = new Photo
            {
                Id = 1,
                Url = "http://test.com",
                PublicId = "cloud123",
                IsMain = false,
                AppUserId = 999
            };

            var user = new AppUser
            {
                Id = 2,
                UserName = "reuf",
                KnownAs = "Reuf",
                Gender = "male",
                City = "Kakanj",
                Country = "Bosnia",
                Photos = new List<Photo>()
            };

            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByUsernameAsync("reuf")).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.PhotosRepository.GetPhotoById(1)).ReturnsAsync(photo);

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _userService.DeletePhotoAsync("reuf", 1));

            Assert.That(ex!.Message, Is.EqualTo("You cannot delete photos that do not belong to you."));
        }

        [Test]
        public void DeletePhotoAsync_ShouldThrow_WhenDeletingMainPhoto_AndNoOtherPhotoIsMain()
        {
            var mainPhoto = new Photo
            {
                Id = 1,
                Url = "http://main.com",
                IsMain = true,
                PublicId = "main123",
                AppUserId = 2
            };

            var user = new AppUser
            {
                Id = 2,
                UserName = "reuf",
                KnownAs = "Reuf",
                Gender = "male",
                City = "Kakanj",
                Country = "Bosnia",
                Photos = new List<Photo> { mainPhoto }
            };

            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByUsernameAsync("reuf")).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.PhotosRepository.GetPhotoById(1)).ReturnsAsync(mainPhoto);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.DeletePhotoAsync("reuf", 1));

            Assert.That(ex!.Message, Is.EqualTo("Cannot delete this photo."));
        }

        [Test]
        public void DeletePhotoAsync_ShouldThrow_WhenPhotoIsMain()
        {
            var photo = new Photo
            {
                Id = 1,
                Url = "http://test.com",
                IsMain = true,
                AppUserId = 2
            };

            var user = new AppUser
            {
                Id = 2,
                UserName = "reuf",
                KnownAs = "Reuf",
                Gender = "male",
                City = "Kakanj",
                Country = "Bosnia",
                Photos = new List<Photo> { photo }
            };

            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByUsernameAsync("reuf")).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.PhotosRepository.GetPhotoById(1)).ReturnsAsync(photo);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.DeletePhotoAsync("reuf", 1));

            Assert.That(ex!.Message, Is.EqualTo("Cannot delete this photo."));
        }

        [Test]
        public void DeletePhotoAsync_ShouldThrow_WhenSaveFails()
        {
            var photo = new Photo
            {
                Id = 1,
                Url = "http://test.com",
                PublicId = "cloud123",
                IsMain = false,
                AppUserId = 2
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

            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByUsernameAsync("testuser")).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.PhotosRepository.GetPhotoById(1)).ReturnsAsync(photo);
            _photoServiceMock.Setup(p => p.DeletePhotoAsync("cloud123")).ReturnsAsync(new DeletionResult { Result = "ok" });
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<Exception>(() =>
                _userService.DeletePhotoAsync("testuser", 1));

            Assert.That(ex!.Message, Is.EqualTo("Error deleting photo."));
        }
    }
}
