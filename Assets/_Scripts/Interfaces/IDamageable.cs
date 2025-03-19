using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float damage);
    public bool CanParry();
    public bool IsParrying();
}
