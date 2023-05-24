using System;
using System.Collections.Generic;

namespace Starter_NET_7.Database.Models;

public partial class UnionPermissionsUser
{
  public int PermissionId { get; set; }

  public int UserId { get; set; }

  public bool Status { get; set; }

  public int AssignedBy { get; set; }

  public DateTime AssignedDate { get; set; }

  public virtual Permission Permission { get; set; } = null!;

  public virtual User User { get; set; } = null!;
}
