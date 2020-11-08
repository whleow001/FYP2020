using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage
{
    public int damage;
    public Vector3 sourcePosition;

    public Damage(int _damage, Vector3 _sourcePosition) {
      damage = _damage;
      sourcePosition = _sourcePosition;
    }
}
