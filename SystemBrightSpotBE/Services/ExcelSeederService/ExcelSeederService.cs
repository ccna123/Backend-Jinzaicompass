using SystemBrightSpotBE.Models;
using log4net;
using OfficeOpenXml;
using Microsoft.AspNet.Identity;

namespace SystemBrightSpotBE.Services.ExcelSeederService
{
    public class ExcelSeederService : IExcelSeederService
    {
        private readonly ILog _log;
        private readonly DataContext _context;

        public ExcelSeederService(DataContext context)
        {
            _log = LogManager.GetLogger(typeof(ExcelSeederService));
            _context = context;
        }

        public Dictionary<string, List<(string Text, string Color)>> ReadExcelCategoryFile(string path)
        {
            ExcelPackage.License.SetNonCommercialPersonal("SystemBrightSpotBE");
            var result = new Dictionary<string, List<(string Text, string Color)>>();

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int columnCount = worksheet.Dimension.End.Column;
                int rowCount = worksheet.Dimension.End.Row;

                var tableNames = new List<string>();
                for (int col = 1; col <= columnCount; col++)
                {
                    var tableName = worksheet.Cells[1, col].Text;
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tableNames.Add(tableName);
                        result[tableName] = new List<(string Text, string Color)>();
                    }
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    for (int col = 1; col <= tableNames.Count; col++)
                    {
                        var text = worksheet.Cells[row, col].Text;
                        var color = worksheet.Cells[row, col].Style.Font.Color.Rgb?.ToString() ?? String.Empty;
                        if (!string.IsNullOrEmpty(text))
                        {
                            result[tableNames[col - 1]].Add((text, color));
                        }
                    }
                }
            }

