using Domain.Entities;
using Domain.Enums;
using Domain.Errors;

namespace Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_AssignsPropertiesAndGeneratedId()
    {
        var result = User.Create("owner@example.com", "hashed-password", UserRole.Admin);

        Assert.True(result.IsSuccess);
        var user = result.Value!;
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("owner@example.com", user.Email);
        Assert.Equal("hashed-password", user.HashedPassword);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ReturnsEmailCannotBeEmptyError(string? email)
    {
        var result = User.Create(email!, "hashed-password", UserRole.Admin);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.EmailCannotBeEmpty, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPassword_ReturnsPasswordCannotBeEmptyError(string? password)
    {
        var result = User.Create("owner@example.com", password!, UserRole.Admin);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.PasswordCannotBeEmpty, result.Error);
    }
}
