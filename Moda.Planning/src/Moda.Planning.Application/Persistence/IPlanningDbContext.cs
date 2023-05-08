﻿namespace Moda.Planning.Application.Persistence;
public interface IPlanningDbContext : IModaDbContext
{
    DbSet<ProgramIncrement> ProgramIncrements { get; }
    DbSet<Risk> Risks { get; }
}
