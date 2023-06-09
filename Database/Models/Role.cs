﻿using System;
using System.Collections.Generic;

namespace Starter_NET_7.Database.Models;

public partial class Role
{
    public int IdRole { get; set; }

    public string Name { get; set; } = null!;

    public bool Status { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreationDate { get; set; }

    public int? LastUpdateBy { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    public virtual ICollection<UnionPermissionsRole> UnionPermissionsRoles { get; set; } = new List<UnionPermissionsRole>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
