using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float damage);
    public void SetInmune(bool inmune);
    public bool CanParry();
    public bool IsParrying();

}
