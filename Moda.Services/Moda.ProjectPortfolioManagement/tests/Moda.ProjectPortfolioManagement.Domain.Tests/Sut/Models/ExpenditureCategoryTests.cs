using FluentAssertions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public class ExpenditureCategoryTests
{
    private readonly ExpenditureCategoryFaker _faker;

    public ExpenditureCategoryTests()
    {
        _faker = new ExpenditureCategoryFaker();
    }

    #region Creation Tests

    [Fact]
    public void Create_ShouldCreateProposedCategorySuccessfully()
    {
        // Arrange
        var name = "Capex - IT Equipment";
        var description = "Capital expenditures for IT hardware.";
        var isCapitalizable = true;
        var requiresDepreciation = true;
        var accountingCode = "IT-1001";

        // Act
        var category = ExpenditureCategory.Create(name, description, isCapitalizable, requiresDepreciation, accountingCode);

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.State.Should().Be(ExpenditureCategoryState.Proposed);
        category.IsCapitalizable.Should().Be(isCapitalizable);
        category.RequiresDepreciation.Should().Be(requiresDepreciation);
        category.AccountingCode.Should().Be(accountingCode);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_ShouldUpdateAllFields_WhenProposed()
    {
        // Arrange
        var category = _faker.GenerateProposed();
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newIsCapitalizable = !category.IsCapitalizable;
        var newRequiresDepreciation = !category.RequiresDepreciation;
        var newAccountingCode = "Updated-AC-1001";

        // Act
        var result = category.Update(newName, newDescription, newIsCapitalizable, newRequiresDepreciation, newAccountingCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
        category.IsCapitalizable.Should().Be(newIsCapitalizable);
        category.RequiresDepreciation.Should().Be(newRequiresDepreciation);
        category.AccountingCode.Should().Be(newAccountingCode);
    }

    [Fact]
    public void Update_ShouldFail_WhenActive_IfCapitalizationChanges()
    {
        // Arrange
        var category = _faker.GenerateActive();
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newIsCapitalizable = !category.IsCapitalizable;
        var newRequiresDepreciation = category.RequiresDepreciation;
        var newAccountingCode = "Updated-AC-1001";

        // Act
        var result = category.Update(newName, newDescription, newIsCapitalizable, newRequiresDepreciation, newAccountingCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot change capitalizable or depreciation settings once the category is active.");
    }

    [Fact]
    public void Update_ShouldFail_WhenArchived()
    {
        // Arrange
        var category = _faker.GenerateArchived();
        var newName = "Updated Name";
        var newDescription = "Updated Description";

        // Act
        var result = category.Update(newName, newDescription, category.IsCapitalizable, category.RequiresDepreciation, category.AccountingCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot update an archived expenditure category.");
    }

    #endregion Update Tests

    #region Lifecycle Tests

    [Fact]
    public void Activate_ShouldSucceed_WhenProposed()
    {
        // Arrange
        var category = _faker.GenerateProposed();

        // Act
        var result = category.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.State.Should().Be(ExpenditureCategoryState.Active);
    }

    [Fact]
    public void Activate_ShouldFail_WhenAlreadyActive()
    {
        // Arrange
        var category = _faker.GenerateActive();

        // Act
        var result = category.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only proposed categories can be activated.");
    }

    [Fact]
    public void Archive_ShouldSucceed_WhenActive()
    {
        // Arrange
        var category = _faker.GenerateActive();

        // Act
        var result = category.Archive();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.State.Should().Be(ExpenditureCategoryState.Archived);
    }

    [Fact]
    public void Archive_ShouldFail_WhenNotActive()
    {
        // Arrange
        var category = _faker.GenerateProposed();

        // Act
        var result = category.Archive();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only active categories can be archived.");
    }

    #endregion Lifecycle Tests
}

