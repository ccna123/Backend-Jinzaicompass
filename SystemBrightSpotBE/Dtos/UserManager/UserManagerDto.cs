public class UserManagerDto
{
	public long Id { get; set; }
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public long RoleId { get; set; }
	public long DepartmentId { get; set; }
	public long DivisionId { get; set; }
	public long GroupId { get; set; }
}