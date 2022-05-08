﻿using Microsoft.AspNetCore.Identity;
using NotesRepository.Areas.Identity.Data;
using NotesRepository.Data.Models;
using NotesRepository.Repositories;
using NotesRepository.Services;
using Directory = NotesRepository.Data.Models.Directory;

namespace NotesRepository.Data
{
    public static class DefaultEntitiesSeeder
    {
        /// <summary>
        /// This method is responsible for seeding default directories, notes etc. for one particular user - admin@admin.com
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void SeedDefaultEntities(this IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetService<ApplicationDbContext>();
            var ns = new NoteService(new NoteRepository(context), new UserRepository(context), new EventRepository(context), new DirectoryRepository(context), new ImageRepository(context));

            // seed Admin
            var user = new ApplicationUser
            {
                Id = "ba6c78a4-9cff-4bb1-acbd-f4c23a063616",
                Email = "admin@admin.com",
                NormalizedEmail = "ADMIN@ADMIN.COM",
                UserName = "admin@admin.com",
                NormalizedUserName = "ADMIN@ADMIN.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // seed default directory
            var defDir = new Directory
            {
                DirectoryId = Guid.Parse("5357fe5b-e0c5-40a3-98be-6776844dbaf7"),
                Name = "Default",
                User = user
            };

            //seed bin directory
            var defBinDir = new Directory
            {
                DirectoryId = Guid.Parse("e7ce7d86-314c-4ab6-a33e-4c6349b5a1cc"),
                Name = "Bin",
                User = user
            };

            //seed subdirectories for default directory
            var subDirsOfDefault = new List<Directory>
            {
                new Directory { DirectoryId = Guid.Parse("1125c320-d885-4d32-a8cb-2f11fe1b64ef"), Name = "SubDirOfDefault_1", ParentDir = defDir, User = user },
                new Directory
                {
                    DirectoryId = Guid.Parse("8e069728-754f-4580-8426-9d6f160ea952"),
                    Name = "SubDirOfDefault_2",
                    ParentDir = defDir,
                    User = user,
                    SubDirectories = new List<Directory> { new Directory { DirectoryId = Guid.Parse("a3c77e40-6b71-4776-ba00-08f128be6018"), Name = "SubSubDirOfDefault2_1", User = user },
                        new Directory { DirectoryId = Guid.Parse("f8622d56-a8b9-4132-8a7c-133adbbece18"), Name = "SubSubDirOfDefault2_2", User = user } }
                },

            };

            //seed custom directory with subdirectories
            var customDirWithSubDirs = new Directory
            {
                DirectoryId = Guid.Parse("ec3720dd-8d33-406d-b3f5-cdc70d5a9955"),
                Name = "CustomDir",
                User = user,
                SubDirectories = new List<Directory> { new Directory { DirectoryId = Guid.Parse("6a5931d2-8457-4804-bf44-b6548f00ebdf"), Name = "SubDirOfCustom_1", User = user },
                    new Directory { DirectoryId = Guid.Parse("9833eefb-b902-4d4e-93d8-bc760325e576"), Name = "SubDirOfCustom_2", User = user } }
            };

            if (!context.Users.Any(x => x.Id == user.Id))
            {
                user.HashPassword("Admin123!");
                context.Users.Add(user);
                context.SaveChanges();
            }

            if (!context.Directories.Any(x => x.DirectoryId == defDir.DirectoryId))
            {
                context.Directories.Add(defDir);
                context.SaveChanges();
            }

            if (!context.Directories.Any(x => x.DirectoryId == defBinDir.DirectoryId))
            {
                context.Directories.Add(defBinDir);
                context.SaveChanges();
            }

            if (!context.Directories.Any(x => x.DirectoryId == customDirWithSubDirs.DirectoryId))
            {
                context.Directories.Add(customDirWithSubDirs);
                context.SaveChanges();
            }

            if (!context.Directories.Any(x => x.DirectoryId == subDirsOfDefault.ElementAt(0).DirectoryId) &&
               !context.Directories.Any(x => x.DirectoryId == subDirsOfDefault.ElementAt(1).DirectoryId) &&
               !context.Directories.Any(x => x.DirectoryId == subDirsOfDefault.ElementAt(1).SubDirectories!.ElementAt(0).DirectoryId) &&
               !context.Directories.Any(x => x.DirectoryId == subDirsOfDefault.ElementAt(1).SubDirectories!.ElementAt(1).DirectoryId))
            {
                context.Directories.AddRange(subDirsOfDefault);
                context.SaveChanges();
            }

            if (!context.Notes.Any(x => x.NoteId == Guid.Parse("9c755080-ba6b-48f1-854d-1816ad5fa74a")))
            {
                var note = new Note
                {
                    NoteId = Guid.Parse("9c755080-ba6b-48f1-854d-1816ad5fa74a"),
                    Title = "Password list",
                    Content = "Note under default directory",
                    IconName = "",
                    CreatedAt = new DateTime(2019, 12, 24, 4, 20, 0),
                    Owner = user,
                    Directory = defDir
                };
                context.Notes.Add(note);
                context.SaveChanges();
            }

            if (!context.Notes.Any(x => x.NoteId == Guid.Parse("0b2f78d3-d292-462e-a889-dd5c541f131b")))
            {
                var note = new Note
                {
                    NoteId = Guid.Parse("0b2f78d3-d292-462e-a889-dd5c541f131b"),
                    Title = "Trips I want to take",
                    Content = "Note under custom directory",
                    IconName = "",
                    CreatedAt = new DateTime(2021, 4, 12, 5, 12, 0),
                    Owner = user,
                    Directory = customDirWithSubDirs
                };
                context.Notes.Add(note);
                context.SaveChanges();
            }

            if (!context.Notes.Any(x => x.NoteId == Guid.Parse("9bb2e297-6a70-4517-82b5-7fca43550c3e")))
            {
                var note = new Note
                {
                    NoteId = Guid.Parse("9bb2e297-6a70-4517-82b5-7fca43550c3e"),
                    Title = "Sicilia monuments",
                    Content = "Note under subdirectory_1 of custom dir",
                    IconName = "",
                    CreatedAt = new DateTime(2018, 6, 1, 2, 2, 0),
                    Owner = user,
                    Directory = customDirWithSubDirs.SubDirectories.ElementAt(0)
                };
                context.Notes.Add(note);
                context.SaveChanges();
            }

            if (!context.Notes.Any(x => x.NoteId == Guid.Parse("8e37c761-b4af-40bd-b2aa-f260a9b5d5b7")))
            {
                var note = new Note
                {
                    NoteId = Guid.Parse("8e37c761-b4af-40bd-b2aa-f260a9b5d5b7"),
                    Title = "Madeira monuments",
                    Content = "Note under subdirectory_2 of custom dir",
                    IconName = "",
                    CreatedAt = new DateTime(2019, 9, 5, 12, 44, 0),
                    Owner = user,
                    Directory = customDirWithSubDirs.SubDirectories.ElementAt(1)
                };
                context.Notes.Add(note);
                context.SaveChanges();
            }

            if (!context.Notes.Any(x => x.NoteId == Guid.Parse("8d44780e-592d-4003-a271-69c186653dda")))
            {
                var note = new Note
                {
                    NoteId = Guid.Parse("8d44780e-592d-4003-a271-69c186653dda"),
                    Title = "Fast cars",
                    Content = "Note under subdirectory_2 of default dir",
                    IconName = "",
                    CreatedAt = new DateTime(2019, 9, 5, 12, 44, 0),
                    Owner = user,
                    Directory = subDirsOfDefault.ElementAt(1)
                };
                context.Notes.Add(note);
                context.SaveChanges();
            }

            if (!context.Notes.Any(x => x.NoteId == Guid.Parse("1e3a6c05-18d5-4b16-a546-b28f243e0f72")))
            {
                var note = new Note
                {
                    NoteId = Guid.Parse("1e3a6c05-18d5-4b16-a546-b28f243e0f72"),
                    Title = "BMW M4",
                    Content = "Note under subsubdirectory_1 of default dir",
                    IconName = "",
                    CreatedAt = new DateTime(2022, 2, 1, 5, 23, 0),
                    Owner = user,
                    Directory = subDirsOfDefault.ElementAt(1).SubDirectories.ElementAt(0)
                };
                context.Notes.Add(note);
                context.SaveChanges();
            }

            if (!context.Notes.Any(x => x.NoteId == Guid.Parse("a6fd2e45-df97-4de1-8119-76a6191fb690")))
            {
                var note = new Note
                {
                    NoteId = Guid.Parse("a6fd2e45-df97-4de1-8119-76a6191fb690"),
                    Title = "Audi RS6",
                    Content = "Note under subsubdirectory_2 of default dir",
                    IconName = "",
                    CreatedAt = new DateTime(2017, 11, 12, 2, 11, 0),
                    Owner = user,
                    Directory = subDirsOfDefault.ElementAt(1).SubDirectories.ElementAt(1)
                };
                context.Notes.Add(note);
                context.SaveChanges();
            }

            if (!context.Notes.Any(x => x.NoteId == Guid.Parse("a6fd2e45-df97-4de1-8119-76a6191fb690")))
            {
                var note = new Note
                {
                    NoteId = Guid.Parse("a6fd2e45-df97-4de1-8119-76a6191fb690"),
                    Title = "Audi SQ7",
                    Content = "Note under subsubdirectory_2 of default dir",
                    IconName = "",
                    CreatedAt = new DateTime(2019, 2, 12, 9, 43, 0),
                    Owner = user,
                    Directory = subDirsOfDefault.ElementAt(1).SubDirectories.ElementAt(1)
                };
                context.Notes.Add(note);
                context.SaveChanges();
            }
        }

        private static void HashPassword(this ApplicationUser user, string psswd = "Password123!")
        {
            var password = new PasswordHasher<ApplicationUser>();
            var hashed = password.HashPassword(user, psswd);
            user.PasswordHash = hashed;
        }
    }
}