            return result;
        }

        public async Task HandleImportExcelUserFileAsync(string path)
        {
            ExcelPackage.License.SetNonCommercialPersonal("SystemBrightSpotBE");

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int columnCount = worksheet.Dimension.End.Column;
                int rowCount = worksheet.Dimension.End.Row;
                PasswordHasher hasher = new PasswordHasher();

                for (int row = 2; row <= rowCount; row++)
                {
                    bool isEmpty = true;
                    for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                    {
                        if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                        {
                            isEmpty = false;
                            break;
                        }
                    }

                    if (isEmpty)
                    {
                        continue;
                    }

                    var roleId = _context.roles.Where(u => u.name == worksheet.Cells[row, 9].Text).Select(u => (long?)u.id).FirstOrDefault();
                    var departmentId = _context.departments.Where(u => u.name == worksheet.Cells[row, 10].Text).Select(u => (long?)u.id).FirstOrDefault();
                    var divisionId = _context.divisions.Where(u => u.name == worksheet.Cells[row, 11].Text).Select(u => (long?)u.id).FirstOrDefault();
                    var groupId = _context.groups.Where(u => u.name == worksheet.Cells[row, 12].Text).Select(u => (long?)u.id).FirstOrDefault();
                    var positionId = _context.positions.Where(u => u.name == worksheet.Cells[row, 13].Text).Select(u => (long?)u.id).FirstOrDefault();
                    //var occupationId = _context.occupations.Where(u => u.name == worksheet.Cells[row, 14].Text).Select(u => (long?)u.id).FirstOrDefault();
                    var employmentTypeId = _context.employment_types.Where(u => u.name == worksheet.Cells[row, 15].Text).Select(u => (long?)u.id).FirstOrDefault();

                    var user = new User
                    {
                        first_name = worksheet.Cells[row, 1].Text,
                        last_name = worksheet.Cells[row, 2].Text,
                        first_name_kana = worksheet.Cells[row, 3].Text,
                        last_name_kana = worksheet.Cells[row, 4].Text,
                        email = worksheet.Cells[row, 5].Text,
                        code = worksheet.Cells[row, 6].Text,
                        gender_id = long.TryParse(worksheet.Cells[row, 7].Text, out var genderId) ? genderId : null,
                        date_of_birth = DateOnly.TryParse(worksheet.Cells[row, 8].Text, out var dob) ? dob : null,
                        role_id = roleId,
                        department_id = departmentId,
                        division_id = divisionId,
                        group_id = groupId,
                        position_id = positionId,
                        //occupation_id = occupationId,
                        employment_type_id = employmentTypeId,
                        phone = string.IsNullOrWhiteSpace(worksheet.Cells[row, 16].Text) ? null : worksheet.Cells[row, 16].Text,
                        address = string.IsNullOrWhiteSpace(worksheet.Cells[row, 17].Text) ? null : worksheet.Cells[row, 17].Text,
                        nearest_station = string.IsNullOrWhiteSpace(worksheet.Cells[row, 18].Text) ? null : worksheet.Cells[row, 18].Text,
                        password = hasher.HashPassword(worksheet.Cells[row, 19].Text),
                        active = true,
                        temp_password_used = false
                    };
                    _context.users.Add(user);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task SeedDataFromExcelAsync(string pathCategory, string pathUser)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dataCategory = ReadExcelCategoryFile(pathCategory);

                //if (dataCategory.ContainsKey("department"))
                //{
                //    foreach (var record in dataCategory["department"])
                //    {
                //        var department = new Department
                //        {
                //            name = record.Text,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.departments.Add(department);
                //    }
                //    await _context.SaveChangesAsync();
                //}

                //if (dataCategory.ContainsKey("division"))
                //{
                //    foreach (var record in dataCategory["division"])
                //    {
                //        var division = new Division
                //        {
                //            name = record.Text,
                //            department_id = 1,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.divisions.Add(division);
                //    }
                //    await _context.SaveChangesAsync();
                //}

                //if (dataCategory.ContainsKey("group"))
                //{
                //    foreach (var record in dataCategory["group"])
                //    {
                //        var group = new Group
                //        {
                //            name = record.Text,
                //            division_id = 1,
                //            department_id = 1,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.groups.Add(group);
                //    }
                //    await _context.SaveChangesAsync();
                //}

                //if (dataCategory.ContainsKey("experience-job"))
                //{
                //    foreach (var record in dataCategory["experience-job"])
                //    {
                //        var experienceJob = new ExperienceJob
                //        {
                //            name = record.Text,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.experience_jobs.Add(experienceJob);
                //    }
                //    await _context.SaveChangesAsync();
                //}

                //if (dataCategory.ContainsKey("experience-field"))
                //{
                //    foreach (var record in dataCategory["experience-field"])
                //    {
                //        var experienceField = new ExperienceField
                //        {
                //            name = record.Text,
                //            experience_job_id = 1,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.experience_fields.Add(experienceField);
                //    }
                //    await _context.SaveChangesAsync();
                //}

                //if (dataCategory.ContainsKey("experience-area"))
                //{
                //    foreach (var record in dataCategory["experience-area"])
                //    {
                //        var experienceArea = new ExperienceArea
                //        {
                //            name = record.Text,
                //            experience_field_id = 1,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.experience_areas.Add(experienceArea);
                //    }
                //    await _context.SaveChangesAsync();
                //}

                //if (dataCategory.ContainsKey("specific-skill"))
                //{
                //    foreach (var record in dataCategory["specific-skill"])
                //    {
                //        var specificSkill = new SpecificSkill
                //        {
                //            name = record.Text,
                //            experience_area_id = 1,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.specific_skills.Add(specificSkill);
                //    }
                //    await _context.SaveChangesAsync();
                //}

                foreach (var entry in dataCategory)
                {
                    var tableName = entry.Key;
                    var records = entry.Value;
                    var skipTable = new List<string> { "departmnet", "division", "group", "experience-job", "experience-field", "experience-area", "specific-skill" };
                    if (skipTable.Contains(tableName))
                    {
                        continue;
                    }

                    foreach (var record in records)
                    {
                        var text = record.Text;
                        var color = record.Color;
                        switch (tableName.ToLower())
                        {
                            //case "certification":
                            //    var certification = new Certification
                            //    {
                            //        name = text,
                            //        delete_flag = !string.IsNullOrEmpty(color)
                            //    };
                            //    _context.certifications.Add(certification);
                            //    break;

                            //case "company-award":
                            //    var companyAward = new CompanyAward
                            //    {
                            //        name = text,
                            //        delete_flag = !string.IsNullOrEmpty(color)
                            //    };
                            //    _context.company_awards.Add(companyAward);
                            //    break;

                            //case "position":
                            //    var position = new Position
                            //    {
                            //        name = text,
                            //        delete_flag = !string.IsNullOrEmpty(color)
                            //    };
                            //    _context.positions.Add(position);
                            //    break;

                            //case "employment-type":
                            //    var employmentType = new EmploymentType
                            //    {
                            //        name = text,
                            //        delete_flag = !string.IsNullOrEmpty(color)
                            //    };
                            //    _context.employment_types.Add(employmentType);
                            //    break;

                            case "employment-status":
                                var employmentStatus = new EmploymentStatus
                                {
                                    name = text
                                };
                                _context.employment_status.Add(employmentStatus);
                                break;

                            case "gender":
                                var gender = new Gender
                                {
                                    name = text
                                };
                                _context.genders.Add(gender);
                                break;

                            case "role":
                                var role = new Role
                                {
                                    name = text
                                };
                                _context.roles.Add(role);
                                break;

                            //case "participation-process":
                            //    var participationProcess = new ParticipationProcess
                            //    {
                            //        name = text,
                            //        delete_flag = !string.IsNullOrEmpty(color)
                            //    };
                            //    _context.participation_processes.Add(participationProcess);
                            //    break;

                            //case "participation-position":
                            //    var participationPosition = new ParticipationPosition
                            //    {
                            //        name = text,
                            //        delete_flag = !string.IsNullOrEmpty(color)
                            //    };
                            //    _context.participation_positions.Add(participationPosition);
                            //    break;

                            //case "report-type":
                            //    var reportType = new ReportType
                            //    {
                            //        name = text,
                            //        delete_flag = !string.IsNullOrEmpty(color)
                            //    };
                            //    _context.report_types.Add(reportType);
                            //    break;
                            default:
                                break;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _log.Info($"Data seeded successfully at path: {pathCategory}");

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Error during seeding: {ex.Message}");
                throw;
            }

            using var transactionUser = await _context.Database.BeginTransactionAsync();
            try
            {
                await HandleImportExcelUserFileAsync(pathUser);
                _log.Info($"Data seeded successfully at path: {pathUser}");
                await transactionUser.CommitAsync();
            }
            catch (Exception ex)
            {
                await transactionUser.RollbackAsync();
                _log.Error($"Error during seeding: {ex.Message}");
                throw;
            }
        }
    }
}
