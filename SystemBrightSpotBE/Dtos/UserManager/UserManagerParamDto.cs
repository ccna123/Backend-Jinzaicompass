using System.Text.Json.Serialization;

public class UserManagerParamDto
{
	[JsonPropertyName("department_id")]
	public long DepartmentId { get; set; }

	[JsonPropertyName("division_id")]
	public long DivisionId { get; set; }

	[JsonPropertyName("group_id")]
	public long GroupId { get; set; }
}
