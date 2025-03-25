using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Extensions;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data.Extensions;

/// <summary>
/// Extensions to help with testing and creating test data for ProjectPortfolio.
/// </summary>
public static class ProjectPortfolioDataExtensions
{
    /// <summary>
    /// Adds programs, projects, and strategic initiatives to the portfolio.
    /// </summary>
    /// <param name="portfolio"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="programCount"></param>
    /// <param name="projectCount"></param>
    /// <param name="strategicInitiativeCount"></param>
    /// <returns></returns>
    public static ProjectPortfolio AddChildren(this ProjectPortfolio portfolio, TestingDateTimeProvider dateTimeProvider, int programCount = 2, int projectCount = 5, int strategicInitiativeCount = 3)
    {
        portfolio.AddPrograms(programCount, dateTimeProvider);
        portfolio.AddProjects(projectCount, dateTimeProvider);
        portfolio.AddStrategicThemes(strategicInitiativeCount, dateTimeProvider);

        return portfolio;
    }

    /// <summary>
    /// Adds programs to the portfolio.
    /// </summary>
    /// <param name="portfolio"></param>
    /// <param name="count"></param>
    /// <param name="dateTimeProvider"></param>
    /// <returns></returns>
    public static ProjectPortfolio AddPrograms(this ProjectPortfolio portfolio, int count, TestingDateTimeProvider dateTimeProvider)
    {
        if (count <= 0) return portfolio;

        var programFaker = new ProgramFaker();

        var programsList = GenericExtensions.GetPrivateHashSet<Program>(portfolio, "_programs");


        // TODO: add logic based on the current status of the portfolio
        for (int i = 0; i < count; i++)
        {
            var program = programFaker.AsActive(dateTimeProvider, portfolio.Id);
            programsList.Add(program);
        }

        return portfolio;
    }

    /// <summary>
    /// Adds projects to the portfolio.
    /// </summary>
    /// <param name="portfolio"></param>
    /// <param name="count"></param>
    /// <param name="dateTimeProvider"></param>
    /// <returns></returns>
    public static ProjectPortfolio AddProjects(this ProjectPortfolio portfolio, int count, TestingDateTimeProvider dateTimeProvider)
    {
        if (count <= 0) return portfolio;

        var projectFaker = new ProjectFaker();

        var projectsList = GenericExtensions.GetPrivateHashSet<Project>(portfolio, "_projects");

        // TODO: add logic based on the current status of the portfolio
        for (int i = 0; i < count - 1; i++)
        {
            var project = projectFaker.AsActive(dateTimeProvider, portfolio.Id);
            projectsList.Add(project);
        }

        // Add a proposed project as the last one
        var lastProject = projectFaker.AsProposed(dateTimeProvider, portfolio.Id);
        projectsList.Add(lastProject);

        return portfolio;
    }


    /// <summary>
    /// Adds strategic themes to the portfolio.
    /// </summary>
    /// <param name="portfolio"></param>
    /// <param name="count"></param>
    /// <param name="dateTimeProvider"></param>
    /// <returns></returns>
    public static ProjectPortfolio AddStrategicThemes(this ProjectPortfolio portfolio, int count, TestingDateTimeProvider dateTimeProvider)
    {
        if (count <= 0) return portfolio;

        var strategicInitiativeFaker = new StrategicInitiativeFaker(dateTimeProvider);

        var strategicInitiativesList = GenericExtensions.GetPrivateHashSet<StrategicInitiative>(portfolio, "_strategicInitiatives");

        // TODO: add logic based on the current status of the portfolio
        for (int i = 0; i < count - 1; i++)
        {
            var strategicInitiative = strategicInitiativeFaker.AsActive(dateTimeProvider, portfolio.Id);
            strategicInitiativesList.Add(strategicInitiative);
        }

        // Add a proposed strategic initiative as the last one
        var lastStrategicInitiative = strategicInitiativeFaker.AsProposed(dateTimeProvider, portfolio.Id);
        strategicInitiativesList.Add(lastStrategicInitiative);

        return portfolio;
    }
}
