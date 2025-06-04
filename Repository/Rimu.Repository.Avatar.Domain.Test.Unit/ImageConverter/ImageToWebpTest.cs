using FakeItEasy;
using FluentAssertions;
using ImageMagick;
using Rimu.Repository.Avatar.Domain.ImageConverter;
using Rimu.Repository.Environment.Adapter.Interface;
namespace Rimu.Repository.Avatar.Domain.Test.Unit.ImageConverter;

public class ImageToWebpTest {
    [Test]
    public void Convert_ShouldReturnWebpImage_Low_WhenValidImageBytesProvided() {
        // Arrange
        var envDb = A.Fake<IEnvDb>();
        A.CallTo(() => envDb.UserAvatar_ByteSizeLow).Returns(10_000);
        A.CallTo(() => envDb.UserAvatar_SizeLow).Returns(256);
        var imageToWebp = new ImageToWebp(envDb);
        byte[] imageBytes = File.ReadAllBytes(@"./TestFiles/animation.gif");

        // Act
        byte[] result = imageToWebp.ConvertWithLowSize(imageBytes);

        // Assertü
        (result.Length > 0).Should().BeTrue();
        
        System.IO.File.WriteAllBytes(@"C:\Users\schoppe\Downloads\a.webp", result);
    }
}