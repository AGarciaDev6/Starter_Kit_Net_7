using System;
using System.Collections.Generic;

namespace Starter_NET_7.Database.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreationDate { get; set; }

    public int? LastUpdateBy { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public UserValidation UserValidation { get; set; } = null!;

    public virtual ICollection<UnionPermissionsUser> UnionPermissionsUsers { get; set; } = new List<UnionPermissionsUser>();
}
