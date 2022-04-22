﻿using Microsoft.EntityFrameworkCore;
using NotesRepository.Data;
using Directory = NotesRepository.Data.Models.Directory;
namespace NotesRepository.Repositories

{
    public class DirectoryRepository : IDirectoryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;

        public DirectoryRepository(IDbContextFactory<ApplicationDbContext> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Adds a directory entity to the database
        /// </summary>
        /// <param name="directory">The directory entity</param>
        /// <returns>true if directory was successfully added; otherwise false</returns>
        public async Task<bool> AddDirectoryAsync(Directory directory)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                await ctx.Directories.AddAsync(directory);
                var result = await ctx.SaveChangesAsync();
                return result > 0;
            }
        }

        /// <summary>
        /// Attaches a subdirectory entity to the particular directory. 
        /// The subDirectory cannot have an assigned user while attaching it to the directory.
        /// </summary>
        /// <param name="subDirectory">The subdirectory entity</param>
        /// <param name="directoryId">The unique ID of directory</param>
        /// <returns>true if subdirectory was successfully added; otherwise false</returns>
        public async Task<bool> AttachSubDirectoryToParticularDirectoryAsync(Directory subDirectory, Guid directoryId)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                var dir = await ctx.Directories.FirstOrDefaultAsync(x => x.DirectoryId == directoryId);
                if (dir is not null)
                {
                    if (dir.SubDirectories is not null)
                        dir.SubDirectories.Add(subDirectory);
                    else
                        dir.SubDirectories = new List<Directory> { subDirectory };

                    ctx.Directories.Update(dir);    
                    var result = await ctx.SaveChangesAsync();
                    return result > 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Adds a subDirectory entity to the database
        /// </summary>
        /// <param name="subDirectory">The subdirectory entity</param>
        /// <returns>true if the subDirectory was successfully added; otherwise false</returns>
        public async Task<bool> AddSubDirectoryAsync(Directory subDirectory)
            => await AddDirectoryAsync(subDirectory);

        /// <summary>
        /// Removes multiple directory entities from the database
        /// </summary>
        /// <param name="directories">directory entities</param>
        /// <returns>true if directories were successfully removed; otherwise false</returns>
        public async Task<bool> DeleteDirectoriesAsync(ICollection<Directory> directories)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                ctx.Directories.RemoveRange(directories);
                var result = await ctx.SaveChangesAsync();
                return result > 0;
            }
        }

        /// <summary>
        /// Removes directory entity from the database
        /// </summary>
        /// <param name="directory">The directory entity</param>
        /// <returns>true if directory was successfully removed; otherwise false</returns>
        public async Task<bool> DeleteDirectoryAsync(Directory directory)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                ctx.Directories.Remove(directory);
                var result = await ctx.SaveChangesAsync();
                return result > 0;
            }
        }

        /// <summary>
        /// Removes a directory entity from the database by directoryId
        /// </summary>
        /// <param name="directoryId">The unique id of a directory</param>
        /// <returns>true if directory was successfully removed; otherwise false</returns>
        public async Task<bool> DeleteDirectoryByIdAsync(Guid directoryId)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                var directory = await ctx.Directories.FirstOrDefaultAsync(x => x.DirectoryId == directoryId);
                if (directory is not null)
                {
                    ctx.Directories.Remove(directory);
                    var result = await ctx.SaveChangesAsync();
                    return result > 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Removes a subdirectory entity for particular directory from the database
        /// </summary>
        /// <param name="subDirectoryId">The unique ID of a subdirectory</param>
        /// <param name="directoryId">The unique ID of a directory</param>
        /// <returns>true if subdirectory was successfully removed; otherwise false</returns>
        public async Task<bool> DeleteSubDirectoryByIdForParticularDirectoryAsync(Guid subDirectoryId, Guid directoryId)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                var directory = await GetDirectoryByIdAsync(directoryId);
                if (directory is not null)
                {
                    if (directory.SubDirectories is not null)
                    {
                        var subDirectory = directory.SubDirectories.Where(s => s.DirectoryId == subDirectoryId).SingleOrDefault();
                        if (subDirectory is not null)
                        {
                            ctx.Directories.Remove(subDirectory);
                            var result = await ctx.SaveChangesAsync();
                            return result > 0;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Removes all subdirectory entities for particular directory
        /// </summary>
        /// <param name="directoryId">The unique ID of a directory</param>
        /// <returns>true if all subdirectories were successfully removed; otherwise false</returns>
        public async Task<bool> DeleteAllSubDirectoriesForParticularDirectoryAsync(Guid directoryId)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                var directory = await GetDirectoryByIdAsync(directoryId);
                if (directory is not null)
                {
                    if (directory.SubDirectories is not null)
                    {
                        ctx.Directories.RemoveRange(directory.SubDirectories);
                        var result = await ctx.SaveChangesAsync();
                        return result > 0;
                    }
                }
                return false;
            }
        }

        /// <summary>
        ///  Gets all directories from the database
        /// </summary>
        /// <returns>A collection of directories currently stored in the database</returns>
        public async Task<ICollection<Directory>> GetAllDirectoriesAsync()
        {
            using (var ctx = _factory.CreateDbContext())
            {
                return await ctx.Directories
                    .Include(n => n.Notes)
                    .Include(s => s.SubDirectories)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Gets a directory entity from the database by directoryId
        /// </summary>
        /// <param name="directoryId">The unique id of a directory</param>
        /// <returns>A directory entity if it exists in the db; otherwise null</returns>
        public async Task<Directory?> GetDirectoryByIdAsync(Guid directoryId)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                return await ctx.Directories
                    .Include(n => n.Notes)
                    .Include(s => s.SubDirectories)
                    .FirstOrDefaultAsync(i => i.DirectoryId == directoryId);
            }
        }

        /// <summary>
        /// Gets a directory entity from the database by name
        /// </summary>
        /// <param name="name">The name of a directory</param>
        /// <returns>A directory entity if it exists in the db; otherwise null</returns>
        public async Task<Directory?> GetDirectoryByNameAsync(string name)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                return await ctx.Directories
                    .Include(n => n.Notes)
                    .Include(s => s.SubDirectories)
                    .FirstOrDefaultAsync(i => i.Name == name);
            }
        }

        /// <summary>
        /// Gets all directories from specific user
        /// </summary>
        /// <param name="userId">The unique ID of user</param>
        /// <returns>A collection of directories from specific user</returns>
        public async Task<ICollection<Directory>> GetAllDirectoriesForParticularUserAsync(string userId)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                return await ctx.Directories
                    .Where(u => u.User.Id == userId)
                    .Include(n => n.Notes)
                    .Include(s => s.SubDirectories)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Gets all subdirectories for specific directory
        /// </summary>
        /// <param name="directoryId">The unique ID of the directory</param>
        /// <returns>A collection of subdirectories for specific directory</returns>
        public async Task<ICollection<Directory>?> GetAllSubDirectoriesOfParticularDirectory(Guid directoryId)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                return await ctx.Directories
                    .Where(d => d.DirectoryId == directoryId)
                    .Include(s => s.SubDirectories)
                    .Select(d => d.SubDirectories)
                    .SingleOrDefaultAsync();
            }
        }

        /// <summary>
        /// Updates the directory entity in the database
        /// </summary>
        /// <param name="directory">The directory entity</param>
        /// <returns>True if directory was successfully updated; otherwise false</returns>
        public async Task<bool> UpdateDirectoryAsync(Directory directory)
        {
            using (var ctx = _factory.CreateDbContext())
            {
                ctx.Directories.Update(directory);
                var result = await ctx.SaveChangesAsync();
                return result > 0;
            }
        }

        /// <summary>
        /// Updates the subDirectory entity in the database
        /// </summary>
        /// <param name="subDirectory">The subDirectory entity</param>
        /// <returns>True if subDirectory was successfully updated; otherwise false</returns>
        public async Task<bool> UpdateSubDirectoryAsync(Directory subDirectory)
            => await UpdateDirectoryAsync(subDirectory);
    }
}
